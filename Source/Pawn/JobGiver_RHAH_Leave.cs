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
            if (!HungerAndHavocApi.IsVisitor(pawn))
            {
                return null;
            }

            // 离场走 ExitMap 或饱食后离开闸门
            bool exit = HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.ExitMap);
            bool fedLeave = HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.LeaveAfterFed);
            if (!exit && !fedLeave)
            {
                return null;
            }

            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (fedLeave && !exit && settings != null && !settings.leaveAfterFed)
            {
                return null;
            }

            if (pawn.Map == null || JobDefOf.Goto == null)
            {
                return null;
            }

            if (!RHAH_ChildMovement.CanWalkOut(pawn))
            {
                return CarryDependent(pawn);
            }

            IntVec3 spot;
            if (!RCellFinder.TryFindBestExitSpot(pawn, out spot))
            {
                return null;
            }

            HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Leaving);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, spot);
            job.exitMapOnArrival = true;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            return job;
        }

        static Job CarryDependent(Verse.Pawn pawn)
        {
            if (pawn.Downed || pawn.CarriedBy != null || !HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Carry))
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

                if (RHAH_ChildMovement.CanWalkOut(child) || !HungerAndHavocApi.IsVisitor(child))
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
