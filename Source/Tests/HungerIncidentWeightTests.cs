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
