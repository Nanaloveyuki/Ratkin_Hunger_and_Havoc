using HungerAndHavoc.Identity;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Pawn.Compat;
using RimWorld;
using Verse.AI.Group;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Leave : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return TryCreate(pawn);
        }

        // 供无 Lord 的总 JobGiver 调度
        internal static Job TryCreate(Verse.Pawn pawn)
        {
            if (!RHAH_Api.IsVisitor(pawn))
            {
                return null;
            }

            // 离场走 ExitMap 或饱食后离开闸门
            bool exit = RHAH_Api.Allows(pawn, RHAH_BehaviorGate.ExitMap);
            bool fedLeave = RHAH_Api.Allows(pawn, RHAH_BehaviorGate.LeaveAfterFed);
            if (!exit && !fedLeave)
            {
                return null;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            bool fedDue = snapshot != null && RHAH_VisitorRules.FedLeaveDue(
                settings == null || settings.leaveAfterFed,
                snapshot.HasBeenFed,
                now,
                snapshot.LeaveAfterGameTick,
                pawn.Downed);
            bool foodWaitExpired = snapshot != null && RHAH_VisitorRules.NoFoodWaitExpired(
                true,
                snapshot.HasBeenFed,
                now,
                FoodWaitDeadline(pawn));
            if (fedLeave && !exit && !fedDue && !foodWaitExpired)
            {
                return null;
            }

            if (pawn.Downed)
            {
                return null;
            }

            if (pawn.Map == null || JobDefOf.Goto == null)
            {
                return null;
            }

            if (RHAH_ChildMovement.CanWalkOut(pawn))
            {
                Job carry = CarryDependent(pawn);
                if (carry != null)
                {
                    return carry;
                }
            }
            else
            {
                return null;
            }

            IntVec3 spot;
            if (!RCellFinder.TryFindBestExitSpot(pawn, out spot))
            {
                return null;
            }

            RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving);
            RHAH_LeashBridge.ClearDeparture(pawn);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
            job.exitMapOnArrival = true;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            return job;
        }

        static int FoodWaitDeadline(Verse.Pawn pawn)
        {
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            return comp == null ? -1 : comp.State.foodWaitUntilTick;
        }

        static Job CarryDependent(Verse.Pawn pawn)
        {
            if (pawn.Downed ||
                pawn.CarriedBy != null ||
                !RHAH_ChildMovement.CanWalkOut(pawn) ||
                !RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Carry))
            {
                return null;
            }

            Lord lord = pawn.GetLord();
            if (lord == null || lord.ownedPawns == null || JobDefOf.CarryDownedPawnToExit == null)
            {
                return null;
            }

            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn child = lord.ownedPawns[i];
                if (child == null || child == pawn || child.Map != pawn.Map || child.CarriedBy != null)
                {
                    continue;
                }

                if (RHAH_ChildMovement.CanWalkOut(child) || !RHAH_Api.IsVisitor(child))
                {
                    continue;
                }

                if (!pawn.CanReach(child, PathEndMode.Touch, Danger.Deadly) || !pawn.CanReserve(child))
                {
                    continue;
                }

                return JobMaker.MakeJob(JobDefOf.CarryDownedPawnToExit, child);
            }

            return null;
        }
    }
}
