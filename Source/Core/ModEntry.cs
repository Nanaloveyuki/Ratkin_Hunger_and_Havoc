using UnityEngine;
using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocMod : Mod
    {
        public static HungerAndHavocSettings Settings;

        public HungerAndHavocMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<HungerAndHavocSettings>();
#if RHAH_IRISMENUS
            Pawn.Compat.RHAH_IrisMenusCompat.TryRegister(this);
#endif
        }

        public override string SettingsCategory()
        {
            return "RHAH_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("RHAH_Settings_EnableNewContent".Translate(), ref Settings.enableNewContent,
                "RHAH_Settings_EnableNewContent_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_OptimizeGeneration".Translate(), ref Settings.optimizeGeneration,
                "RHAH_Settings_OptimizeGeneration_Tooltip".Translate());
            bool relief = Settings.reliefEnabled;
            listing.CheckboxLabeled("RHAH_Settings_ReliefEnabled".Translate(), ref relief,
                "RHAH_Settings_ReliefEnabled_Tooltip".Translate());
            if (relief != Settings.reliefEnabled)
            {
                Settings.reliefEnabled = relief;
                Settings.InvalidateReliefSearch();
            }

            bool outside = Settings.allowEatOutsideRelief;
            listing.CheckboxLabeled("RHAH_Settings_EatOutsideRelief".Translate(), ref outside,
                "RHAH_Settings_EatOutsideRelief_Tooltip".Translate());
            if (outside != Settings.allowEatOutsideRelief)
            {
                Settings.allowEatOutsideRelief = outside;
                Settings.InvalidateReliefSearch();
            }

            bool ignore = Settings.ignoreReliefAfterFed;
            listing.CheckboxLabeled("RHAH_Settings_IgnoreReliefAfterFed".Translate(), ref ignore,
                "RHAH_Settings_IgnoreReliefAfterFed_Tooltip".Translate());
            if (ignore != Settings.ignoreReliefAfterFed)
            {
                Settings.ignoreReliefAfterFed = ignore;
                Settings.InvalidateReliefSearch();
            }

            bool leave = Settings.leaveAfterFed;
            listing.CheckboxLabeled("RHAH_Settings_LeaveAfterFed".Translate(), ref leave,
                "RHAH_Settings_LeaveAfterFed_Tooltip".Translate());
            if (leave != Settings.leaveAfterFed)
            {
                Settings.leaveAfterFed = leave;
                Settings.InvalidateReliefSearch();
            }
            listing.CheckboxLabeled("RHAH_Settings_AidRequests".Translate(), ref Settings.aidRequestsEnabled,
                "RHAH_Settings_AidRequests_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_IntelTrades".Translate(), ref Settings.intelTradesEnabled,
                "RHAH_Settings_IntelTrades_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_VisitorChoices".Translate(), ref Settings.visitorChoicesEnabled,
                "RHAH_Settings_VisitorChoices_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_FamilyDrop".Translate(), ref Settings.familyDropEnabled,
                "RHAH_Settings_FamilyDrop_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_MotherFeed".Translate(), ref Settings.motherFeedEnabled,
                "RHAH_Settings_MotherFeed_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_PrisonerScavenge".Translate(), ref Settings.prisonerScavengeEnabled,
                "RHAH_Settings_PrisonerScavenge_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_TailBite".Translate(), ref Settings.tailBiteEnabled,
                "RHAH_Settings_TailBite_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_Broadcast".Translate(), ref Settings.broadcastEnabled,
                "RHAH_Settings_Broadcast_Tooltip".Translate());
            listing.CheckboxLabeled("RHAH_Settings_Stagger".Translate(), ref Settings.staggerGeneration,
                "RHAH_Settings_Stagger_Tooltip".Translate());

            listing.End();
            Settings.Write();
        }
    }
}
