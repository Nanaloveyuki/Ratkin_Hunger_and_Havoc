using HungerAndHavoc.Api;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Visitor : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            if (!HungerAndHavocApi.IsVisitor(pawn))
            {
                return null;
            }

            MarkSeekingFood(pawn);

            if (HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.FeedFromRelief))
            {
                Job feed = JobGiver_RHAH_Feed.TryCreate(pawn);
                if (feed != null)
                {
                    return feed;
                }
            }

            if (HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Beg))
            {
                Job beg = JobGiver_RHAH_Beg.TryCreate(pawn);
                if (beg != null)
                {
                    return beg;
                }
            }

            if (HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Steal))
            {
                Job steal = JobGiver_RHAH_Steal.TryCreate(pawn);
                if (steal != null)
                {
                    return steal;
                }
            }

            if (HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Gnaw))
            {
                Job gnaw = JobGiver_RHAH_Gnaw.TryCreate(pawn);
                if (gnaw != null)
                {
                    return gnaw;
                }
            }

            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.EatOutsideRelief))
            {
                Job wait = JobGiver_RHAH_WaitFood.TryCreate(pawn);
                if (wait != null)
                {
                    return wait;
                }
            }

            if (HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.LeaveAfterFed) ||
                HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.ExitMap))
            {
                Job leave = JobGiver_RHAH_Leave.TryCreate(pawn);
                if (leave != null)
                {
                    return leave;
                }
            }

            return null;
        }

        internal static void MarkSeekingFood(Verse.Pawn pawn)
        {
            IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
            if (snapshot != null && snapshot.Lifecycle == HungerLifecycle.Arriving)
            {
                HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.SeekingFood);
            }
        }

        internal static void MarkFed(Verse.Pawn pawn)
        {
            IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
            if (snapshot != null && snapshot.Lifecycle == HungerLifecycle.SeekingFood)
            {
                HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Fed);
            }
        }
    }
}
