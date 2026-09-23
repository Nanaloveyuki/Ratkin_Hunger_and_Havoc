using System.Collections.Generic;
using HarmonyLib;
using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    // 原版俘虏不看来源闸门 这里只拦本模组来客
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.CapturedBy))]
    internal static class RHAH_CaptureGatePatch
    {
        static bool Prefix(Pawn_GuestTracker __instance, Faction by)
        {
            return RHAH_ColonyGates.AllowsCapture(Guest(__instance), by);
        }

        static Verse.Pawn Guest(Pawn_GuestTracker guest)
        {
            return guest == null
                ? null
                : AccessTools.Field(typeof(Pawn_GuestTracker), "pawn")?.GetValue(guest) as Verse.Pawn;
        }
    }

    // 原版角色买卖不看来源闸门 关闸后这笔不成交
    [HarmonyPatch(typeof(Tradeable_Pawn), "ResolveTrade")]
    internal static class RHAH_TradePawnGatePatch
    {
        static bool Prefix(Tradeable_Pawn __instance)
        {
            return RHAH_ColonyGates.AllowsTrade(__instance);
        }
    }

    internal static class RHAH_ColonyGates
    {
        internal static bool AllowsCapture(Verse.Pawn pawn, Faction by)
        {
            if (!RHAH_Api.IsOrigin(pawn) || by == null || by != Faction.OfPlayerSilentFail)
            {
                return true;
            }

            if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Imprison))
            {
                return false;
            }

            RHAH_Api.ReleaseToColony(pawn, RHAH_ReleaseReason.Imprisoned);
            return true;
        }

        internal static bool AllowsTrade(Tradeable_Pawn trade)
        {
            if (trade == null || trade.ActionToDo == TradeAction.None)
            {
                return true;
            }

            int count = trade.ActionToDo == TradeAction.PlayerSells
                ? trade.CountToTransferToDestination
                : trade.CountToTransferToSource;
            List<Thing> things = trade.ActionToDo == TradeAction.PlayerSells
                ? trade.thingsColony
                : trade.thingsTrader;
            if (things == null || count <= 0)
            {
                return true;
            }

            int seen = 0;
            foreach (Thing thing in things)
            {
                if (seen >= count)
                {
                    break;
                }

                seen++;
                Verse.Pawn pawn = thing as Verse.Pawn;
                if (!RHAH_Api.IsOrigin(pawn))
                {
                    continue;
                }

                if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Transfer))
                {
                    return false;
                }
            }

            seen = 0;
            foreach (Thing thing in things)
            {
                if (seen >= count)
                {
                    break;
                }

                seen++;
                Verse.Pawn pawn = thing as Verse.Pawn;
                if (RHAH_Api.IsOrigin(pawn))
                {
                    RHAH_Api.ReleaseToColony(pawn, RHAH_ReleaseReason.ModRequest);
                }
            }

            return true;
        }
    }
}
