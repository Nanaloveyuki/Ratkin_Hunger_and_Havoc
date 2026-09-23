extern alias iris;
using System;
using System.Collections.Generic;
using System.Linq;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Generation;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Narrative;
using iris::IrisMenus;
using RimWorld;
using UnityEngine;
using Verse;

namespace HungerAndHavoc.Pawn.Compat
{
    internal static class RHAH_IrisMenusCompat
    {
        internal const string PackageId = "Nanaloveyuki.IrisMenus";
        internal const string SupportedVersion = "1.6";

        static bool registered;

        internal static void TryRegister(RHAH_Mod owner)
        {
            if (registered || owner == null)
            {
                return;
            }

            ModMetaData meta = ModLister.GetActiveModWithIdentifier(PackageId, false);
            if (meta == null)
            {
                Log.Message("[RHAH] IrisMenus is not active. Menu pages were not registered.");
                return;
            }

            string version = string.IsNullOrEmpty(meta.ModVersion) ? "unspecified" : meta.ModVersion;
            bool supported = meta.SupportedVersionsReadOnly != null &&
                meta.SupportedVersionsReadOnly.Any(item => item != null && item.Major == 1 && item.Minor == 6);
            if (!supported || (!string.IsNullOrEmpty(meta.ModVersion) &&
                !string.Equals(meta.ModVersion, SupportedVersion, StringComparison.Ordinal)))
            {
                Log.Warning("[RHAH] IrisMenus version " + version + " does not match supported " +
                    SupportedVersion + ". Menu pages were not registered.");
                return;
            }

            try
            {
                new RHAH_IrisMenusPages(version).Register(owner);
                registered = true;
                Log.Message("[RHAH] IrisMenus " + version + " pages registered.");
            }
            catch (Exception exception)
            {
                Log.Error("[RHAH] IrisMenus registration failed. The mod will keep loading without menu pages.\n" +
                    exception);
            }
        }

        internal static void ResetForTests()
        {
            registered = false;
        }
    }

    internal sealed class RHAH_IrisMenusPages
    {
        const float ControlRow = 34f;
        const float IncidentMetaHeight = 42f;
        const float IncidentButtonHeight = 28f;

        readonly string irisVersion;
        readonly Dictionary<string, string> debugResults = new Dictionary<string, string>();
        readonly Dictionary<string, string> pointBuffers = new Dictionary<string, string>();
        readonly Dictionary<string, string> weightBuffers = new Dictionary<string, string>();
        string foodQuery = string.Empty;
        string pendingFoodMod;
        readonly HashSet<string> collapsedFoodMods = new HashSet<string>();

        string selectedPawnLabel = string.Empty;

        internal RHAH_IrisMenusPages(string irisVersion)
        {
            this.irisVersion = irisVersion;
        }

        internal void Register(Mod owner)
        {
            RegisterPage(owner, "overview", "RHAH_Menu_Overview", DrawOverview, SearchOverview);
            RegisterPage(owner, "relief", "RHAH_Menu_Relief", DrawRelief, SearchRelief);
            RegisterPage(owner, "visitors", "RHAH_Menu_Visitors", DrawVisitors, SearchVisitors);
            RegisterPage(owner, "plague", "RHAH_Menu_Plague", DrawPlague, SearchPlague);
            RegisterPage(owner, "events", "RHAH_Menu_Events", DrawEvents, SearchCatalogEvents);
            RegisterPage(owner, "pawns", "RHAH_Menu_Pawns", DrawPawns, SearchPawns);
            RegisterPage(owner, "narrative", "RHAH_Menu_Narrative", DrawNarrative, SearchNarrative);
            RegisterPage(owner, "other", "RHAH_Menu_Other", DrawOther, SearchOther);
            RegisterPage(owner, "compat-diagnostics", "RHAH_Menu_CompatDiagnostics", DrawCompatDiagnostics, SearchCompatDiagnostics);
            RegisterPage(owner, "ending", "RHAH_Menu_Ending", DrawEnding, SearchEnding);
            RegisterPage(owner, "genes", "RHAH_Menu_Genes", DrawGenes, SearchGenes);
            RegisterPage(owner, "dev-events", "RHAH_Menu_DevEvents", DrawDevEvents, SearchDevEvents);
            RegisterPage(owner, "event-frequency", "RHAH_Menu_EventFrequency", DrawFrequency, SearchFrequency);
            RegisterPage(owner, "pawn-history", "RHAH_Menu_PawnHistory", DrawPawnHistory, SearchPawnHistory);
            RegisterPage(owner, "developer", "RHAH_Menu_Developer", DrawDeveloper, SearchDeveloper);
            RegisterPage(owner, "experimental", "RHAH_Menu_Experimental", DrawExperimental, SearchExperimental);
        }

        static void RegisterPage(
            Mod owner,
            string pageId,
            string titleKey,
            Action<Listing_Standard> draw,
            Func<IEnumerable<MenuSearchEntry>> search)
        {
            MenuRegistry.RegisterSubItemListing(owner, pageId, () => titleKey.Translate(), draw);
            MenuRegistry.RegisterSearchProvider(owner, pageId, search);
        }

        void DrawOverview(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Overview");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Anchor(list, "enable-new-content");
            MenuControls.Checkbox(
                list,
                "RHAH_Settings_EnableNewContent".Translate(),
                ref settings.enableNewContent,
                "RHAH_Settings_EnableNewContent_Tooltip".Translate());
        }

        IEnumerable<MenuSearchEntry> SearchOverview()
        {
            yield return Entry("enable-new-content", "RHAH_Settings_EnableNewContent", "toggle content");
        }

        void DrawEvents(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Events");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                DrawIncident(list, entries[i], false, settings);
            }

            settings.Write();
        }

