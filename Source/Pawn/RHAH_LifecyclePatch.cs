using HarmonyLib;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using RimWorld;
using Verse;
using Verse.AI;
namespace HungerAndHavoc.Pawn
{
    // 原版吃完一口后才记吃饱 啃树皮不走这条
    [HarmonyPatch(typeof(Toils_Ingest), nameof(Toils_Ingest.FinalizeIngest))]
    internal static class RHAH_IngestPatch
    {
        static void Postfix(Verse.Pawn ingester, Toil __result)
        {
            if (__result == null || ingester == null || !RHAH_Api.IsVisitor(ingester))
            {
                return;
            }

            __result.AddFinishAction(delegate
            {
                RHAH_Feeding.TryComplete(ingester);
            });
        }
    }

    // 原版死亡不会改来源生命周期 这里只收本模组仍活跃的来客
    [HarmonyPatch(typeof(Verse.Pawn), nameof(Verse.Pawn.Kill))]
    internal static class RHAH_DeathPatch
    {
        static void Postfix(Verse.Pawn __instance)
        {
            if (__instance == null || !__instance.Dead)
            {
                return;
            }

            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(__instance);
            if (comp == null || !comp.State.IsActiveVisitor)
            {
                return;
            }

            RHAH_Api.SetLifecycle(__instance, RHAH_Lifecycle.Dead);
            RHAH_VisitorGroup.NotifyDead(__instance);
        }
    }
}
