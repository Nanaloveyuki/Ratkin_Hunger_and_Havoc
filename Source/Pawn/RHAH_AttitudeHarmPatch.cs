using HarmonyLib;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    // 原版伤害不会按批次改态度 这里只接玩家外部暴力
    [HarmonyPatch(typeof(Thing), nameof(Thing.PreApplyDamage))]
    internal static class RHAH_AttitudeHarmPatch
    {
        static void Postfix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed || dinfo.Def == null || !dinfo.Def.ExternalViolenceFor(__instance))
            {
                return;
            }

            Verse.Pawn instigator = dinfo.Instigator as Verse.Pawn;
            if (instigator?.Faction != Faction.OfPlayer)
            {
                return;
            }

            Verse.Pawn harmed = __instance as Verse.Pawn;
            if (harmed == null)
            {
                return;
            }

            RHAH_BatchAttitude.TryShift(harmed, false);
        }
    }
}
