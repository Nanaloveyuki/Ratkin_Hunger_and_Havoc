using HungerAndHavoc.Core;
using HungerAndHavoc.Trade;
using HungerAndHavoc.Api;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Visitor : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            if (!RHAH_BatchAttitude.CanOrderLeave(pawn))
            {
                return null;
            }

            MarkSeekingFood(pawn);

            Job departure = JobGiver_RHAH_Leave.TryCreate(pawn);
            if (departure != null || RHAH_Api.Get(pawn).Lifecycle == RHAH_Lifecycle.Leaving)
            {
                return departure;
            }

            Job feedChild = JobGiver_RHAH_MotherFeed.TryCreate(pawn);
            if (feedChild != null)
            {
                return feedChild;
            }

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.FeedFromRelief))
            {
                Job feed = JobGiver_RHAH_Feed.TryCreate(pawn);
                if (feed != null)
                {
                    return feed;
                }
            }

            if (RHAH_Begging.PrefersGnaw(pawn))
            {
                Job gnaw = JobGiver_RHAH_Gnaw.TryCreate(pawn, true);
                if (gnaw != null)
                {
                    return gnaw;
                }
            }

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Beg))
            {
                Job beg = JobGiver_RHAH_Beg.TryCreate(pawn);
                if (beg != null)
                {
                    return beg;
                }
            }

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Steal))
            {
                Job steal = JobGiver_RHAH_Steal.TryCreate(pawn);
                if (steal != null)
                {
                    return steal;
                }
            }



            if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.EatOutsideRelief))
            {
                Job wait = JobGiver_RHAH_WaitFood.TryCreate(pawn);
                if (wait != null)
                {
                    return wait;
                }
            }


            return null;
        }

        internal static void MarkSeekingFood(Verse.Pawn pawn)
        {
            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot != null && snapshot.Lifecycle == RHAH_Lifecycle.Arriving)
            {
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.SeekingFood);
            }
        }
    }
}
