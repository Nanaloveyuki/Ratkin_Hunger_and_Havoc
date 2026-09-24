using System.IO;
using System.Runtime.CompilerServices;
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
            Assert.Equal(12f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.Beggar, null, 10f, 20f, 0.2f, false));
            Assert.Equal(1.5f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.RatkinYoung, 1.5f, 20f, 40f, 1f, false));
            Assert.Equal(28f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.Mother, 28f, 0f, 10f, 0f, true));
            Assert.Null(RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.BeggarMother, null, 0f, 50f, 0f, true));
            Assert.Equal(24f, RHAH_VisitorRules.GenerationAge(RHAH_PawnRole.RatkinYoung, null, 20f, 40f, 0.2f, true));
        }

        [Fact]
        public void GoodwillStaysHostileOrNeutralAndYoungNeedThreeWithoutToddlers()
        {
            Assert.Equal(-100, RHAH_VisitorRules.LockedGoodwill(RHAH_Attitude.Hostile));
            Assert.Equal(0, RHAH_VisitorRules.LockedGoodwill(RHAH_Attitude.Friendly));
            Assert.Equal(0, RHAH_VisitorRules.LockedGoodwill(RHAH_Attitude.Neutral));
            Assert.Equal(3f, RHAH_VisitorRules.WalkingAgeFloor(0.1f, false, false));
            Assert.Equal(8f, RHAH_VisitorRules.WalkingAgeFloor(8f, false, false));
            Assert.Equal(0.1f, RHAH_VisitorRules.WalkingAgeFloor(0.1f, true, false));
            Assert.Equal(0.1f, RHAH_VisitorRules.WalkingAgeFloor(0.1f, false, true));
            Assert.True(RHAH_VisitorRules.ClearsApparel(3, false));
            Assert.False(RHAH_VisitorRules.ClearsApparel(3, true));
            Assert.False(RHAH_VisitorRules.AddsColdClothes(3, true, false, -20f, 10f));
            Assert.True(RHAH_VisitorRules.AddsColdClothes(0, true, false, -20f, 10f));
            Assert.False(RHAH_VisitorRules.AddsColdClothes(1, false, false, -20f, 10f));
            Assert.True(RHAH_VisitorRules.AddsColdClothes(2, false, false, -20f, 10f));
            Assert.Equal(0, RHAH_VisitorRules.ClampOwnedTraits(-1));
            Assert.Equal(3, RHAH_VisitorRules.ClampOwnedTraits(9));
        }

        [Fact]
        public void GenderModeOverridesARandomRollButNotARequestedGender()
        {
            Assert.Equal(2, RHAH_VisitorRules.ResolveGender(null, 2, 0, 0f));
            Assert.Equal(1, RHAH_VisitorRules.ResolveGender(null, 1, 0, 0f));
            Assert.Equal(1, RHAH_VisitorRules.ResolveGender(1, 3, 0, 1f));
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
        public void BegTargetMustBeReachableAndAvailable()
        {
            Assert.True(RHAH_VisitorRules.CanSelectBegTarget(false, false, false, true, true));
            Assert.False(RHAH_VisitorRules.CanSelectBegTarget(false, false, false, false, true));
            Assert.False(RHAH_VisitorRules.CanSelectBegTarget(false, false, false, true, false));
            Assert.False(RHAH_VisitorRules.CanSelectBegTarget(true, false, false, true, true));
            Assert.False(RHAH_VisitorRules.CanSelectBegTarget(false, true, false, true, true));
            Assert.False(RHAH_VisitorRules.CanSelectBegTarget(false, false, true, true, true));
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

        [Fact]
        public void AttitudeFactionsUseRealFieldsAndDoNotDropRaidLoot()
        {
            string xml = File.ReadAllText(FactionPath());
            Assert.DoesNotContain("naturalColonyGoodwill", xml);
            Assert.Contains("<naturalEnemy>true</naturalEnemy>", xml);
            Assert.Contains("<raidLootValueFromPointsCurve>", xml);
            Assert.Equal(1, Count(xml, "<raidLootValueFromPointsCurve>"));
        }

        static int Count(string text, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        static string FactionPath([CallerFilePath] string testFile = null)
        {
            return Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(testFile), "..", "..", "1.6", "Defs", "FactionDefs", "RHAH_Factions.xml"));
        }
    }
}
