using HungerAndHavoc.Api;
using RimWorld;
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
            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.ExitMap) &&
                !HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.LeaveAfterFed))
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed || JobDefOf.Goto == null)
            {
                return null;
            }

            IntVec3 exit;
            if (!RCellFinder.TryFindBestExitSpot(pawn, out exit))
            {
                return null;
            }

            HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Leaving);
            Job job = JobMaker.MakeJob(JobDefOf.Goto, exit);
            job.exitMapOnArrival = true;
            job.locomotionUrgency = LocomotionUrgency.Jog;
            return job;
        }
    }
}
