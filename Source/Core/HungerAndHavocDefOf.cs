using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Core
{
    [DefOf]
    public static class HungerAndHavocDefOf
    {
        public static HediffDef RHAH_HungerMark;
        public static JobDef RHAH_Beg;
        public static JobDef RHAH_Gnaw;
        public static DutyDef RHAH_VisitorSeek;
        public static DutyDef RHAH_VisitorLeave;
        public static ThinkTreeDef RHAH_VisitorFallback;

        static HungerAndHavocDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HungerAndHavocDefOf));
        }
    }
}
