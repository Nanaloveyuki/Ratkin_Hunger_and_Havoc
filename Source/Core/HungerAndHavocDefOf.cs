using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Core
{
    [DefOf]
    public static class HungerAndHavocDefOf
    {
        public static HediffDef RHAH_HungerMark;
        public static HediffDef RHAH_Plague;
        public static HediffDef RHAH_RefeedingSyndrome;
        public static JobDef RHAH_Beg;
        public static JobDef RHAH_Gnaw;
        public static DutyDef RHAH_VisitorSeek;
        public static DutyDef RHAH_VisitorLeave;
        public static ThinkTreeDef RHAH_VisitorFallback;
        public static FactionDef RHAH_Faction_Hostile;
        public static FactionDef RHAH_Faction_LeaningHostile;
        public static FactionDef RHAH_Faction_Neutral;
        public static FactionDef RHAH_Faction_LeaningFriendly;
        public static FactionDef RHAH_Faction_Friendly;

        static HungerAndHavocDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HungerAndHavocDefOf));
        }
    }
}