        void DrawDevEvents(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_DevEvents");
            RHAH_Settings settings = RHAH_Mod.Settings;
            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                DrawIncident(list, entries[i], true, settings);
            }
            if (settings != null)
            {
                settings.Write();
            }
        }

        void DrawIncident(Listing_Standard list, RHAH_IncidentEntry entry, bool debug, RHAH_Settings settings)
        {
            string anchor = (debug ? "dev-" : "event-") + entry.DisplayId;
            float height = IncidentMetaHeight + RHAH_IrisMenusWidgets.CardPad * 2f;
            if (debug)
            {
                height += IncidentButtonHeight;
            }

            if (settings != null)
            {
                height += ControlRow * (entry.DisplayId == "I-051" ? 6f : 3f);
            }

            Rect inner = RHAH_IrisMenusWidgets.Card(list, anchor, height);
            Widgets.Label(new Rect(inner.x, inner.y, inner.width, 22f),
                entry.DisplayId + "  " + entry.LabelKey.Translate());
            Widgets.Label(new Rect(inner.x, inner.y + 20f, inner.width, 22f),
                "RHAH_Menu_EventMeta".Translate(
                    FamilyLabel(entry.Family),
                    OriginLabel(entry.Origin),
                    CategoryLabel(entry.Category),
                    TargetLabel(entry.Target),
                    entry.BroadcastEligible ? "RHAH_Menu_Yes".Translate() : "RHAH_Menu_No".Translate()));
            float cursor = inner.y + IncidentMetaHeight;
            if (debug)
            {
                Rect button = new Rect(inner.x, cursor, 160f, 24f);
                if (Widgets.ButtonText(button, "RHAH_Menu_Trigger".Translate()))
                {
                    debugResults[entry.DisplayId] = Queue(entry);
                }

                string result;
                if (debugResults.TryGetValue(entry.DisplayId, out result))
                {
                    Widgets.Label(new Rect(button.xMax + 8f, button.y, inner.width - 168f, 24f), result);
                }

                cursor += IncidentButtonHeight;
            }

            if (settings != null)
            {
                DrawIncidentControls(inner, cursor, entry, settings);
            }
        }

        void DrawIncidentControls(Rect inner, float y, RHAH_IncidentEntry entry, RHAH_Settings settings)
        {
            bool enabled = settings.IsIncidentEnabled(entry.DisplayId);
            Widgets.CheckboxLabeled(
                new Rect(inner.x, y, inner.width, 24f),
                "RHAH_Menu_EventEnabled".Translate(),
                ref enabled);
            settings.SetIncidentEnabled(entry.DisplayId, enabled);
            y += ControlRow;

            float points = settings.IncidentDebugPoints(entry.DisplayId, entry.DebugPoints);
            string pointBuffer = Buffer(pointBuffers, entry.DisplayId, points, "0");
            points = RHAH_IrisMenusWidgets.TunedValue(
                new Rect(inner.x, y, inner.width, 30f),
                "RHAH_Menu_DebugPoints".Translate(),
                points,
                ref pointBuffer,
                RHAH_IncidentTuning.MinDebugPoints,
                RHAH_IncidentTuning.MaxDebugPoints,
                "0",
                "RHAH_Menu_DebugPoints_Tip".Translate());
            pointBuffers[entry.DisplayId] = pointBuffer;
            settings.SetIncidentDebugPoints(entry.DisplayId, points);
            y += ControlRow;

            float weight = settings.IncidentWeight(entry.DisplayId);
            string weightBuffer = Buffer(weightBuffers, entry.DisplayId, weight, "0");
            weight = RHAH_IrisMenusWidgets.TunedValue(
                new Rect(inner.x, y, inner.width, 30f),
                "RHAH_Menu_IncidentWeight".Translate(),
                weight,
                ref weightBuffer,
                RHAH_IncidentTuning.MinWeight,
                RHAH_IncidentTuning.MaxWeight,
                "0",
                "RHAH_Menu_IncidentWeight_Tip".Translate());
            weightBuffers[entry.DisplayId] = weightBuffer;
            settings.SetIncidentWeight(entry.DisplayId, weight);
            if (entry.DisplayId != "I-051")
            {
                return;
            }

            y += ControlRow;
            string chanceBuffer = Buffer(weightBuffers, "predation-chance", settings.refugeePredationChancePercent, "0");
            settings.refugeePredationChancePercent = RHAH_IrisMenusWidgets.TunedValue(
                new Rect(inner.x, y, inner.width, 30f),
                "RHAH_Settings_PredationChance".Translate(settings.refugeePredationChancePercent.ToString("0")),
                settings.refugeePredationChancePercent,
                ref chanceBuffer,
                0f,
                100f,
                "0",
                "RHAH_Settings_PredationChance_Tooltip".Translate());
            weightBuffers["predation-chance"] = chanceBuffer;
            y += ControlRow;
            Widgets.CheckboxLabeled(new Rect(inner.x, y, inner.width, 24f), "RHAH_Settings_PredationFightBack".Translate(), ref settings.refugeePredationFightBack);
            TooltipHandler.TipRegion(new Rect(inner.x, y, inner.width, 24f), "RHAH_Settings_PredationFightBack_Tooltip".Translate());
            y += ControlRow;
            Widgets.CheckboxLabeled(new Rect(inner.x, y, inner.width, 24f), "RHAH_Settings_PredationFollowDifficulty".Translate(), ref settings.outsidePredatorsFollowDifficulty);
            TooltipHandler.TipRegion(new Rect(inner.x, y, inner.width, 24f), "RHAH_Settings_PredationFollowDifficulty_Tooltip".Translate());
        }

        static string Buffer(Dictionary<string, string> buffers, string id, float value, string format)
        {
            string buffer;
            if (buffers.TryGetValue(id, out buffer))
            {
                return buffer;
            }

            return value.ToString(format);
        }

        static string Queue(RHAH_IncidentEntry entry)
        {
            if (!RHAH_Runtime.AllowsNewContent)
            {
                return "RHAH_Menu_Queue_Disabled".Translate();
            }

            if (Current.Game == null)
            {
                return "RHAH_Menu_Queue_NoGame".Translate();
            }

            if (entry.Target == RHAH_IncidentTarget.Map && RHAH_MapResolver.Resolve() == null)
            {
                return "RHAH_Menu_Queue_NoMap".Translate();
            }

            if (entry.Target == RHAH_IncidentTarget.Caravan &&
                Caravan.CaravanTargetResolver.ResolvePlayerCaravan() == null)
            {
                return "RHAH_Menu_Queue_NoCaravan".Translate();
            }

            if (RHAH_Scheduler.QueueDebugIncident(entry.DisplayId))
            {
                return "RHAH_Menu_Queue_Queued".Translate();
            }

            return "RHAH_Menu_Queue_Failed".Translate();
        }

        static IEnumerable<MenuSearchEntry> SearchCatalogEvents()
        {
            return SearchIncidentEntries("event-", string.Empty);
        }

        static IEnumerable<MenuSearchEntry> SearchDevEvents()
        {
            return SearchIncidentEntries("dev-", "debug ");
        }

        static IEnumerable<MenuSearchEntry> SearchIncidentEntries(string anchorPrefix, string keywordPrefix)
        {
            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                RHAH_IncidentEntry entry = entries[i];
                string keywords = keywordPrefix + entry.DefName + " " + entry.DisplayId;
                yield return new MenuSearchEntry(
                    anchorPrefix + entry.DisplayId,
                    () => entry.DisplayId + " " + entry.LabelKey.Translate(),
                    () => keywords,
                    () => entry.Family + " " + entry.Category);
            }
        }

        void DrawPawns(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Pawns");
            Map map = Find.CurrentMap;
            if (map == null || map.mapPawns == null)
            {
                Empty(list, "RHAH_Menu_Pawns_NoMap");
                return;
            }

            IReadOnlyList<Verse.Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            bool any = false;
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (pawn == null || pawn.Destroyed || !RHAH_Api.IsOrigin(pawn))
                {
                    continue;
                }

                any = true;
                DrawPawn(list, pawn, RHAH_Api.Get(pawn));
            }

            if (!any)
            {
                Empty(list, "RHAH_Menu_Pawns_None");
            }
        }

        void DrawPawn(Listing_Standard list, Verse.Pawn pawn, IRHAH_Pawn snapshot)
        {
            if (pawn == null || pawn.Destroyed || snapshot == null)
            {
                Empty(list, "RHAH_Menu_Pawns_Invalid");
                return;
            }

            string anchor = "pawn-" + pawn.thingIDNumber;
            Rect inner = RHAH_IrisMenusWidgets.Card(list, anchor, 68f);
            string name = pawn.LabelShort;
            Widgets.Label(new Rect(inner.x, inner.y, inner.width, 22f), name);
            Widgets.Label(new Rect(inner.x, inner.y + 20f, inner.width, 22f),
                "RHAH_Menu_PawnMeta".Translate(
                    RoleLabel(snapshot.Role),
                    LifecycleLabel(snapshot.Lifecycle),
                    snapshot.SourceIncidentDisplayId ?? "RHAH_Menu_None".Translate()));
            Widgets.Label(new Rect(inner.x, inner.y + 40f, inner.width, 22f),
                "RHAH_Menu_PawnRelation".Translate(
                    snapshot.RelationshipGroupId,
                    snapshot.ParentPawnLoadId,
                    snapshot.ChildPawnLoadIds == null ? 0 : snapshot.ChildPawnLoadIds.Count,
                    snapshot.HasBeenFed ? "RHAH_Menu_Yes".Translate() : "RHAH_Menu_No".Translate(),
                    snapshot.CarriesPlague ? "RHAH_Menu_Yes".Translate() : "RHAH_Menu_No".Translate()));
            selectedPawnLabel = name;
        }

        IEnumerable<MenuSearchEntry> SearchPawns()
        {
            yield return Entry("pawns-list", "RHAH_Menu_Pawns");
        }

        void DrawNarrative(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Narrative");
            if (Current.Game == null)
            {
                Empty(list, "RHAH_Menu_Narrative_NoGame");
                return;
            }

            NarrativeState state = Current.Game.GetComponent<NarrativeState>();
            if (state == null)
            {
                Empty(list, "RHAH_Menu_Unavailable");
                return;
            }

            NarrativeSnapshot snapshot = state.Snapshot();
            if (snapshot == null)
            {
                Empty(list, "RHAH_Menu_Unavailable");
                return;
            }

            Status(list, "RHAH_Menu_Narrative_Revealed", snapshot.RevealedCount.ToString());
            Status(list, "RHAH_Menu_Narrative_Trust", snapshot.Trust.ToString());
            Status(list, "RHAH_Menu_Narrative_Rescued", snapshot.Rescued.ToString());
            Status(list, "RHAH_Menu_Narrative_Lost", snapshot.Lost.ToString());
            Status(list, "RHAH_Menu_Narrative_Failed",
                snapshot.Failed ? "RHAH_Menu_Yes".Translate() : "RHAH_Menu_No".Translate());
            Status(list, "RHAH_Menu_Narrative_Outcome", OutcomeLabel(state.CalculateOutcome()));
            Section(list, "RHAH_Menu_Narrative_Nodes");
            DrawNode(list, state, SuiyinNode.N001, "N-001");
            DrawNode(list, state, SuiyinNode.N002, "N-002");
            DrawNode(list, state, SuiyinNode.N003, "N-003");
            DrawNode(list, state, SuiyinNode.N004, "N-004");
            DrawNode(list, state, SuiyinNode.N005, "N-005");
            DrawNode(list, state, SuiyinNode.N006, "N-006");
            DrawNode(list, state, SuiyinNode.N007, "N-007");
            DrawNode(list, state, SuiyinNode.N008, "N-008");
            DrawNode(list, state, SuiyinNode.N009, "N-009");
            Section(list, "RHAH_Menu_Narrative_Thresholds");
            SuiyinConfig config = state.Book.Config;
            config.ProgressKinds = ClampKinds(list, "narrative-progress", "RHAH_Menu_Narrative_Progress", config.ProgressKinds);
            config.RewardKinds = ClampKinds(list, "narrative-reward", "RHAH_Menu_Narrative_Reward", config.RewardKinds);
            config.EnvoyKinds = ClampKinds(list, "narrative-envoy", "RHAH_Menu_Narrative_Envoy", config.EnvoyKinds);
            config.RelicKinds = ClampKinds(list, "narrative-relic", "RHAH_Menu_Narrative_Relic", config.RelicKinds);
            Section(list, "RHAH_Menu_Narrative_Journals");
            for (int journal = 1; journal <= 14; journal++)
            {
                string label = "RHAH_Menu_Narrative_JournalOpen".Translate(journal);
                if (list.ButtonText(label))
                {
                    Find.LetterStack.ReceiveLetter(
                        "RHAH_Suiyin_Journal_Label".Translate(journal),
                        "RHAH_Suiyin_Journal_Text".Translate(),
                        LetterDefOf.NeutralEvent);
                }
            }

            if (snapshot.Trust <= -75)
            {
                Note(list, "RHAH_Menu_Narrative_TrustClosed");
            }
        }

        IEnumerable<MenuSearchEntry> SearchNarrative()
        {
            yield return Entry("narrative-outcome", "RHAH_Menu_Narrative_Outcome");
            yield return Entry("narrative-nodes", "RHAH_Menu_Narrative_Nodes");
            yield return Entry("narrative-thresholds", "RHAH_Menu_Narrative_Thresholds");
            yield return Entry("narrative-journals", "RHAH_Menu_Narrative_Journals");
        }

        void DrawOther(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Other");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            DrawOtherNumbers(list, settings);
        }

        static IEnumerable<MenuSearchEntry> SearchOther()
        {
            yield break;
        }
        void DrawOtherNumbers(Listing_Standard list, RHAH_Settings settings)
        {
            string minAge = Buffer(weightBuffers, "age-min", settings.minGeneratedAge, "0.0");
            settings.minGeneratedAge = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_MinAge".Translate(settings.minGeneratedAge.ToString("0.0")), settings.minGeneratedAge, ref minAge, 0f, 100f, "0.0", "RHAH_Settings_Age_Tooltip".Translate());
            weightBuffers["age-min"] = minAge;
            string maxAge = Buffer(weightBuffers, "age-max", settings.maxGeneratedAge, "0.0");
            settings.maxGeneratedAge = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_MaxAge".Translate(settings.maxGeneratedAge.ToString("0.0")), settings.maxGeneratedAge, ref maxAge, settings.minGeneratedAge, 100f, "0.0", "RHAH_Settings_Age_Tooltip".Translate());
            weightBuffers["age-max"] = maxAge;
            string shelter = Buffer(weightBuffers, "shelter-days", settings.shelterDays, "0");
            settings.shelterDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_ShelterDays".Translate(settings.shelterDays), settings.shelterDays, ref shelter, 5f, 60f, "0", "RHAH_Settings_ShelterDays_Tooltip".Translate());
            weightBuffers["shelter-days"] = shelter;
            string hire = Buffer(weightBuffers, "hire-days", settings.hireDays, "0");
            settings.hireDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_HireDays".Translate(settings.hireDays), settings.hireDays, ref hire, 5f, 600f, "0", "RHAH_Settings_HireDays_Tooltip".Translate());
            weightBuffers["hire-days"] = hire;
            MenuControls.Checkbox(list, "RHAH_Settings_ColdClothes".Translate(), ref settings.coldClothesEnabled, "RHAH_Settings_ColdClothes_Tooltip".Translate());
            string cold = Buffer(weightBuffers, "temp-min", settings.minimumEventTemperature, "0");
            settings.minimumEventTemperature = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_TempMin".Translate(settings.minimumEventTemperature.ToString("0")), settings.minimumEventTemperature, ref cold, -35f, 70f, "0", "RHAH_Settings_Temp_Tooltip".Translate());
            weightBuffers["temp-min"] = cold;
            string heat = Buffer(weightBuffers, "temp-max", settings.maximumEventTemperature, "0");
            settings.maximumEventTemperature = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_TempMax".Translate(settings.maximumEventTemperature.ToString("0")), settings.maximumEventTemperature, ref heat, settings.minimumEventTemperature, 70f, "0", "RHAH_Settings_Temp_Tooltip".Translate());
            weightBuffers["temp-max"] = heat;
        }


        void DrawRelief(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Relief");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            DrawReliefToggle(list, "relief-enabled", ref settings.reliefEnabled,
                "RHAH_Settings_ReliefEnabled", settings);
            DrawReliefToggle(list, "relief-outside", ref settings.allowEatOutsideRelief,
                "RHAH_Settings_EatOutsideRelief", settings);
            DrawReliefToggle(list, "relief-ignore", ref settings.ignoreReliefAfterFed,
                "RHAH_Settings_IgnoreReliefAfterFed", settings);
            DrawReliefToggle(list, "relief-leave", ref settings.leaveAfterFed,
                "RHAH_Settings_LeaveAfterFed", settings);
            DrawVisitorNumbers(list, settings);
            DrawFoodList(list, settings);
        }
        void DrawVisitors(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Visitors");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Checkbox(list, "RHAH_Settings_Begging".Translate(), ref settings.beggingEnabled, "RHAH_Settings_Begging_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Stealing".Translate(), ref settings.stealingEnabled, "RHAH_Settings_Stealing_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Fighting".Translate(), ref settings.fightingEnabled, "RHAH_Settings_Fighting_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Gnawing".Translate(), ref settings.gnawingEnabled, "RHAH_Settings_Gnawing_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_BatchHostile".Translate(), ref settings.batchTurnsHostile, "RHAH_Settings_BatchHostile_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_BatchLeave".Translate(), ref settings.batchLeavesTogether, "RHAH_Settings_BatchLeave_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_FamilyDrop".Translate(), ref settings.familyDropEnabled, "RHAH_Settings_FamilyDrop_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_MotherFeed".Translate(), ref settings.motherFeedEnabled, "RHAH_Settings_MotherFeed_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_PrisonerScavenge".Translate(), ref settings.prisonerScavengeEnabled, "RHAH_Settings_PrisonerScavenge_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_TailBite".Translate(), ref settings.tailBiteEnabled, "RHAH_Settings_TailBite_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Broadcast".Translate(), ref settings.broadcastEnabled, "RHAH_Settings_Broadcast_Tooltip".Translate());
            string cooldown = Buffer(weightBuffers, "broadcast-days", settings.broadcastCooldownDays, "0");
            settings.broadcastCooldownDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_BroadcastCooldown".Translate(settings.broadcastCooldownDays), settings.broadcastCooldownDays, ref cooldown, 0f, 10f, "0", "RHAH_Settings_BroadcastCooldown_Tooltip".Translate());
            weightBuffers["broadcast-days"] = cooldown;
            MenuControls.Checkbox(list, "RHAH_Settings_RefugeeCamp".Translate(), ref settings.refugeeCampEnabled, "RHAH_Settings_RefugeeCamp_Tooltip".Translate());
        }

        static IEnumerable<MenuSearchEntry> SearchVisitors()
        {
            yield return Entry("visitors-beg", "RHAH_Settings_Begging");
            yield return Entry("visitors-broadcast", "RHAH_Settings_Broadcast");
            yield return Entry("visitors-camp", "RHAH_Settings_RefugeeCamp");
        }

        void DrawPlague(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Plague");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Checkbox(list, "RHAH_Settings_Plague".Translate(), ref settings.plagueEnabled, "RHAH_Settings_Plague_Tooltip".Translate());
            string per = Buffer(weightBuffers, "plague-per", settings.plagueSpreadChancePerCarrier * 100f, "0.0");
            float perPercent = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlaguePerCarrier".Translate((settings.plagueSpreadChancePerCarrier * 100f).ToString("0.0")), settings.plagueSpreadChancePerCarrier * 100f, ref per, 0f, 100f, "0.0", "RHAH_Settings_PlaguePerCarrier_Tooltip".Translate());
            settings.plagueSpreadChancePerCarrier = perPercent / 100f;
            weightBuffers["plague-per"] = per;
            string cap = Buffer(weightBuffers, "plague-cap", settings.plagueSpreadChanceCap * 100f, "0");
            float capPercent = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueCap".Translate((settings.plagueSpreadChanceCap * 100f).ToString("0")), settings.plagueSpreadChanceCap * 100f, ref cap, 0f, 100f, "0", "RHAH_Settings_PlagueCap_Tooltip".Translate());
            settings.plagueSpreadChanceCap = capPercent / 100f;
            weightBuffers["plague-cap"] = cap;
            string days = Buffer(weightBuffers, "plague-days", settings.plagueSpreadDayInterval, "0");
            settings.plagueSpreadDayInterval = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueDays".Translate(settings.plagueSpreadDayInterval), settings.plagueSpreadDayInterval, ref days, 1f, 30f, "0", "RHAH_Settings_PlagueDays_Tooltip".Translate());
            weightBuffers["plague-days"] = days;
            string hour = Buffer(weightBuffers, "plague-hour", settings.plagueSpreadHour, "0");
            settings.plagueSpreadHour = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueHour".Translate(settings.plagueSpreadHour), settings.plagueSpreadHour, ref hour, 0f, 23f, "0", "RHAH_Settings_PlagueHour_Tooltip".Translate());
            weightBuffers["plague-hour"] = hour;
            string blood = Buffer(weightBuffers, "plague-blood", settings.plagueBloodPumpingSkipPercent, "0");
            settings.plagueBloodPumpingSkipPercent = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueBlood".Translate(settings.plagueBloodPumpingSkipPercent.ToString("0")), settings.plagueBloodPumpingSkipPercent, ref blood, 0f, 300f, "0", "RHAH_Settings_PlagueBlood_Tooltip".Translate());
            weightBuffers["plague-blood"] = blood;
            MenuControls.Checkbox(list, "RHAH_Settings_PlagueQuarantine".Translate(), ref settings.plagueQuarantineBlocksJoin, "RHAH_Settings_PlagueQuarantine_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_PlagueReturn".Translate(), ref settings.plagueReturnEnabled, "RHAH_Settings_PlagueReturn_Tooltip".Translate());
            string delay = Buffer(weightBuffers, "plague-delay", settings.plagueReturnDelayDays, "0");
            settings.plagueReturnDelayDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueDelay".Translate(settings.plagueReturnDelayDays), settings.plagueReturnDelayDays, ref delay, 0f, 60f, "0", "RHAH_Settings_PlagueDelay_Tooltip".Translate());
            weightBuffers["plague-delay"] = delay;
            string stay = Buffer(weightBuffers, "plague-stay", settings.plagueReturnStayDays, "0");
            settings.plagueReturnStayDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_PlagueStay".Translate(settings.plagueReturnStayDays), settings.plagueReturnStayDays, ref stay, 0f, 15f, "0", "RHAH_Settings_PlagueStay_Tooltip".Translate());
            weightBuffers["plague-stay"] = stay;
        }

        static IEnumerable<MenuSearchEntry> SearchPlague()
        {
            yield return Entry("plague-enabled", "RHAH_Settings_Plague");
            yield return Entry("plague-return", "RHAH_Settings_PlagueReturn");
        }


        static void DrawReliefToggle(
            Listing_Standard list,
            string anchor,
            ref bool value,
            string key,
            RHAH_Settings settings)
        {
            bool before = value;
            MenuControls.Anchor(list, anchor);
            MenuControls.Checkbox(list, key.Translate(), ref value, (key + "_Tooltip").Translate());
            if (before != value)
            {
                settings.InvalidateReliefSearch();
            }
        }
        void DrawVisitorNumbers(Listing_Standard list, RHAH_Settings settings)
        {
            string cap = Buffer(weightBuffers, "event-cap", settings.maxEventPawns, "0");
            settings.maxEventPawns = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_MaxEventPawns".Translate(settings.maxEventPawns), settings.maxEventPawns, ref cap, 1f, 100f, "0", "RHAH_Settings_MaxEventPawns_Tooltip".Translate());
            weightBuffers["event-cap"] = cap;
            string bonus = Buffer(weightBuffers, "relief-bonus", settings.reliefFoodScoreBonus * 100f, "0");
            float percent = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_ReliefScore".Translate((settings.reliefFoodScoreBonus * 100f).ToString("0")), settings.reliefFoodScoreBonus * 100f, ref bonus, 0f, 100f, "0", "RHAH_Settings_ReliefScore_Tooltip".Translate());
            settings.reliefFoodScoreBonus = percent / 100f;
            weightBuffers["relief-bonus"] = bonus;
            string stay = Buffer(weightBuffers, "fed-stay", settings.fedStayDays, "0.00");
            settings.fedStayDays = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_FedStay".Translate(settings.fedStayDays.ToString("0.00")), settings.fedStayDays, ref stay, 0f, 5f, "0.00", "RHAH_Settings_FedStay_Tooltip".Translate());
            weightBuffers["fed-stay"] = stay;
            MenuControls.Checkbox(list, "RHAH_Settings_WaitFood".Translate(), ref settings.waitWhenNoFood, "RHAH_Settings_WaitFood_Tooltip".Translate());
            string wait = Buffer(weightBuffers, "food-wait", settings.noFoodWaitDays, "0.00");
            settings.noFoodWaitDays = RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Settings_FoodWait".Translate(settings.noFoodWaitDays.ToString("0.00")), settings.noFoodWaitDays, ref wait, 0f, 5f, "0.00", "RHAH_Settings_FoodWait_Tooltip".Translate());
            weightBuffers["food-wait"] = wait;
        }

        void DrawFoodList(Listing_Standard list, RHAH_Settings settings)
        {
            MenuControls.Anchor(list, "relief-foods", 86f);
            RHAH_IrisMenusWidgets.Quote(list, "relief-foods-note", "RHAH_Settings_ReliefFoods_Quote".Translate());
            Rect buttons = list.GetRect(28f);
            if (Widgets.ButtonText(new Rect(buttons.x, buttons.y, 140f, 26f),
                "RHAH_Settings_ReliefFoods_All".Translate()))
            {
                settings.SetAllReliefFood(true, null);
            }

            if (Widgets.ButtonText(new Rect(buttons.x + 148f, buttons.y, 140f, 26f),
                "RHAH_Settings_ReliefFoods_None".Translate()))
            {
                List<ThingDef> foods = new List<ThingDef>();
                RHAH_ReliefFood.AppendCandidateFoods(foods);
                List<string> names = new List<string>(foods.Count);
                for (int i = 0; i < foods.Count; i++)
                {
                    names.Add(foods[i].defName);
                }

                settings.SetAllReliefFood(false, names);
            }

            list.Gap(4f);
            Rect search = list.GetRect(28f);
            foodQuery = Widgets.TextField(search, foodQuery ?? string.Empty);
            if (string.IsNullOrEmpty(foodQuery))
            {
                GUI.color = Color.gray;
                Widgets.Label(new Rect(search.x + 6f, search.y, search.width - 8f, search.height),
                    "RHAH_Settings_ReliefFoods_Search".Translate());
                GUI.color = Color.white;
            }

            list.Gap(4f);
            List<ThingDef> listed = new List<ThingDef>();
            RHAH_ReliefFood.AppendCandidateFoods(listed);
            List<List<ThingDef>> groups = RHAH_ReliefFood.GroupBySourceMod(listed);
            bool searching = !string.IsNullOrEmpty(foodQuery);
            bool any = false;
            for (int i = 0; i < groups.Count; i++)
            {
                List<ThingDef> matched = MatchedFoods(groups[i]);
                if (matched.Count == 0)
                {
                    continue;
                }

                any = true;
                string modName = RHAH_ReliefFood.SourceModName(groups[i][0]);
                string key = modName ?? string.Empty;
                if (pendingFoodMod != null && string.Equals(pendingFoodMod, key, StringComparison.Ordinal))
                {
                    collapsedFoodMods.Remove(key);
                    pendingFoodMod = null;
                }

                bool open = searching || !collapsedFoodMods.Contains(key);
                string title = (modName ?? "RHAH_Menu_Genes_UnknownMod".Translate()) + "  " + matched.Count;
                MenuControls.Anchor(list, "relief-food-mod-" + key, 28f);
                if (DrawFoodFold(list, title, open))
                {
                    if (open)
                    {
                        collapsedFoodMods.Add(key);
                    }
                    else
                    {
                        collapsedFoodMods.Remove(key);
                    }

                    open = !open;
                }

                if (!open)
                {
                    continue;
                }

                for (int foodIndex = 0; foodIndex < matched.Count; foodIndex++)
                {
                    ThingDef food = matched[foodIndex];
                    bool enabled = settings.IsReliefFoodEnabled(food.defName);
                    MenuControls.Anchor(list, "relief-food-" + food.defName);
                    MenuControls.Checkbox(list, food.LabelCap, ref enabled);
                    settings.SetReliefFoodEnabled(food.defName, enabled);
                }
            }

            if (!any)
            {
                Empty(list, "RHAH_Settings_ReliefFoods_Empty");
            }
        }

        List<ThingDef> MatchedFoods(List<ThingDef> foods)
        {
            List<ThingDef> matched = new List<ThingDef>();
            for (int i = 0; i < foods.Count; i++)
            {
                if (RHAH_ReliefFood.MatchesQuery(foods[i], foodQuery))
                {
                    matched.Add(foods[i]);
                }
            }

            return matched;
        }

        static bool DrawFoodFold(Listing_Standard list, string title, bool open)
        {
            Rect row = list.GetRect(28f);
            Widgets.DrawHighlightIfMouseover(row);
            Rect mark = new Rect(row.x, row.y + 2f, 24f, 24f);
            Widgets.DrawTextureFitted(mark, open ? TexButton.Collapse : TexButton.Reveal, 0.7f);
            Widgets.Label(new Rect(row.x + 26f, row.y, row.width - 26f, row.height), title);
            return Widgets.ButtonInvisible(row);
        }

        IEnumerable<MenuSearchEntry> SearchRelief()
        {
            yield return Entry("relief-enabled", "RHAH_Settings_ReliefEnabled");
            yield return Entry("relief-outside", "RHAH_Settings_EatOutsideRelief");
            yield return Entry("relief-foods", "RHAH_Settings_ReliefFoods");
            List<ThingDef> foods = new List<ThingDef>();
            RHAH_ReliefFood.AppendCandidateFoods(foods);
            for (int i = 0; i < foods.Count; i++)
            {
                ThingDef food = foods[i];
                if (food == null || string.IsNullOrEmpty(food.defName))
                {
                    continue;
                }

                string modName = RHAH_ReliefFood.SourceModName(food);
                string key = modName ?? string.Empty;
                string id = "relief-food-" + food.defName;
                string label = food.LabelCap;
                yield return new MenuSearchEntry(
                    id,
                    () => label,
                    () => food.defName + " " + (modName ?? string.Empty),
                    () =>
                    {
                        pendingFoodMod = key;
                        return modName;
                    });
            }
        }
        void DrawCompatDiagnostics(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_CompatDiagnostics");
            Status(list, "RHAH_Menu_Field_Mod", "RHAH_ModName".Translate());
            Status(list, "RHAH_Menu_Field_Version", ContentFinderVersion());
            Status(list, "RHAH_Menu_Field_Game",
                Current.Game == null ? "RHAH_Menu_NoGame".Translate() : "RHAH_Menu_GameLoaded".Translate());
            DrawModRow(list, "ratkin", "NewRatkinPlus", "Solaris.RatkinRaceMod", true);
            DrawModRow(list, "harmony", "Harmony", "brrainz.harmony", true);
            DrawModRow(list, "biotech", "Biotech", "Ludeon.RimWorld.Biotech", true);
            DrawModRow(list, "iris", "IrisMenus", PackageId(), false);
            DrawModRow(list, "toddlers", "Toddlers", "cyanobot.toddlers", false);
            DrawModRow(list, "leash", "Lead Your Pet", "nanaloveyuki.leadyourpet.continued", false);
            DrawModRow(list, "prisoner", "Prisoner Work", "LeZhizhong.PrisonerWorkExpansion", false);
            Status(list, "RHAH_Menu_Field_IrisMenus", irisVersion);
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                Empty(list, "RHAH_Menu_Diagnostics_NoGame");
                return;
            }

            Status(list, "RHAH_Menu_Diagnostics_Pending", game.PendingIncidentDisplayIds.Count.ToString());
            Status(list, "RHAH_Menu_Diagnostics_Batches", game.ActiveGenerationBatches.Count.ToString());
            Map map = RHAH_MapResolver.Resolve();
            Status(list, "RHAH_Menu_Diagnostics_Map",
                map == null ? "RHAH_Menu_Queue_NoMap".Translate() : map.uniqueID.ToString());
        }

        IEnumerable<MenuSearchEntry> SearchCompatDiagnostics()
        {
            yield return Entry("compat-mod", "RHAH_Menu_Field_Mod");
            yield return Entry("compat-version", "RHAH_Menu_Field_Version");
            yield return Entry("compat-game", "RHAH_Menu_Field_Game");
            yield return Entry("compat-iris", "RHAH_Menu_Field_IrisMenus");
            yield return Entry("diagnostics-pending", "RHAH_Menu_Diagnostics_Pending");
        }

        static void DrawModRow(Listing_Standard list, string anchor, string label, string packageId, bool required)
        {
            MenuControls.Anchor(list, "compat-" + anchor);
            ModMetaData meta = ModLister.GetActiveModWithIdentifier(packageId, false);
            string state = meta == null
                ? (required ? "RHAH_Menu_Compat_MissingRequired".Translate() : "RHAH_Menu_Compat_Inactive".Translate())
                : "RHAH_Menu_Compat_Active".Translate(VersionOf(meta));
            Status(list, label, state);
        }

        static string PackageId()
        {
            return RHAH_IrisMenusCompat.PackageId;
        }


        void DrawEnding(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Ending");
            RHAH_IrisMenusWidgets.Quote(list, "ending-note", "RHAH_Menu_Ending_Note".Translate());
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Checkbox(list, "RHAH_Ending_CountWithout".Translate(), ref settings.countEndingsWithoutNarrator, "RHAH_Ending_CountWithout_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_WithoutNarrator".Translate(), ref settings.endingsWithoutNarrator, "RHAH_Ending_WithoutNarrator_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_E01_Label".Translate(), ref settings.endingE01, "RHAH_Ending_Toggle_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_E02_Label".Translate(), ref settings.endingE02, "RHAH_Ending_Toggle_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_E03_Label".Translate(), ref settings.endingE03, "RHAH_Ending_Toggle_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_E04_Label".Translate(), ref settings.endingE04, "RHAH_Ending_Toggle_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_E05_Label".Translate(), ref settings.endingE05, "RHAH_Ending_Toggle_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Ending_Identity".Translate(), ref settings.endingIdentity, "RHAH_Ending_Identity_Tooltip".Translate());
            string aid = Buffer(weightBuffers, "ending-aid", settings.endingAidGoal, "0");
            settings.endingAidGoal = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Ending_Aid".Translate(settings.endingAidGoal), settings.endingAidGoal, ref aid, 1f, 999f, "0", "RHAH_Ending_Aid_Tooltip".Translate());
            weightBuffers["ending-aid"] = aid;
            string broadcasts = Buffer(weightBuffers, "ending-broadcast", settings.endingBroadcastGoal, "0");
            settings.endingBroadcastGoal = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Ending_Broadcast".Translate(settings.endingBroadcastGoal), settings.endingBroadcastGoal, ref broadcasts, 1f, 99f, "0", "RHAH_Ending_Broadcast_Tooltip".Translate());
            weightBuffers["ending-broadcast"] = broadcasts;
            string expulsions = Buffer(weightBuffers, "ending-expel", settings.endingExpulsionLimit, "0");
            settings.endingExpulsionLimit = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Ending_Expel".Translate(settings.endingExpulsionLimit), settings.endingExpulsionLimit, ref expulsions, 0f, 99f, "0", "RHAH_Ending_Expel_Tooltip".Translate());
            weightBuffers["ending-expel"] = expulsions;
            string adults = Buffer(weightBuffers, "ending-adults", settings.endingAdultGoal, "0");
            settings.endingAdultGoal = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Ending_Adults".Translate(settings.endingAdultGoal), settings.endingAdultGoal, ref adults, 1f, 500f, "0", "RHAH_Ending_Adults_Tooltip".Translate());
            weightBuffers["ending-adults"] = adults;
            string wait = Buffer(weightBuffers, "ending-wait", settings.endingWaitDays, "0");
            settings.endingWaitDays = (int)RHAH_IrisMenusWidgets.TunedValue(list, "RHAH_Ending_Wait".Translate(settings.endingWaitDays), settings.endingWaitDays, ref wait, 0f, 120f, "0", "RHAH_Ending_Wait_Tooltip".Translate());
            weightBuffers["ending-wait"] = wait;
            if (Prefs.DevMode)
            {
                RHAH_IrisMenusWidgets.Quote(list, "ending-preview", "RHAH_Ending_Preview_Note".Translate());
                PreviewButton(list, "preview-e01", RHAH_EndingId.E01);
                PreviewButton(list, "preview-e02", RHAH_EndingId.E02);
                PreviewButton(list, "preview-e03", RHAH_EndingId.E03);
                PreviewButton(list, "preview-e04", RHAH_EndingId.E04);
                PreviewButton(list, "preview-e05", RHAH_EndingId.E05);
                PreviewButton(list, "preview-r01", RHAH_EndingId.None);
            }
        }

        void PreviewButton(Listing_Standard list, string anchor, RHAH_EndingId id)
        {
            MenuControls.Anchor(list, anchor);
            string key = id == RHAH_EndingId.None ? "RHAH_Ending_IdentityAsk_Label" : RHAH_EndingRuntime.TextKey(id, true) + "_Label";
            if (list.ButtonText(key.Translate()))
            {
                NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
                int aid = state == null ? 0 : state.AidCount;
                int broadcasts = state == null ? 0 : state.BroadcastCount;
                int adults = state == null ? 0 : state.AdultCount;
                Messages.Message(RHAH_EndingRuntime.Preview(id, true, aid, broadcasts, adults), MessageTypeDefOf.NeutralEvent, false);
            }
        }

        void DrawGenes(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Genes");
            if (!ModsConfig.BiotechActive)
            {
                Empty(list, "RHAH_Menu_Genes_NoBiotech");
                return;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            RHAH_IrisMenusWidgets.Quote(list, "genes-note", "RHAH_Menu_Genes_Note".Translate());
            if (list.ButtonText("RHAH_Menu_Genes_Reset".Translate()))
            {
                settings.ResetXenotypeWeights();
            }

            List<XenotypeDef> xenotypes = RHAH_XenotypeResolver.LoadedCandidates(settings);
            float total = 0f;
            for (int i = 0; i < xenotypes.Count; i++)
            {
                float weight = settings.XenotypeWeight(xenotypes[i].defName);
                if (weight > 0f)
                {
                    total += weight;
                }
            }

            ReservedNote(list, "genes-fallback", "RHAH_Menu_Genes_Fallback", total <= 0f);

            DrawXenotypesByMod(list, xenotypes, group =>
            {
                for (int i = 0; i < group.Count; i++)
                {
                    DrawXenotypeBar(list, settings, group[i], total);
                }
            });

            DrawMissingXenotypes(list, settings, xenotypes);
            DrawJoinableXenotypes(list, settings);
            DrawGeneSwitches(list, settings);
            RHAH_Mod.Settings.Write();
        }

        static void DrawJoinableXenotypes(Listing_Standard list, RHAH_Settings settings)
        {
            List<XenotypeDef> available = RHAH_XenotypeResolver.AvailableToJoin(settings);
            if (available.Count == 0)
            {
                return;
            }

            Section(list, "RHAH_Menu_Genes_Join");
            DrawXenotypesByMod(list, available, group =>
            {
                for (int i = 0; i < group.Count; i++)
                {
                    XenotypeDef xenotype = group[i];
                    if (list.ButtonText("RHAH_Menu_Genes_JoinOne".Translate(xenotype.LabelCap)))
                    {
                        settings.SetXenotypeEnabled(xenotype.defName, true);
                    }
                }
            });
        }

        static void DrawXenotypesByMod(Listing_Standard list, List<XenotypeDef> xenotypes, Action<List<XenotypeDef>> drawGroup)
        {
            List<List<XenotypeDef>> groups = RHAH_XenotypeResolver.GroupBySourceMod(xenotypes);
            for (int i = 0; i < groups.Count; i++)
            {
                string name = RHAH_XenotypeResolver.SourceModName(groups[i][0]);
                MenuControls.Section(list, name ?? "RHAH_Menu_Genes_UnknownMod".Translate());
                drawGroup(groups[i]);
            }
        }

        void DrawXenotypeBar(Listing_Standard list, RHAH_Settings settings, XenotypeDef xenotype, float total)
        {
            float weight = settings.XenotypeWeight(xenotype.defName);
            float share = total <= 0f || weight <= 0f ? 0f : weight / total;
            string buffer = Buffer(weightBuffers, "xeno-" + xenotype.defName, weight, "0");
            weight = RHAH_IrisMenusWidgets.TunedValue(
                list,
                xenotype.LabelCap + "  " + share.ToString("P0"),
                weight,
                ref buffer,
                RHAH_XenotypeWeightTable.MinWeight,
                RHAH_XenotypeWeightTable.MaxWeight,
                "0",
                "RHAH_Menu_Genes_WeightTip".Translate(xenotype.defName, RHAH_GeneCatalog.SuggestedWeight(xenotype.defName).ToString("0")));
            weightBuffers["xeno-" + xenotype.defName] = buffer;
            settings.SetXenotypeWeight(xenotype.defName, weight);
            if (!RHAH_GeneCatalog.IsBuiltin(xenotype.defName))
            {
                Rect remove = list.GetRect(26f);
                if (Widgets.ButtonText(new Rect(remove.xMax - 72f, remove.y, 68f, 24f), "RHAH_Menu_Genes_Remove".Translate()))
                {
                    settings.SetXenotypeEnabled(xenotype.defName, false);
                }

                list.Gap(4f);
            }
        }



        static void DrawMissingXenotypes(Listing_Standard list, RHAH_Settings settings, List<XenotypeDef> loaded)
        {
            List<string> stored = settings.MissingXenotypeNames();
            for (int i = 0; i < stored.Count; i++)
            {
                if (Loaded(loaded, stored[i]) || RHAH_GeneCatalog.IsBuiltin(stored[i]))
                {
                    continue;
                }

                list.Label("RHAH_Menu_Genes_Missing".Translate(stored[i]));
            }
        }

        static bool Loaded(List<XenotypeDef> loaded, string defName)
        {
            for (int i = 0; i < loaded.Count; i++)
            {
                if (loaded[i].defName == defName)
                {
                    return true;
                }
            }

            return false;
        }

        static void DrawGeneSwitches(Listing_Standard list, RHAH_Settings settings)
        {
            Section(list, "RHAH_Menu_Genes_Switches");
            List<GeneDef> genes = RHAH_XenotypeResolver.LoadedOwnedGenes();
            if (genes.Count == 0)
            {
                Empty(list, "RHAH_Menu_Genes_NoOwned");
                return;
            }

            for (int i = 0; i < genes.Count; i++)
            {
                bool enabled = settings.IsGeneEnabled(genes[i].defName);
                MenuControls.Checkbox(list, genes[i].LabelCap, ref enabled, genes[i].description);
                settings.SetGeneEnabled(genes[i].defName, enabled);
            }
        }

        void DrawPawnHistory(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_PawnHistory");
            RHAH_IrisMenusWidgets.Quote(list, "history-note", "RHAH_Menu_PawnHistory_Note".Translate());
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Checkbox(
                list,
                "RHAH_Settings_PawnHistories".Translate(),
                ref settings.pawnHistoriesEnabled,
                "RHAH_Settings_PawnHistories_Tooltip".Translate());
            MenuControls.Checkbox(
                list,
                "RHAH_Settings_PawnTraits".Translate(),
                ref settings.pawnTraitsEnabled,
                "RHAH_Settings_PawnTraits_Tooltip".Translate());
            DrawContentToggles(list, settings);
        }

        void DrawContentToggles(Listing_Standard list, RHAH_Settings settings)
        {
            List<string> historyIds = new List<string>();
            RHAH_Api.CopyHistoryIds(historyIds);
            if (list.ButtonText("RHAH_Menu_PawnHistory_EnableAll".Translate()))
            {
                settings.SetAllHistories(true, historyIds);
            }

            if (list.ButtonText("RHAH_Menu_PawnHistory_DisableAll".Translate()))
            {
                settings.SetAllHistories(false, historyIds);
            }

            for (int i = 0; i < historyIds.Count; i++)
            {
                string id = historyIds[i];
                string defName;
                bool enabled = settings.IsHistoryEnabled(id);
                string label = RHAH_Api.TryGetHistory(id, out defName)
                    ? id + " " + BackstoryTitle(defName)
                    : id;
                MenuControls.Checkbox(list, label, ref enabled, "RHAH_Menu_PawnHistory_ItemTip".Translate());
                settings.SetHistoryEnabled(id, enabled);
            }

            Section(list, "RHAH_Menu_PawnTrait");
            List<string> traitIds = new List<string>();
            RHAH_Api.CopyTraitIds(traitIds);
            if (list.ButtonText("RHAH_Menu_PawnTrait_EnableAll".Translate()))
            {
                settings.SetAllTraits(true, traitIds);
            }

            if (list.ButtonText("RHAH_Menu_PawnTrait_DisableAll".Translate()))
            {
                settings.SetAllTraits(false, traitIds);
            }

            for (int i = 0; i < traitIds.Count; i++)
            {
                string id = traitIds[i];
                string defName;
                bool enabled = settings.IsTraitEnabled(id);
                string label = RHAH_Api.TryGetTrait(id, out defName)
                    ? id + " " + TraitTitle(defName)
                    : id;
                MenuControls.Checkbox(list, label, ref enabled, "RHAH_Menu_PawnTrait_ItemTip".Translate());
                settings.SetTraitEnabled(id, enabled);
                float weight = settings.TraitWeight(id);
                string buffer = Buffer(weightBuffers, "trait-" + id, weight, "0");
                weight = RHAH_IrisMenusWidgets.TunedValue(
                    list,
                    "RHAH_Menu_PawnTrait_Weight".Translate(),
                    weight,
                    ref buffer,
                    0f,
                    100f,
                    "0",
                    "RHAH_Menu_PawnTrait_WeightTip".Translate());
                weightBuffers["trait-" + id] = buffer;
                settings.SetTraitWeight(id, weight);
            }
        }

        static string BackstoryTitle(string defName)
        {
            BackstoryDef backstory = DefDatabase<BackstoryDef>.GetNamedSilentFail(defName);
            string title = backstory?.title;
            return string.IsNullOrEmpty(title) ? defName : title;
        }

        static string TraitTitle(string defName)
        {
            TraitDef trait = DefDatabase<TraitDef>.GetNamedSilentFail(defName);
            if (trait?.degreeDatas == null || trait.degreeDatas.Count == 0 || trait.degreeDatas[0] == null)
            {
                return defName;
            }

            string label = trait.degreeDatas[0].label;
            return string.IsNullOrEmpty(label) ? defName : label;
        }

        void DrawExperimental(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Experimental");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Anchor(list, "optimize-generation");
            MenuControls.Checkbox(
                list,
                "RHAH_Settings_OptimizeGeneration".Translate(),
                ref settings.optimizeGeneration,
                "RHAH_Settings_OptimizeGeneration_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_AidRequests".Translate(), ref settings.aidRequestsEnabled, "RHAH_Settings_AidRequests_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_IntelTrades".Translate(), ref settings.intelTradesEnabled, "RHAH_Settings_IntelTrades_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_VisitorChoices".Translate(), ref settings.visitorChoicesEnabled, "RHAH_Settings_VisitorChoices_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_TraderIgnoreEnvironment".Translate(), ref settings.traderIgnoresHarshEnvironment, "RHAH_Settings_TraderIgnoreEnvironment_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_TraderIgnoreEnclosed".Translate(), ref settings.traderIgnoresEnclosedSpace, "RHAH_Settings_TraderIgnoreEnclosed_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_ChildExchangeFood".Translate(), ref settings.childExchangeFoodSubstitution, "RHAH_Settings_ChildExchangeFood_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Stagger".Translate(), ref settings.staggerGeneration, "RHAH_Settings_Stagger_Tooltip".Translate());
        }

        void DrawFrequency(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_EventFrequency");
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
                return;
            }

            MenuControls.Anchor(list, "frequency-positive");
            settings.positiveIncidentDays = PoolDays(
                list,
                "frequency-positive",
                "RHAH_Menu_Frequency_Positive".Translate(),
                settings.positiveIncidentDays);
            RHAH_IrisMenusWidgets.OccurrenceCurve(
                list,
                "frequency-positive-curve",
                settings.positiveIncidentDays,
                new Color(0.45f, 0.78f, 0.48f));

            MenuControls.Anchor(list, "frequency-negative");
            settings.negativeIncidentDays = PoolDays(
                list,
                "frequency-negative",
                "RHAH_Menu_Frequency_Negative".Translate(),
                settings.negativeIncidentDays);
            RHAH_IrisMenusWidgets.OccurrenceCurve(
                list,
                "frequency-negative-curve",
                settings.negativeIncidentDays,
                new Color(0.86f, 0.42f, 0.36f));
            Factor(list, settings, "weight-wild", "RHAH_Settings_WeightWild", ref settings.weightWild);
            Factor(list, settings, "weight-beggar", "RHAH_Settings_WeightBeggar", ref settings.weightBeggar);
            Factor(list, settings, "weight-thief", "RHAH_Settings_WeightThief", ref settings.weightThief);
            Factor(list, settings, "weight-trade", "RHAH_Settings_WeightTrade", ref settings.weightTrade);
            Factor(list, settings, "weight-siege", "RHAH_Settings_WeightSiege", ref settings.weightSiege);
            Factor(list, settings, "weight-aid", "RHAH_Settings_WeightAid", ref settings.weightAid);
            Factor(list, settings, "weight-special", "RHAH_Settings_WeightSpecial", ref settings.weightSpecial);
            Factor(list, settings, "weight-intel", "RHAH_Settings_WeightIntel", ref settings.weightIntel);
            Factor(list, settings, "weight-season", "RHAH_Settings_WeightSeason", ref settings.weightSeason);
            Factor(list, settings, "weight-plague", "RHAH_Settings_WeightPlague", ref settings.weightPlague);
            MenuControls.Checkbox(list, "RHAH_Settings_SuiyinTempo".Translate(), ref settings.suiYinThreatTempo, "RHAH_Settings_SuiyinTempo_Tooltip".Translate());
            list.Gap(4f);
        }

        float PoolDays(Listing_Standard list, string id, string label, float days)
        {
            string buffer = Buffer(weightBuffers, id, days, "0.#");
            days = RHAH_IrisMenusWidgets.TunedValue(
                list,
                label,
                days,
                ref buffer,
                RHAH_IncidentSchedule.MinDays,
                RHAH_IncidentSchedule.MaxDays,
                "0.#",
                "RHAH_Menu_Frequency_Gap".Translate());
            weightBuffers[id] = buffer;
            return RHAH_IncidentSchedule.ClampDays(days);
        }

        void Factor(Listing_Standard list, RHAH_Settings settings, string id, string key, ref float value)
        {
            string buffer = Buffer(weightBuffers, id, value, "0.00");
            value = RHAH_IrisMenusWidgets.TunedValue(list, key.Translate(value.ToString("0.00")), value, ref buffer, 0f, 5f, "0.00", "RHAH_Settings_Weight_Tooltip".Translate());
            weightBuffers[id] = buffer;
        }

        IEnumerable<MenuSearchEntry> SearchFrequency()
        {
            yield return Entry("frequency-positive", "RHAH_Menu_Frequency_Positive");
            yield return Entry("frequency-positive-curve", "RHAH_Menu_Frequency_Curve");
            yield return Entry("frequency-negative", "RHAH_Menu_Frequency_Negative");
            yield return Entry("frequency-negative-curve", "RHAH_Menu_Frequency_Curve");
        }

        void DrawDeveloper(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Developer");
            if (!string.IsNullOrEmpty(selectedPawnLabel))
            {
                Status(list, "RHAH_Menu_Developer_LastPawn", selectedPawnLabel);
            }
        }

        IEnumerable<MenuSearchEntry> SearchDeveloper()
        {
            yield return Entry("developer-last-pawn", "RHAH_Menu_Developer_LastPawn");
        }

        static IEnumerable<MenuSearchEntry> SearchEnding()
        {
            yield return Entry("ending-count", "RHAH_Ending_CountWithout");
            yield return Entry("ending-without", "RHAH_Ending_WithoutNarrator");
            yield return Entry("ending-aid", "RHAH_Ending_Aid");
            yield return Entry("ending-identity", "RHAH_Ending_Identity");
        }

        static IEnumerable<MenuSearchEntry> SearchGenes()
        {
            yield return Entry("genes-weights", "RHAH_Menu_Genes");
            yield return Entry("genes-switches", "RHAH_Menu_Genes_Switches");
        }

        static IEnumerable<MenuSearchEntry> SearchPawnHistory()
        {
            yield return Entry("history-master", "RHAH_Settings_PawnHistories");
            yield return Entry("trait-master", "RHAH_Settings_PawnTraits");
        }

        static IEnumerable<MenuSearchEntry> SearchExperimental()
        {
            yield return Entry("optimize-generation", "RHAH_Settings_OptimizeGeneration", "generation optimizer");
        }

        static void Unavailable(Listing_Standard list, string titleKey, string gapKey)
        {
            Section(list, titleKey);
            Empty(list, "RHAH_Menu_Unavailable");
            Note(list, gapKey);
        }
        static void DrawNode(Listing_Standard list, NarrativeState state, SuiyinNode node, string label)
        {
            bool enabled = state.SuiyinEnabled(node);
            list.CheckboxLabeled(label, ref enabled);
            state.SetSuiyinEnabled(node, enabled);
        }

        static int ClampKinds(Listing_Standard list, string anchor, string key, int value)
        {
            MenuControls.Anchor(list, anchor, 28f);
            value = (int)list.Slider(value, 1f, 14f);
            list.Label(key.Translate() + ": " + value);
            if (value < 1)
            {
                return 1;
            }

            return value > 14 ? 14 : value;
        }


        static void Section(Listing_Standard list, string titleKey)
        {
            MenuControls.Section(list, titleKey.Translate());
        }

        static void Status(Listing_Standard list, string labelKey, string value)
        {
            string label = labelKey.StartsWith("RHAH_", StringComparison.Ordinal) ? labelKey.Translate() : labelKey;
            list.Label(label + ": " + value);
        }

        static void Note(Listing_Standard list, string key)
        {
            list.Label(key.Translate());
            list.Gap(4f);
        }

        static void ReservedNote(Listing_Standard list, string anchor, string key, bool visible)
        {
            string text = key.Translate();
            float width = Mathf.Max(1f, list.ColumnWidth);
            float height = Text.CalcHeight(text, width);
            MenuControls.Anchor(list, anchor, height + 4f);
            Rect rect = list.GetRect(height);
            if (visible)
            {
                Widgets.Label(rect, text);
            }

            list.Gap(4f);
        }

        static void Empty(Listing_Standard list, string key)
        {
            MenuControls.Anchor(list, "empty-" + key);
            list.Label(key.Translate());
            list.Gap(4f);
        }

        static string ContentFinderVersion()
        {
            ModMetaData meta = ModLister.GetActiveModWithIdentifier(RHAH_Runtime.PackageId, false);
            return meta == null ? "0.1.0" : VersionOf(meta);
        }

        static string VersionOf(ModMetaData meta)
        {
            return string.IsNullOrEmpty(meta.ModVersion) ? "unknown" : meta.ModVersion;
        }

        static MenuSearchEntry Entry(string id, string titleKey, string keywords = null)
        {
            return new MenuSearchEntry(id, () => titleKey.Translate(), keywords == null ? null : () => keywords);
        }

        static string FamilyLabel(RHAH_IncidentFamily family)
        {
            return ("RHAH_Menu_Family_" + family).Translate();
        }

        static string OriginLabel(RHAH_IncidentOrigin origin)
        {
            return ("RHAH_Menu_Origin_" + origin).Translate();
        }

        static string CategoryLabel(RHAH_IncidentCategory category)
        {
            return ("RHAH_Menu_Category_" + category).Translate();
        }

        static string TargetLabel(RHAH_IncidentTarget target)
        {
            return ("RHAH_Menu_Target_" + target).Translate();
        }

        static string RoleLabel(RHAH_PawnRole role)
        {
            return ("RHAH_Role_" + role).Translate();
        }

        static string LifecycleLabel(RHAH_Lifecycle lifecycle)
        {
            return ("RHAH_Menu_Lifecycle_" + lifecycle).Translate();
        }

        static string OutcomeLabel(NarrativeOutcome outcome)
        {
            return ("RHAH_Menu_Outcome_" + outcome).Translate();
        }
    }
}
