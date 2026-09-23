using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Core
{
    [DefOf]
    public static class RHAH_DefOf
    {
        public static HediffDef RHAH_HungerMark;
        public static HediffDef RHAH_Plague;
        public static HediffDef RHAH_RefeedingSyndrome;
        public static HediffDef RHAH_GnawedBark;
        public static HediffDef RHAH_GnawedWall;
        public static HediffDef RHAH_OvergnawedWall;
        public static HediffDef RHAH_ClaySatiety;
        public static ThingDef RHAH_GuanyinTu;
        public static JobDef RHAH_Beg;
        public static JobDef RHAH_Gnaw;
        public static JobDef RHAH_DropChild;
        public static JobDef RHAH_MotherFeed;
        public static JobDef RHAH_Scavenge;
        public static JobDef RHAH_TailBite;
        public static DutyDef RHAH_VisitorSeek;
        public static DutyDef RHAH_VisitorLeave;
        public static ThinkTreeDef RHAH_VisitorFallback;
        public static FactionDef RHAH_Faction_Hostile;
        public static FactionDef RHAH_Faction_LeaningHostile;
        public static FactionDef RHAH_Faction_Neutral;
        public static FactionDef RHAH_Faction_LeaningFriendly;
        public static FactionDef RHAH_Faction_Friendly;

        static RHAH_DefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RHAH_DefOf));
        }
    }
}
