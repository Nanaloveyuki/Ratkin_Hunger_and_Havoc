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

            RHAH_Settings settings = RHAH_Mod.Settings;
            bool wait = settings == null || settings.waitWhenNoFood;
            float days = settings == null ? RHAH_VisitorRules.DefaultNoFoodWaitDays : settings.noFoodWaitDays;
            int budget = RHAH_VisitorRules.WaitTicks(wait, days);
            if (budget <= 0)
            {
                return null;
            }

            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
            int deadline = comp == null ? -1 : comp.State.foodWaitUntilTick;
            if (deadline >= 0 && now >= deadline)
            {
                return null;
            }

            if (comp != null && deadline < 0)
            {
                comp.SetFoodWait(RHAH_VisitorRules.NextWaitTick(now, -1, false, true, days));
            }


            IntVec3 spot = WaitSpot(pawn);
            if (!spot.IsValid)
            {
                return null;
            }

            if (spot != pawn.Position && JobDefOf.Goto != null)
            {
                Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, spot);
                gotoJob.expiryInterval = RHAH_ReliefFood.RetryBaseTicks;
                gotoJob.checkOverrideOnExpire = true;
                return gotoJob;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Wait);
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
                if (!cell.Standable(pawn.Map) ||
                    !pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly))
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
