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
            if (!RHAH_Api.IsVisitor(pawn))
            {
                return null;
            }

            MarkSeekingFood(pawn);
            if (RHAH_CaravanStay.ShouldHold(
                pawn,
                RHAH_Mod.Settings == null || RHAH_Mod.Settings.traderIgnoresHarshEnvironment,
                RHAH_Mod.Settings == null || RHAH_Mod.Settings.traderIgnoresEnclosedSpace))
            {
                return null;
            }

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.FeedFromRelief))
            {
                Job feed = JobGiver_RHAH_Feed.TryCreate(pawn);
                if (feed != null)
                {
                    return feed;
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

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Gnaw))
            {
                Job gnaw = JobGiver_RHAH_Gnaw.TryCreate(pawn);
                if (gnaw != null)
                {
                    return gnaw;
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

            if (RHAH_Api.Allows(pawn, RHAH_BehaviorGate.LeaveAfterFed) ||
                RHAH_Api.Allows(pawn, RHAH_BehaviorGate.ExitMap))
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
            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot != null && snapshot.Lifecycle == RHAH_Lifecycle.Arriving)
            {
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.SeekingFood);
            }
        }

        internal static void MarkFed(Verse.Pawn pawn)
        {
            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot != null && snapshot.Lifecycle == RHAH_Lifecycle.SeekingFood)
            {
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Fed);
            }
        }
    }
}
