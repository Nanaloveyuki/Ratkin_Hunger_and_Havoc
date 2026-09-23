using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerIncidentScheduleTests
    {
        [Fact]
        public void DaysClampToTheFrequencyRange()
        {
            Assert.Equal(15f, HungerIncidentSchedule.ClampDays(float.NaN));
            Assert.Equal(15f, HungerIncidentSchedule.ClampDays(-1f));
            Assert.Equal(0f, HungerIncidentSchedule.ClampDays(0f));
            Assert.Equal(60f, HungerIncidentSchedule.ClampDays(90f));
        }

        [Fact]
        public void OccurrenceChanceIsThePoolCheckNotAnIncidentShare()
        {
            Assert.Equal(0f, HungerIncidentSchedule.OccurrenceChance(0f));
            Assert.Equal(1000f / (15f * 60000f), HungerIncidentSchedule.OccurrenceChance(15f), 6);
            Assert.True(HungerIncidentSchedule.OccurrenceChance(15f) > HungerIncidentSchedule.OccurrenceChance(60f));
            Assert.Equal(1000f / (60f * 60000f), HungerIncidentSchedule.OccurrenceChance(90f), 6);
        }

        [Fact]
        public void MapPoolCannotSelectCaravanIncidents()
        {
            string selected = HungerIncidentSchedule.Select(
                HungerAttitudePool.Negative,
                trust: 0,
                HungerIncidentSeason.Summer,
                mapHome: true,
                playerCaravan: false,
                roll: 0f);

            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(selected);
            Assert.NotNull(entry);
            Assert.Equal(HungerIncidentTarget.Map, entry.Target);
            Assert.Equal(HungerAttitudePool.Negative, entry.DefaultAttitudePool);
        }

        [Fact]
        public void CaravanPoolSelectsOnlyCaravanIncidents()
        {
            float total = HungerIncidentSchedule.TotalWeight(
                HungerAttitudePool.Negative,
                0,
                HungerIncidentSeason.Summer,
                mapHome: false,
                playerCaravan: true);
            string selected = HungerIncidentSchedule.Select(
                HungerAttitudePool.Negative,
                0,
                HungerIncidentSeason.Summer,
                mapHome: false,
                playerCaravan: true,
                total - 0.001f);

            Assert.Contains(selected, new[] { "I-035", "I-050" });
        }

        [Fact]
        public void PositivePoolStaysOnAidIntelAndPositiveOriginals()
        {
            string selected = HungerIncidentSchedule.Select(
                HungerAttitudePool.Positive,
                trust: -100,
                HungerIncidentSeason.Winter,
                mapHome: true,
                playerCaravan: false,
                roll: 0f);

            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(selected);
            Assert.Equal(HungerAttitudePool.Positive, entry.DefaultAttitudePool);
            Assert.NotEqual(HungerIncidentCategory.Plague, entry.Category);
        }

        [Fact]
        public void HighestTrustStillLeavesNegativeWeight()
        {
            float hostile = HungerIncidentSchedule.TotalWeight(
                HungerAttitudePool.Negative,
                trust: 100,
                HungerIncidentSeason.Fall,
                mapHome: true,
                playerCaravan: false);
            float friendly = HungerIncidentSchedule.TotalWeight(
                HungerAttitudePool.Negative,
                trust: -100,
                HungerIncidentSeason.Fall,
                mapHome: true,
                playerCaravan: false);

            Assert.True(hostile > 0f);
            Assert.True(friendly > hostile);
        }
    }
}
