using HungerAndHavoc.Core;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_ReliefFoodTests
    {
        [Fact]
        public void EmptyDenylistAllowsFoodAndDisableAllKeepsLaterFoods()
        {
            HungerAndHavocSettings settings = new HungerAndHavocSettings();
            Assert.True(settings.IsReliefFoodEnabled("MealSimple"));
            settings.SetAllReliefFood(false, new System.Collections.Generic.List<string> { "MealSimple" });
            Assert.False(settings.IsReliefFoodEnabled("MealSimple"));
            Assert.True(settings.IsReliefFoodEnabled("MealFine"));
            settings.SetAllReliefFood(true, null);
            Assert.True(settings.IsReliefFoodEnabled("MealSimple"));
        }

        [Fact]
        public void SatietyAndRefeedThresholdsStayAtThePlannedValues()
        {
            Assert.Equal(0.82f, RHAH_ReliefFood.SatisfiedLevel);
            Assert.Equal(0.4f, RHAH_ReliefFood.RefeedMalnutrition);
            Assert.True(0.81f < RHAH_ReliefFood.SatisfiedLevel);
            Assert.True(0.39f < RHAH_ReliefFood.RefeedMalnutrition);
            Assert.False(0.39f >= RHAH_ReliefFood.RefeedMalnutrition);
        }

        [Fact]
        public void MissingFoodIsRejectedBeforeAJobCanBeBuilt()
        {
            Assert.Equal(RHAH_FoodReject.Missing, RHAH_ReliefFood.Reject(null, null, true));
            Assert.Null(RHAH_ReliefFood.MakeJob(null, null));
            Assert.Null(JobGiver_RHAH_Feed.TryCreate(null));
            Assert.Null(JobGiver_RHAH_WaitFood.TryCreate(null));
        }
    }
}
