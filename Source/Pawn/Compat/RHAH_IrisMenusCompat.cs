extern alias iris;
using System;
using System.Collections.Generic;
using System.Linq;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
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

        internal static void TryRegister(HungerAndHavocMod owner)
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
        const float RowHeight = 28f;
        const int FrequencyDefaultDays = 15;
        const int FrequencyMaxDays = 60;

        readonly string irisVersion;
        readonly Dictionary<string, string> debugResults = new Dictionary<string, string>();
        string frequencyDaysBuffer = FrequencyDefaultDays.ToString();
        int frequencyDays = FrequencyDefaultDays;
        string selectedPawnLabel = string.Empty;

        internal RHAH_IrisMenusPages(string irisVersion)
        {
            this.irisVersion = irisVersion;
        }

        internal void Register(Mod owner)
        {
            RegisterPage(owner, "overview", "RHAH_Menu_Overview", DrawOverview, SearchOverview);
            RegisterPage(owner, "events", "RHAH_Menu_Events", DrawEvents, SearchCatalogEvents);
            RegisterPage(owner, "pawns", "RHAH_Menu_Pawns", DrawPawns, SearchPawns);
            RegisterPage(owner, "narrative", "RHAH_Menu_Narrative", DrawNarrative, SearchNarrative);
            RegisterPage(owner, "other", "RHAH_Menu_Other", DrawOther, SearchOther);
            RegisterPage(owner, "compatibility", "RHAH_Menu_Compatibility", DrawCompatibility, SearchCompatibility);
            RegisterPage(owner, "diagnostics", "RHAH_Menu_Diagnostics", DrawDiagnostics, SearchDiagnostics);
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
            Status(list, "RHAH_Menu_Field_NewContent",
                HungerAndHavocRuntime.AllowsNewContent ? "RHAH_Menu_On".Translate() : "RHAH_Menu_Off".Translate());
            Status(list, "RHAH_Menu_Field_Game",
                Current.Game == null ? "RHAH_Menu_NoGame".Translate() : "RHAH_Menu_GameLoaded".Translate());
            Status(list, "RHAH_Menu_Field_IrisMenus", irisVersion);
        }

        IEnumerable<MenuSearchEntry> SearchOverview()
        {
            yield return Entry("overview-version", "RHAH_Menu_Field_Version");
            yield return Entry("overview-content", "RHAH_Menu_Field_NewContent");
        }

        void DrawEvents(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Events");
            IReadOnlyList<HungerIncidentEntry> entries = HungerIncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                DrawIncident(list, entries[i], false);
            }
        }

        void DrawDevEvents(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_DevEvents");
            Note(list, "RHAH_Menu_DevEvents_Note");
            IReadOnlyList<HungerIncidentEntry> entries = HungerIncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                DrawIncident(list, entries[i], true);
            }
        }

        void DrawIncident(Listing_Standard list, HungerIncidentEntry entry, bool debug)
        {
            string anchor = (debug ? "dev-" : "event-") + entry.DisplayId;
            MenuControls.Anchor(list, anchor, debug ? 78f : 52f);
            Rect card = list.GetRect(debug ? 74f : 48f);
            Widgets.DrawBox(card);
            Rect inner = card.ContractedBy(4f);
            Widgets.Label(new Rect(inner.x, inner.y, inner.width, 22f),
                entry.DisplayId + "  " + entry.LabelKey.Translate());
            Widgets.Label(new Rect(inner.x, inner.y + 20f, inner.width, 22f),
                "RHAH_Menu_EventMeta".Translate(
                    FamilyLabel(entry.Family),
                    OriginLabel(entry.Origin),
                    CategoryLabel(entry.Category),
                    TargetLabel(entry.Target),
                    entry.BroadcastEligible ? "RHAH_Menu_Yes".Translate() : "RHAH_Menu_No".Translate(),
                    entry.DebugPoints.ToString("0")));
            if (debug)
            {
                Rect button = new Rect(inner.x, inner.y + 44f, 160f, 24f);
                if (Widgets.ButtonText(button, "RHAH_Menu_Queue".Translate()))
                {
                    debugResults[entry.DisplayId] = Queue(entry);
                }

                string result;
                if (debugResults.TryGetValue(entry.DisplayId, out result))
                {
                    Widgets.Label(new Rect(button.xMax + 8f, button.y, inner.width - 168f, 24f), result);
                }
            }

            list.Gap(4f);
        }

        static string Queue(HungerIncidentEntry entry)
        {
            if (!HungerAndHavocRuntime.AllowsNewContent)
            {
                return "RHAH_Menu_Queue_Disabled".Translate();
            }

            if (Current.Game == null)
            {
                return "RHAH_Menu_Queue_NoGame".Translate();
            }

            if (entry.Target == HungerIncidentTarget.Map && HungerMapResolver.Resolve() == null)
            {
                return "RHAH_Menu_Queue_NoMap".Translate();
            }

            if (HungerAndHavocScheduler.QueueDebugIncident(entry.DisplayId))
            {
                return "RHAH_Menu_Queue_Queued".Translate();
            }

            return "RHAH_Menu_Queue_Rejected".Translate();
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
            IReadOnlyList<HungerIncidentEntry> entries = HungerIncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                HungerIncidentEntry entry = entries[i];
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
                if (pawn == null || pawn.Destroyed || !HungerAndHavocApi.IsOrigin(pawn))
                {
                    continue;
                }

                any = true;
                DrawPawn(list, pawn, HungerAndHavocApi.Get(pawn));
            }

            if (!any)
            {
                Empty(list, "RHAH_Menu_Pawns_None");
            }
        }

        void DrawPawn(Listing_Standard list, Verse.Pawn pawn, IHungerPawn snapshot)
        {
            if (pawn == null || pawn.Destroyed || snapshot == null)
            {
                Empty(list, "RHAH_Menu_Pawns_Invalid");
                return;
            }

            string anchor = "pawn-" + pawn.thingIDNumber;
            MenuControls.Anchor(list, anchor, 72f);
            Rect card = list.GetRect(68f);
            Widgets.DrawBox(card);
            Rect inner = card.ContractedBy(4f);
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
            list.Gap(4f);
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

        void DrawOther(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Other");
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
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

        IEnumerable<MenuSearchEntry> SearchOther()
        {
            yield return Entry("enable-new-content", "RHAH_Settings_EnableNewContent", "toggle content");
        }

        void DrawCompatibility(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Compatibility");
            DrawModRow(list, "ratkin", "NewRatkinPlus", "Solaris.RatkinRaceMod", true);
            DrawModRow(list, "harmony", "Harmony", "brrainz.harmony", true);
            DrawModRow(list, "biotech", "Biotech", "Ludeon.RimWorld.Biotech", true);
            DrawModRow(list, "iris", "IrisMenus", PackageId(), false);
            DrawModRow(list, "toddlers", "Toddlers", "cyanobot.toddlers", false);
            DrawModRow(list, "leash", "Lead Your Pet", "nanaloveyuki.leadyourpet.continued", false);
            DrawModRow(list, "prisoner", "Prisoner Work", "LeZhizhong.PrisonerWorkExpansion", false);
        }

        void DrawModRow(Listing_Standard list, string anchor, string label, string packageId, bool required)
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

        IEnumerable<MenuSearchEntry> SearchCompatibility()
        {
            yield return Entry("compat-iris", "RHAH_Menu_Compatibility");
        }

        void DrawDiagnostics(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_Diagnostics");
            Status(list, "RHAH_Menu_Field_IrisMenus", irisVersion);
            Status(list, "RHAH_Menu_Field_NewContent",
                HungerAndHavocRuntime.AllowsNewContent ? "RHAH_Menu_On".Translate() : "RHAH_Menu_Off".Translate());
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (game == null)
            {
                Empty(list, "RHAH_Menu_Diagnostics_NoGame");
                return;
            }

            Status(list, "RHAH_Menu_Diagnostics_Pending", game.PendingIncidentDisplayIds.Count.ToString());
            Status(list, "RHAH_Menu_Diagnostics_Batches", game.ActiveGenerationBatches.Count.ToString());
            Map map = HungerMapResolver.Resolve();
            Status(list, "RHAH_Menu_Diagnostics_Map",
                map == null ? "RHAH_Menu_Queue_NoMap".Translate() : map.uniqueID.ToString());
        }

        IEnumerable<MenuSearchEntry> SearchDiagnostics()
        {
            yield return Entry("diagnostics-pending", "RHAH_Menu_Diagnostics_Pending");
        }

        void DrawEnding(Listing_Standard list)
        {
            Unavailable(list, "RHAH_Menu_Ending", "RHAH_Menu_Ending_Gap");
        }

        void DrawGenes(Listing_Standard list)
        {
            Unavailable(list, "RHAH_Menu_Genes", "RHAH_Menu_Genes_Gap");
        }

        void DrawPawnHistory(Listing_Standard list)
        {
            Unavailable(list, "RHAH_Menu_PawnHistory", "RHAH_Menu_PawnHistory_Gap");
        }

        void DrawExperimental(Listing_Standard list)
        {
            Unavailable(list, "RHAH_Menu_Experimental", "RHAH_Menu_Experimental_Gap");
        }

        void DrawFrequency(Listing_Standard list)
        {
            Section(list, "RHAH_Menu_EventFrequency");
            Note(list, "RHAH_Menu_Frequency_Gap");
            MenuControls.Anchor(list, "frequency-days");
            string buffer = frequencyDaysBuffer;
            int days = frequencyDays;
            MenuControls.Number(
                list,
                "RHAH_Menu_Frequency_Days".Translate(),
                ref days,
                ref buffer,
                1,
                FrequencyMaxDays);
            frequencyDays = days;
            frequencyDaysBuffer = buffer;
            Rect plot = list.GetRect(160f);
            Widgets.DrawBox(plot);
            DrawCurve(plot.ContractedBy(8f), frequencyDays);
            Status(list, "RHAH_Menu_Frequency_Expression", "y = 0");
            list.Gap(4f);
        }

        static void DrawCurve(Rect plot, int days)
        {
            Widgets.DrawLineHorizontal(plot.x, plot.yMax, plot.width);
            Widgets.DrawLineVertical(plot.x, plot.y, plot.height);
            Widgets.Label(new Rect(plot.x + 4f, plot.y, 80f, 22f), "100%");
            Widgets.Label(new Rect(plot.xMax - 48f, plot.yMax - 22f, 48f, 22f), days + "d");
            Widgets.Label(new Rect(plot.x + 8f, plot.y + plot.height * 0.35f, plot.width - 16f, 44f),
                "RHAH_Menu_Frequency_Flat".Translate());
        }

        IEnumerable<MenuSearchEntry> SearchFrequency()
        {
            yield return Entry("frequency-days", "RHAH_Menu_Frequency_Days");
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
            yield return Entry("genes-gap", "RHAH_Menu_Genes_Gap");
        }

        static IEnumerable<MenuSearchEntry> SearchPawnHistory()
        {
            yield return Entry("history-gap", "RHAH_Menu_PawnHistory_Gap");
        }

        static IEnumerable<MenuSearchEntry> SearchExperimental()
        {
            yield return Entry("experimental-gap", "RHAH_Menu_Experimental_Gap");
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

        static void Empty(Listing_Standard list, string key)
        {
            MenuControls.Anchor(list, "empty-" + key);
            list.Label(key.Translate());
            list.Gap(4f);
        }

        static string ContentFinderVersion()
        {
            ModMetaData meta = ModLister.GetActiveModWithIdentifier(HungerAndHavocRuntime.PackageId, false);
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

        static string FamilyLabel(HungerIncidentFamily family)
        {
            return ("RHAH_Menu_Family_" + family).Translate();
        }

        static string OriginLabel(HungerIncidentOrigin origin)
        {
            return ("RHAH_Menu_Origin_" + origin).Translate();
        }

        static string CategoryLabel(HungerIncidentCategory category)
        {
            return ("RHAH_Menu_Category_" + category).Translate();
        }

        static string TargetLabel(HungerIncidentTarget target)
        {
            return ("RHAH_Menu_Target_" + target).Translate();
        }

        static string RoleLabel(HungerPawnRole role)
        {
            return ("RHAH_Role_" + role).Translate();
        }

        static string LifecycleLabel(HungerLifecycle lifecycle)
        {
            return ("RHAH_Menu_Lifecycle_" + lifecycle).Translate();
        }

        static string OutcomeLabel(NarrativeOutcome outcome)
        {
            return ("RHAH_Menu_Outcome_" + outcome).Translate();
        }
    }
}
