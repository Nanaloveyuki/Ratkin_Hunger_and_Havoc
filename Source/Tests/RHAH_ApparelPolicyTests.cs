using System.Collections.Generic;
using HungerAndHavoc.Generation;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_ApparelPolicyTests
    {
        const int Medieval = 3;
        const int Industrial = 4;

        [Fact]
        public void RefugeeClothesStayInsideRatkinAndVanillaMedievalCloth()
        {
            Assert.True(Allows("RK_ApronSkirt", "Solaris.RatkinRaceMod", false, Medieval, false));
            Assert.True(Allows("Apparel_TribalA", "Ludeon.RimWorld", true, 2, false));
            Assert.True(Allows("Apparel_BasicShirt", "Ludeon.RimWorld.Ideology", true, Medieval, false));
            Assert.True(Allows("Apparel_KidTribal", "Some.OtherMod", false, Medieval, false));
            Assert.True(Allows("Foreign_Sack", "Some.OtherMod", false, Medieval, true));

            Assert.False(Allows("Apparel_PowerArmor", "Ludeon.RimWorld", true, Industrial, false));
            Assert.False(Allows("RK_Plate", "Solaris.RatkinRaceMod", false, Industrial, false));
            Assert.False(Allows("Foreign_Dress", "Some.OtherMod", false, Medieval, false));
            Assert.False(Allows("Apparel_ShieldBelt", "Ludeon.RimWorld", true, Industrial, false, false));
            Assert.False(Allows(null, "Ludeon.RimWorld", true, 2, false));
        }

        [Fact]
        public void YoungClothesStayInTheirAgePool()
        {
            Assert.True(RHAH_ApparelPolicy.FitsStage("Apparel_BabyOnesie", 0));
            Assert.False(RHAH_ApparelPolicy.FitsStage("Apparel_KidTribal", 0));
            Assert.True(RHAH_ApparelPolicy.FitsStage("Apparel_KidTribal", 1));
            Assert.False(RHAH_ApparelPolicy.FitsStage("Apparel_BabyOnesie", 1));
            Assert.False(RHAH_ApparelPolicy.FitsStage("Apparel_KidTribal", 2));
            Assert.True(RHAH_ApparelPolicy.FitsStage("Apparel_TribalA", 2));
        }

        [Fact]
        public void QualityStaysAtNormalOrWorseAndClothIsWornOut()
        {
            Assert.Equal(0, RHAH_ApparelPolicy.Quality(0f));
            Assert.Equal(0, RHAH_ApparelPolicy.Quality(0.349f));
            Assert.Equal(1, RHAH_ApparelPolicy.Quality(0.35f));
            Assert.Equal(1, RHAH_ApparelPolicy.Quality(0.849f));
            Assert.Equal(2, RHAH_ApparelPolicy.Quality(0.85f));
            Assert.Equal(2, RHAH_ApparelPolicy.Quality(1f));
            Assert.Equal(10, RHAH_ApparelPolicy.HitPoints(100, 0.10f));
            Assert.Equal(10, RHAH_ApparelPolicy.HitPoints(100, 0f));
            Assert.Equal(1, RHAH_ApparelPolicy.PieceCount(2, 0f));
            Assert.Equal(4, RHAH_ApparelPolicy.PieceCount(2, 1f));
        }

        [Fact]
        public void StuffPrefersClothThenHumanLeather()
        {
            List<string> both = new List<string> { "Cloth", "Humanleather", "Steel" };
            Assert.Equal("Cloth", RHAH_ApparelPolicy.PreferredStuff(both, 0f));
            Assert.Equal("Humanleather", RHAH_ApparelPolicy.PreferredStuff(both, 0.65f));
            Assert.Equal("Cloth", RHAH_ApparelPolicy.PreferredStuff(new List<string> { "Cloth", "Steel" }, 1f));
            Assert.Null(RHAH_ApparelPolicy.PreferredStuff(new List<string> { "Steel" }, 0f));
        }

        [Fact]
        public void DisabledClothingStaysOutOfThePool()
        {
            Assert.False(Allows("Apparel_TribalA", "Ludeon.RimWorld", true, 2, false, true, false));
            Assert.True(Allows("Apparel_TribalA", "Ludeon.RimWorld", true, 2, false));
        }

        static bool Allows(string defName, string packageId, bool official, int techLevel, bool optIn, bool clothing = true, bool enabled = true)
        {
            return RHAH_ApparelPolicy.AllowsApparel(defName, true, true, clothing, packageId, official, optIn, techLevel, enabled);
        }
    }
}
