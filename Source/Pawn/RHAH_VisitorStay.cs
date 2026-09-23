using HungerAndHavoc.Api;
using RimWorld;
using Verse.AI.Group;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using Verse;

namespace HungerAndHavoc.Pawn
{
    // 收留和雇佣到期后离场 倒地只暂停计时
    internal static class RHAH_VisitorStay
    {
        internal static bool Begin(Verse.Pawn pawn, RHAH_StayKind kind)
        {
            if (pawn == null || !RHAH_Api.Allows(pawn, kind == RHAH_StayKind.Hire ? RHAH_BehaviorGate.Hire : RHAH_BehaviorGate.JoinColony))
            {
                return false;
            }

            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return false;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            int shelter = settings == null ? RHAH_VisitorRules.DefaultShelterDays : settings.shelterDays;
            int hire = settings == null ? RHAH_VisitorRules.DefaultHireDays : settings.hireDays;
            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            int deadline = RHAH_VisitorRules.BeginStay(now, kind, shelter, hire);
            comp.SetStay((int)kind, deadline, deadline - now);
            if (pawn.Faction != Faction.OfPlayer)
            {
                pawn.SetFaction(Faction.OfPlayer);
            }

            ClearTrade(pawn, RHAH_ReleaseReason.Recruited, false);
            return true;
        }

        internal static bool Tick(Verse.Pawn pawn, int now)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null || comp.State.stayKind == (int)RHAH_StayKind.None || pawn.Dead)
            {
                return false;
            }

            bool downed = pawn.Downed;
            int remaining = RHAH_VisitorRules.RemainingStay(now, comp.State.leaveAfterGameTick, downed, comp.State.stayRemainingTicks);
            int deadline = RHAH_VisitorRules.ResumeStay(now, comp.State.leaveAfterGameTick, downed ? remaining : 0, downed);
            comp.SetStay(comp.State.stayKind, deadline, remaining);
            if (!RHAH_VisitorRules.StayExpired(now, deadline, downed))
            {
                return false;
            }

            comp.SetStay((int)RHAH_StayKind.None, -1, 0);
            if (pawn.Faction == Faction.OfPlayer)
            {
                pawn.SetFaction(null);
            }

            RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving);
            return true;
        }

        internal static void ClearTrade(Verse.Pawn pawn, RHAH_ReleaseReason reason, bool letterBatch)
        {
            if (pawn == null || !RHAH_VisitorRules.ClearsTrade(reason, letterBatch))
            {
                return;
            }

            Lord lord = pawn.GetLord();
            if (lord != null)
            {
                lord.RemovePawn(pawn);
            }
            pawn.jobs?.StopAll();
            if (pawn.mindState != null)
            {
                pawn.mindState.duty = null;
            }

        }
    }
}
