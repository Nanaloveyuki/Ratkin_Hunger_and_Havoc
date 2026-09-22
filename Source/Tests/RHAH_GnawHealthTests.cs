using System.Collections.Generic;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_GnawHealthTests
    {
        [Fact]
        public void BarkBite_AddsPainWindowAndToxin()
        {
            GnawBite bite = RHAH_GnawHealth.ForTarget(false);

            Assert.False(bite.Wall);
            Assert.Equal(0.2f, bite.Nutrition);
            Assert.Equal(2f, bite.Damage);
            Assert.Equal(0.2f, bite.Severity);
            Assert.Equal(0.08f, bite.Toxic);
            Assert.Equal(90000, RHAH_GnawHealth.DurationTicks);
        }

        [Fact]
        public void WallBite_UsesHigherNutritionWithoutToxin()
        {
            GnawBite bite = RHAH_GnawHealth.ForTarget(true);

            Assert.True(bite.Wall);
            Assert.Equal(0.5f, bite.Nutrition);
            Assert.Equal(5f, bite.Damage);
            Assert.Equal(0.2f, bite.Severity);
            Assert.Equal(0f, bite.Toxic);
        }

        [Fact]
        public void WallCount_OvergnawsOnFifthAndForgetsOnLeave()
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();

            Assert.Equal(0, RHAH_GnawHealth.NextWallCount(counts, 0));
            Assert.Equal(1, RHAH_GnawHealth.NextWallCount(counts, 4));
            Assert.Equal(2, RHAH_GnawHealth.NextWallCount(counts, 4));
            Assert.Equal(3, RHAH_GnawHealth.NextWallCount(counts, 4));
            Assert.Equal(4, RHAH_GnawHealth.NextWallCount(counts, 4));
            Assert.False(RHAH_GnawHealth.IsOvergnaw(4));
            Assert.Equal(5, RHAH_GnawHealth.NextWallCount(counts, 4));
            Assert.True(RHAH_GnawHealth.IsOvergnaw(5));

            RHAH_GnawHealth.Forget(counts, 4);
            Assert.False(counts.ContainsKey(4));
            Assert.Equal(1, RHAH_GnawHealth.NextWallCount(counts, 4));
        }

        [Fact]
        public void ClampFood_StopsAtCurrentMaximum()
        {
            Assert.Equal(0.4f, RHAH_GnawHealth.ClampFood(1.2f, 0.4f));
            Assert.Equal(0.3f, RHAH_GnawHealth.ClampFood(0.3f, 0.4f));
            Assert.True(float.IsNaN(RHAH_GnawHealth.ClampFood(float.NaN, 0.4f)));
        }
    }
}
