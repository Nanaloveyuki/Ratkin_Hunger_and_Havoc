using HungerAndHavoc.Identity;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_WaitFood : ThinkNode_JobGiver
    {
        internal const int WaitTicks = 30000;

        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return TryCreate(pawn);
        }

        internal static Job TryCreate(Verse.Pawn pawn)
        {
            if (!RHAH_Api.IsVisitor(pawn) || pawn.Map == null || pawn.Downed)
            {
                return null;
            }

            if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.FeedFromRelief))
            {
                return null;
            }

            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot == null || snapshot.HasBeenFed || RHAH_ReliefFood.MayEatOutside(pawn))
            {
                return null;
            }

            if (JobDefOf.Wait == null)
            {
                return null;
            }

            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            if (snapshot.LeaveAfterGameTick >= 0 && now >= snapshot.LeaveAfterGameTick)
            {
                return null;
            }

            if (snapshot.LeaveAfterGameTick < 0)
            {
                CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
                if (comp != null)
                {
                    comp.SetLeaveAfter(now + WaitTicks);
                }
            }


            IntVec3 spot = WaitSpot(pawn);
            Job job = JobMaker.MakeJob(JobDefOf.Wait, spot);
            job.expiryInterval = RHAH_ReliefFood.RetryBaseTicks;
            job.checkOverrideOnExpire = true;
            return job;
        }

        static IntVec3 WaitSpot(Verse.Pawn pawn)
        {
            Area_RHAH_Relief area = RHAH_ReliefArea.Get(pawn.Map);
            if (area == null || area.TrueCount == 0)
            {
                return pawn.Position;
            }

            IntVec3 best = IntVec3.Invalid;
            float bestDist = float.MaxValue;
            foreach (IntVec3 cell in area.ActiveCells)
            {
                if (!cell.Standable(pawn.Map))
                {
                    continue;
                }

                float dist = cell.DistanceToSquared(pawn.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = cell;
                }
            }

            return best.IsValid ? best : pawn.Position;
        }
    }
}
