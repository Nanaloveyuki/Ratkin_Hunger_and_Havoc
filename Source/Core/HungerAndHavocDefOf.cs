using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    [DefOf]
    public static class HungerAndHavocDefOf
    {
        public static HediffDef RHAH_HungerMark;

        static HungerAndHavocDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HungerAndHavocDefOf));
        }
    }
}
