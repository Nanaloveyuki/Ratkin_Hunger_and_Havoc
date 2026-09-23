using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_VisitorRulesTests
    {
        [Fact]
        public void CapKeepsTheEventMinimumAndAFixedMotherGroup()
        {
            Assert.Equal(50, RHAH_VisitorRules.LimitCount(50, 10, 8, false));
            Assert.Equal(4, RHAH_VisitorRules.LimitCount(4, 2, 1, true));
            Assert.Equal(30, RHAH_IncidentScale.Count("I-001", 10000f, 30, false));
            Assert.True(RHAH_IncidentScale.Count("I-004", 10000f, 1, false) >= 3);
        }

        [Fact]
        public void OrdinaryAgeUsesTheRangeAndFixedRolesKeepTheirs()
        {
            Assert.Equal(12f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.Beggar, null, 10f, 20f, 0.2f));
            Assert.Equal(1.5f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.RatkinYoung, 1.5f, 20f, 40f, 1f));
            Assert.Equal(28f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.Mother, 28f, 0f, 10f, 0f));
            Assert.Null(RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.BeggarMother, null, 0f, 50f, 0f));
        }

        [Fact]
        public void FirstFedStayLandsBetweenHalfAndOneAndAHalf()
        {
            int half = RHAH_VisitorRules.FedStayTicks(0.5f, 0f);
            int full = RHAH_VisitorRules.FedStayTicks(0.5f, 1f);
            Assert.Equal(15000, half);
            Assert.Equal(45000, full);
            RHAH_PawnState state = new RHAH_PawnState();
            state.TrySetLifecycle(RHAH_Lifecycle.SeekingFood);
            state.TrySetLifecycle(RHAH_Lifecycle.Fed);
            Assert.True(state.hasBeenFed);
            state.ClearFedTimer();
            Assert.True(state.hasBeenFed);
            Assert.Equal(-1, state.leaveAfterGameTick);
        }

        [Fact]
        public void MissingFoodWaitsThenResetsAfterFoodIsFound()
        {
            Assert.Equal(30000, RHAH_VisitorRules.WaitTicks(true, 0.5f));
            Assert.Equal(0, RHAH_VisitorRules.WaitTicks(false, 0.5f));
            Assert.Equal(40000, RHAH_VisitorRules.NextWaitTick(10000, 20000, true, true, 0.5f));
            Assert.Equal(20000, RHAH_VisitorRules.NextWaitTick(10000, 20000, false, true, 0.5f));
        }

        [Fact]
        public void HireExpiresAndADownedWorkerKeepsTheRemainingTime()
        {
            int deadline = RHAH_VisitorRules.BeginStay(0, RHAH_StayKind.Hire, 5, 60);
            Assert.Equal(60 * RHAH_VisitorRules.TicksPerDay, deadline);
            Assert.False(RHAH_VisitorRules.StayExpired(deadline, deadline, true));
            int resumed = RHAH_VisitorRules.ResumeStay(deadline + 1000, deadline, 5000, false);
            Assert.Equal(deadline + 1000 + 5000, resumed);
            Assert.True(RHAH_VisitorRules.StayExpired(resumed, resumed, false));
            Assert.True(RHAH_VisitorRules.ClearsTrade(RHAH_ReleaseReason.Recruited, false));
            Assert.True(RHAH_VisitorRules.AcceptsStone(true, false));
            Assert.False(RHAH_VisitorRules.AcceptsStone(true, true));
            Assert.True(RHAH_VisitorRules.LiftsAgeImmobility(false, true, true, false));
            Assert.False(RHAH_VisitorRules.LiftsAgeImmobility(false, true, true, true));
        }
    }
}
