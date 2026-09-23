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

        string selectedPawnLabel = string.Empty;

        internal RHAH_IrisMenusPages(string irisVersion)
        {
            this.irisVersion = irisVersion;
        }

        internal void Register(Mod owner)
        {
            RegisterPage(owner, "overview", "RHAH_Menu_Overview", DrawOverview, SearchOverview);
            RegisterPage(owner, "relief", "RHAH_Menu_Relief", DrawRelief, SearchRelief);
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
            Status(list, "RHAH_Menu_Field_Mod", "RHAH_ModName".Translate());
            Status(list, "RHAH_Menu_Field_Version", ContentFinderVersion());
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                Empty(list, "RHAH_Menu_Settings_Missing");
            }
            else
            {
                MenuControls.Anchor(list, "enable-new-content");
                MenuControls.Checkbox(
                    list,
                    "RHAH_Settings_EnableNewContent".Translate(),
                    ref settings.enableNewContent,
                    "RHAH_Settings_EnableNewContent_Tooltip".Translate());
            }

            Status(list, "RHAH_Menu_Field_Game",
                Current.Game == null ? "RHAH_Menu_NoGame".Translate() : "RHAH_Menu_GameLoaded".Translate());
            Status(list, "RHAH_Menu_Field_IrisMenus", irisVersion);
        }

        IEnumerable<MenuSearchEntry> SearchOverview()
        {
            yield return Entry("overview-version", "RHAH_Menu_Field_Version");
            yield return Entry("enable-new-content", "RHAH_Settings_EnableNewContent", "toggle content");
        }

        void DrawEvents(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Events");
            Note(list, "RHAH_Menu_Events_Note");
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
            Note(list, "RHAH_Menu_DevEvents_Note");
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
                height += ControlRow * 3f;
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

            if (RHAH_Scheduler.ExecuteDebugIncident(entry.DisplayId))
            {
                return "RHAH_Menu_Queue_Fired".Translate();
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
        }

        IEnumerable<MenuSearchEntry> SearchNarrative()
        {
            yield return Entry("narrative-outcome", "RHAH_Menu_Narrative_Outcome");
        }

        static void DrawOther(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Other");
        }

        static IEnumerable<MenuSearchEntry> SearchOther()
        {
            yield break;
        }


        void DrawRelief(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Relief");
            Note(list, "RHAH_Menu_Relief_Note");
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
            DrawFoodList(list, settings);
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

        static void DrawFoodList(Listing_Standard list, RHAH_Settings settings)
        {
            MenuControls.Anchor(list, "relief-foods", 52f);
            Note(list, "RHAH_Settings_ReliefFoods_Tooltip");
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
            List<ThingDef> listed = new List<ThingDef>();
            RHAH_ReliefFood.AppendCandidateFoods(listed);
            for (int i = 0; i < listed.Count; i++)
            {
                ThingDef food = listed[i];
                bool enabled = settings.IsReliefFoodEnabled(food.defName);
                MenuControls.Anchor(list, "relief-food-" + food.defName);
                MenuControls.Checkbox(list, food.LabelCap, ref enabled);
                settings.SetReliefFoodEnabled(food.defName, enabled);
            }
        }

        static IEnumerable<MenuSearchEntry> SearchRelief()
        {
            yield return Entry("relief-enabled", "RHAH_Settings_ReliefEnabled");
            yield return Entry("relief-outside", "RHAH_Settings_EatOutsideRelief");
            yield return Entry("relief-foods", "RHAH_Settings_ReliefFoods");
        }
        void DrawCompatDiagnostics(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_CompatDiagnostics");
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
            yield return Entry("compat-iris", "RHAH_Menu_CompatDiagnostics");
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
            Unavailable(list, "RHAH_Menu_Ending", "RHAH_Menu_Ending_Gap");
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

            Note(list, "RHAH_Menu_Genes_Note");
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

            for (int i = 0; i < xenotypes.Count; i++)
            {
                DrawXenotypeBar(list, settings, xenotypes[i], total);
            }

            DrawMissingXenotypes(list, settings, xenotypes);
            DrawJoinableXenotypes(list, settings);
            DrawGeneSwitches(list, settings);
            RHAH_Mod.Settings.Write();
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

        static void DrawJoinableXenotypes(Listing_Standard list, RHAH_Settings settings)
        {
            List<XenotypeDef> available = RHAH_XenotypeResolver.AvailableToJoin(settings);
            if (available.Count == 0)
            {
                return;
            }

            Section(list, "RHAH_Menu_Genes_Join");
            for (int i = 0; i < available.Count; i++)
            {
                XenotypeDef xenotype = available[i];
                if (list.ButtonText("RHAH_Menu_Genes_JoinOne".Translate(xenotype.LabelCap)))
                {
                    settings.SetXenotypeEnabled(xenotype.defName, true);
                }
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
            Note(list, "RHAH_Menu_PawnHistory_Note");
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
            Note(list, "RHAH_Menu_Experimental_Note");
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
            MenuControls.Checkbox(list, "RHAH_Settings_FamilyDrop".Translate(), ref settings.familyDropEnabled, "RHAH_Settings_FamilyDrop_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_MotherFeed".Translate(), ref settings.motherFeedEnabled, "RHAH_Settings_MotherFeed_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_PrisonerScavenge".Translate(), ref settings.prisonerScavengeEnabled, "RHAH_Settings_PrisonerScavenge_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_TailBite".Translate(), ref settings.tailBiteEnabled, "RHAH_Settings_TailBite_Tooltip".Translate());
            MenuControls.Checkbox(list, "RHAH_Settings_Broadcast".Translate(), ref settings.broadcastEnabled, "RHAH_Settings_Broadcast_Tooltip".Translate());
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
            string positiveBuffer = settings.positiveIncidentDays.ToString("0.#");
            float positiveDays = settings.positiveIncidentDays;
            MenuControls.Number(
                list,
                "RHAH_Menu_Frequency_Positive".Translate(),
                ref positiveDays,
                ref positiveBuffer,
                0,
                (int)RHAH_IncidentSchedule.MaxDays);
            settings.positiveIncidentDays = RHAH_IncidentSchedule.ClampDays(positiveDays);

            MenuControls.Anchor(list, "frequency-negative");
            string negativeBuffer = settings.negativeIncidentDays.ToString("0.#");
            float negativeDays = settings.negativeIncidentDays;
            MenuControls.Number(
                list,
                "RHAH_Menu_Frequency_Negative".Translate(),
                ref negativeDays,
                ref negativeBuffer,
                0,
                (int)RHAH_IncidentSchedule.MaxDays);
            settings.negativeIncidentDays = RHAH_IncidentSchedule.ClampDays(negativeDays);
            Note(list, "RHAH_Menu_Frequency_Gap");
            RHAH_IrisMenusWidgets.OccurrenceCurve(
                list,
                "frequency-curve",
                settings.positiveIncidentDays,
                settings.negativeIncidentDays);
            list.Gap(4f);
        }

        IEnumerable<MenuSearchEntry> SearchFrequency()
        {
            yield return Entry("frequency-positive", "RHAH_Menu_Frequency_Positive");
            yield return Entry("frequency-negative", "RHAH_Menu_Frequency_Negative");
            yield return Entry("frequency-curve", "RHAH_Menu_Frequency_Curve");
        }

        void DrawDeveloper(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Developer");
            Note(list, "RHAH_Menu_Developer_Note");
            Status(list, "RHAH_Menu_Field_Version", ContentFinderVersion());
            Status(list, "RHAH_Menu_Field_IrisMenus", irisVersion);
            if (!string.IsNullOrEmpty(selectedPawnLabel))
            {
                Status(list, "RHAH_Menu_Developer_LastPawn", selectedPawnLabel);
            }
        }

        IEnumerable<MenuSearchEntry> SearchDeveloper()
        {
            yield return Entry("developer-version", "RHAH_Menu_Field_Version");
        }

        static IEnumerable<MenuSearchEntry> SearchEnding()
        {
            yield return Entry("ending-gap", "RHAH_Menu_Ending_Gap");
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
