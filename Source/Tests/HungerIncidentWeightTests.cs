using HungerAndHavoc.Core;
using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerIncidentWeightTests
    {
        [Fact]
        public void FamilyWeightsStayOrdered()
        {
            Assert.True(HungerIncidentWeight.FamilyBase(HungerIncidentFamily.Wild) >
                HungerIncidentWeight.FamilyBase(HungerIncidentFamily.Beggar));
            Assert.Equal(0.12f, HungerIncidentWeight.FamilyBase(HungerIncidentFamily.Intel));
            Assert.Equal(0.25f, HungerIncidentWeight.FamilyBase(HungerIncidentFamily.Aid));
            Assert.Equal(0.2f, HungerIncidentWeight.FamilyBase(HungerIncidentFamily.Special));
        }

        [Fact]
        public void PlagueAndWinterStackOnNegativePool()
        {
            float weight = HungerIncidentWeight.Evaluate(Input(
                HungerIncidentFamily.Wild,
                HungerIncidentCategory.Plague,
                HungerAttitudePool.Negative,
                HungerIncidentSeason.Winter,
                trust: -100));

            Assert.Equal(1.4f * 1.1f * 0.5f * 1.25f, weight);
        }

        [Fact]
        public void PositivePoolIgnoresTrust()
        {
            float low = HungerIncidentWeight.Evaluate(Input(
                HungerIncidentFamily.Aid,
                HungerIncidentCategory.Hunger,
                HungerAttitudePool.Positive,
                HungerIncidentSeason.Summer,
                trust: -100));
            float high = HungerIncidentWeight.Evaluate(Input(
                HungerIncidentFamily.Aid,
                HungerIncidentCategory.Hunger,
                HungerAttitudePool.Positive,
                HungerIncidentSeason.Summer,
                trust: 100));

            Assert.Equal(0.25f, low);
            Assert.Equal(low, high);
        }

        [Fact]
        public void TrustClampsAtTheNarrativeLimits()
        {
            Assert.Equal(1.25f, HungerIncidentWeight.TrustFactor(-400));
            Assert.Equal(0.75f, HungerIncidentWeight.TrustFactor(400));
            Assert.Equal(1f, HungerIncidentWeight.TrustFactor(0));
        }

        [Fact]
        public void TargetMismatchZeroesTheWeight()
        {
            HungerIncidentWeightInput caravanOnMap = new HungerIncidentWeightInput(
                HungerIncidentFamily.Thief,
                HungerIncidentCategory.Hunger,
                HungerIncidentTarget.Caravan,
                HungerAttitudePool.Negative,
                HungerIncidentSeason.Spring,
                0,
                mapHome: true,
                playerCaravan: false);
            HungerIncidentWeightInput mapOnCaravan = new HungerIncidentWeightInput(
                HungerIncidentFamily.Beggar,
                HungerIncidentCategory.Hunger,
                HungerIncidentTarget.Map,
                HungerAttitudePool.Negative,
                HungerIncidentSeason.Spring,
                0,
                mapHome: false,
                playerCaravan: true);

            Assert.Equal(0f, HungerIncidentWeight.Evaluate(caravanOnMap));
            Assert.Equal(0f, HungerIncidentWeight.Evaluate(mapOnCaravan));
        }

        [Fact]
        public void SelectionUsesOnlyPositiveWeights()
        {
            float[] weights = { 0f, 1.4f, 0f, 0.7f };

            Assert.Equal(1, HungerIncidentWeight.Select(weights, 0f));
            Assert.Equal(1, HungerIncidentWeight.Select(weights, 1.399f));
            Assert.Equal(3, HungerIncidentWeight.Select(weights, 1.4f));
            Assert.Equal(-1, HungerIncidentWeight.Select(weights, 2.1f));
            Assert.Equal(-1, HungerIncidentWeight.Select(new[] { 0f, 0f }, 0f));
        }
        [Fact]
        public void DisabledIncidentDropsOutOfTheDraw()
        {
            Assert.Equal(0f, HungerIncidentTuning.Scale(1.4f, false, 100f));
            Assert.Equal(0f, HungerIncidentTuning.Scale(1.4f, true, 0f));
        }

        [Fact]
        public void PlayerWeightScalesTheCatalogWeight()
        {
            Assert.Equal(0.7f, HungerIncidentTuning.Scale(1.4f, true, 50f));
            Assert.Equal(1.4f, HungerIncidentTuning.Scale(1.4f, true, 100f));
            Assert.Equal(0f, HungerIncidentTuning.Scale(0f, true, 100f));
        }

        [Fact]
        public void DebugPointsAndWeightsClampToTheirRanges()
        {
            HungerAndHavocSettings settings = new HungerAndHavocSettings();
            Assert.Equal(700f, settings.IncidentDebugPoints("I-001", 700f));
            Assert.Equal(100f, settings.IncidentWeight("I-001"));

            settings.SetIncidentDebugPoints("I-001", 20000f);
            settings.SetIncidentWeight("I-001", -4f);
            Assert.Equal(10000f, settings.IncidentDebugPoints("I-001", 700f));
            Assert.Equal(0f, settings.IncidentWeight("I-001"));

            settings.SetIncidentEnabled("I-001", false);
            Assert.False(settings.IsIncidentEnabled("I-001"));
            settings.SetIncidentEnabled("I-001", true);
            Assert.True(settings.IsIncidentEnabled("I-001"));
        }

        [Fact]
        public void ExactFieldKeepsAnEquivalentTypedBuffer()
        {
            Assert.True(HungerAndHavoc.Pawn.Compat.RHAH_IrisMenusWidgets.SameNumber("50", "50.0"));
            Assert.False(HungerAndHavoc.Pawn.Compat.RHAH_IrisMenusWidgets.SameNumber("50.5", "50"));
            Assert.False(HungerAndHavoc.Pawn.Compat.RHAH_IrisMenusWidgets.SameNumber("soon", "50"));
        }
        [Fact]
        public void CatalogPointsKeepTheCurrentGroupSize()
        {
            Assert.Equal(24, HungerIncidentScale.Count("I-001", 700f));
            Assert.Equal(4, HungerIncidentScale.Count("I-002", 400f));
            Assert.Equal(8, HungerIncidentScale.Count("I-010", 700f));
            Assert.Equal(3, HungerIncidentScale.Count("I-011", 350f));
            Assert.Equal(1, HungerIncidentScale.Count("I-008", 10000f));
            Assert.Equal(10, HungerIncidentScale.Count("I-001", 1f));
            Assert.Equal(50, HungerIncidentScale.Count("I-001", 10000f));
        }

        [Fact]
        public void RequestAmountScalesAroundTheCatalogPoints()
        {
            Assert.Equal(12, HungerIncidentScale.ScaleAmount(12, 300f, 6, 28));
            Assert.Equal(6, HungerIncidentScale.ScaleAmount(12, 1f, 6, 28));
            Assert.Equal(28, HungerIncidentScale.ScaleAmount(12, 10000f, 6, 28));
            Assert.Equal(1, HungerRequestRules.Amount(HungerRequestKind.Baby, 0f, 0, 10000f));
        }

        static HungerIncidentWeightInput Input(
            HungerIncidentFamily family,
            HungerIncidentCategory category,
            HungerAttitudePool pool,
            HungerIncidentSeason season,
            int trust)
        {
            return new HungerIncidentWeightInput(
                family,
                category,
                HungerIncidentTarget.Map,
                pool,
                season,
                trust,
                mapHome: true,
                playerCaravan: false);
        }
    }
}
