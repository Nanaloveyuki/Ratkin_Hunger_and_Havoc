using HarmonyLib;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    // 十五天吃满三块后原版仍会把它当食物 这里只拒观音土
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), typeof(Verse.Pawn), typeof(Thing), typeof(Verse.Pawn), typeof(bool), typeof(bool))]
    internal static class RHAH_ClayEatThingPatch
    {
        static void Postfix(Verse.Pawn p, Thing food, ref bool __result)
        {
            if (__result && !Hediff_RHAH_ClaySatiety.CanEat(p, food))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.WillEat), typeof(Verse.Pawn), typeof(ThingDef), typeof(Verse.Pawn), typeof(bool), typeof(bool))]
    internal static class RHAH_ClayEatDefPatch
    {
        static void Postfix(Verse.Pawn p, ThingDef food, ref bool __result)
        {
            if (__result && !Hediff_RHAH_ClaySatiety.CanEat(p, food))
            {
                __result = false;
            }
        }
    }
}
