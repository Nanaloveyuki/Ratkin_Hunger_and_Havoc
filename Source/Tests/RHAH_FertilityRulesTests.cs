using HungerAndHavoc.Generation;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_FertilityRulesTests
    {
        [Fact]
        public void LitterChancePeaksInTheMiddleAndDropsAtBothEnds()
        {
            Assert.Equal(0f, RHAH_FertilityRules.LitterChance(1, 2, 4, 6));
            Assert.Equal(1f / 3f, RHAH_FertilityRules.LitterChance(2, 2, 4, 6), 3);
            Assert.Equal(2f / 3f, RHAH_FertilityRules.LitterChance(3, 2, 4, 6), 3);
            Assert.Equal(1f, RHAH_FertilityRules.LitterChance(4, 2, 4, 6));
            Assert.Equal(2f / 3f, RHAH_FertilityRules.LitterChance(5, 2, 4, 6), 3);
            Assert.Equal(1f / 3f, RHAH_FertilityRules.LitterChance(6, 2, 4, 6), 3);
        }

        [Fact]
        public void LitterRollFollowsTheSuppliedChance()
        {
            Assert.Equal(4, RHAH_FertilityRules.RollLitter(2, 4, 6, () => 0.5f));
            Assert.Equal(2, RHAH_FertilityRules.RollLitter(2, 4, 6, () => 0f));
            Assert.Equal(6, RHAH_FertilityRules.RollLitter(2, 4, 6, () => 1f));
        }

        [Fact]
        public void LitterBoundsStayOrderedInsideTheAllowedRange()
        {
            int minimum = 0;
            int peak = 20;
            int maximum = 3;
            RHAH_FertilityRules.ClampLitter(ref minimum, ref peak, ref maximum);
            Assert.Equal(1, minimum);
            Assert.Equal(3, peak);
            Assert.Equal(3, maximum);
        }

        [Fact]
        public void FertileAgeStaysBetweenOneAndFourteen()
        {
            Assert.Equal(1f, RHAH_FertilityRules.ClampFertileAge(0f));
            Assert.Equal(14f, RHAH_FertilityRules.ClampFertileAge(40f));
            Assert.True(RHAH_FertilityRules.AgeAllows(1f, 1f));
            Assert.False(RHAH_FertilityRules.AgeAllows(0.9f, 1f));
        }

        [Fact]
        public void FertilityPercentConvertsToABoundedFactor()
        {
            Assert.Equal(1f, RHAH_FertilityRules.FertilityFactor(50f));
            Assert.Equal(3f, RHAH_FertilityRules.FertilityFactor(300f));
            Assert.Equal(10f, RHAH_FertilityRules.FertilityFactor(5000f));
        }

        [Fact]
        public void GestationStaysBetweenThreeDaysAndTheVanillaFloor()
        {
            Assert.Equal(3f, RHAH_FertilityRules.ClampGestationDays(1f));
            Assert.Equal(RHAH_FertilityRules.VanillaGestationFloorDays, RHAH_FertilityRules.ClampGestationDays(18f));
            Assert.Equal(4f, RHAH_FertilityRules.GestationDays(4f, 18f));
            Assert.Equal(5f, RHAH_FertilityRules.GestationDays(5.661f, 5f));
        }
    }
}
