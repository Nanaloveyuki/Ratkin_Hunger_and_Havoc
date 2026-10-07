using HungerAndHavoc.Api;
using RimWorld;
using Verse.AI.Group;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using Verse;
using Verse.AI;

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
            if (kind == RHAH_StayKind.Shelter || kind == RHAH_StayKind.Recruit)
            {
                if (settings == null || settings.shortStayLockApparel)
                {
                    LockShortStayApparel(pawn, comp);
                }
            }
            else
            {
                UnlockShortStayApparel(pawn, comp);
            }
            if (pawn.Faction != Faction.OfPlayer)
            {
                RHAH_VisitorRules.ApplyPlayerIdeo(
                    pawn,
                    settings == null ? 100 : settings.playerIdeoPercent,
                    Rand.Value);
                pawn.SetFaction(Faction.OfPlayer);
            }

            ClearTrade(pawn, RHAH_ReleaseReason.Recruited, false);
            Compat.RHAH_LeashBridge.ClearDeparture(pawn);
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
                if (RHAH_VisitorRules.RestoresPlayerFaction(
                    comp.State.stayKind,
                    now,
                    deadline,
                    downed,
                    pawn.Dead,
                    pawn.IsPrisoner,
                    pawn.IsSlave,
                    pawn.Faction == Faction.OfPlayer) &&
                    pawn.Faction != Faction.OfPlayer)
                {
                    pawn.SetFaction(Faction.OfPlayer);
                }

                return false;
            }

            UnlockShortStayApparel(pawn, comp);
            comp.SetStay((int)RHAH_StayKind.None, -1, 0);
            if (pawn.Faction == Faction.OfPlayer)
            {
                pawn.SetFaction(null);
            }

            RHAH_VisitorGroup.NotifyReleased(pawn);
            RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving);
            return true;
        }

        internal static void LockShortStayApparel(Verse.Pawn pawn, CompRHAH_Pawn comp)
        {
            if (pawn.apparel == null)
            {
                return;
            }

            var worn = pawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                Apparel apparel = worn[i];
                if (!pawn.apparel.IsLocked(apparel))
                {
                    if (comp.shortStayLockedApparel == null)
                    {
                        comp.shortStayLockedApparel = new System.Collections.Generic.List<Apparel>();
                    }
                    comp.shortStayLockedApparel.Add(apparel);
                }
            }
            pawn.apparel.LockAll();
        }

        internal static void UnlockShortStayApparel(Verse.Pawn pawn, CompRHAH_Pawn comp)
        {
            if (comp.shortStayLockedApparel == null)
            {
                return;
            }

            for (int i = 0; i < comp.shortStayLockedApparel.Count; i++)
            {
                Apparel apparel = comp.shortStayLockedApparel[i];
                if (apparel != null)
                {
                    pawn.apparel?.Unlock(apparel);
                }
            }
            comp.shortStayLockedApparel = null;
        }

        internal static int Kind(Verse.Pawn pawn)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            return comp == null ? (int)RHAH_StayKind.None : comp.State.stayKind;
        }

        internal static bool BlocksAssignedWork(Verse.Pawn pawn, int now)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            if (comp == null || pawn == null)
            {
                return false;
            }

            return RHAH_VisitorRules.BlocksAssignedWork(
                comp.State.stayKind,
                now,
                comp.State.leaveAfterGameTick,
                pawn.Downed);
        }

        internal static bool RejectsAssignedJob(Verse.Pawn pawn, Job job, ThinkNode giver)
        {
            if (pawn == null || job == null || job.playerForced || RHAH_VisitorRules.IsPassiveDefense(job.def))
            {
                return false;
            }

            if (RHAH_VisitorRules.IsNeedFood(giver) || job.def == JobDefOf.Ingest)
            {
                return false;
            }

            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            return BlocksAssignedWork(pawn, now);
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
