using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_IncidentScheduleTests
    {
        [Fact]
        public void DaysClampToTheFrequencyRange()
        {
            Assert.Equal(15f, RHAH_IncidentSchedule.ClampDays(float.NaN));
            Assert.Equal(15f, RHAH_IncidentSchedule.ClampDays(-1f));
            Assert.Equal(0f, RHAH_IncidentSchedule.ClampDays(0f));
            Assert.Equal(60f, RHAH_IncidentSchedule.ClampDays(90f));
        }

        [Fact]
        public void OccurrenceChanceIsThePoolCheckNotAnIncidentShare()
        {
            Assert.Equal(0f, RHAH_IncidentSchedule.OccurrenceChance(0f));
            Assert.Equal(1000f / (15f * 60000f), RHAH_IncidentSchedule.OccurrenceChance(15f), 6);
            Assert.True(RHAH_IncidentSchedule.OccurrenceChance(15f) > RHAH_IncidentSchedule.OccurrenceChance(60f));
            Assert.Equal(1000f / (60f * 60000f), RHAH_IncidentSchedule.OccurrenceChance(90f), 6);
        }

        [Fact]
        public void MapPoolCannotSelectCaravanIncidents()
        {
            string selected = RHAH_IncidentSchedule.Select(
                RHAH_AttitudePool.Negative,
                trust: 0,
                RHAH_IncidentSeason.Summer,
                mapHome: true,
                playerCaravan: false,
                roll: 0f);

            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(selected);
            Assert.NotNull(entry);
            Assert.Equal(RHAH_IncidentTarget.Map, entry.Target);
            Assert.Equal(RHAH_AttitudePool.Negative, entry.DefaultAttitudePool);
        }

        [Fact]
        public void CaravanPoolSelectsOnlyCaravanIncidents()
        {
            float total = RHAH_IncidentSchedule.TotalWeight(
                RHAH_AttitudePool.Negative,
                0,
                RHAH_IncidentSeason.Summer,
                mapHome: false,
                playerCaravan: true);
            string selected = RHAH_IncidentSchedule.Select(
                RHAH_AttitudePool.Negative,
                0,
                RHAH_IncidentSeason.Summer,
                mapHome: false,
                playerCaravan: true,
                total - 0.001f);

            Assert.Contains(selected, new[] { "I-035", "I-050" });
        }

        [Fact]
        public void PositivePoolStaysOnAidIntelAndPositiveOriginals()
        {
            string selected = RHAH_IncidentSchedule.Select(
                RHAH_AttitudePool.Positive,
                trust: -100,
                RHAH_IncidentSeason.Winter,
                mapHome: true,
                playerCaravan: false,
                roll: 0f);

            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(selected);
            Assert.Equal(RHAH_AttitudePool.Positive, entry.DefaultAttitudePool);
            Assert.NotEqual(RHAH_IncidentCategory.Plague, entry.Category);
        }

        [Fact]
        public void HighestTrustStillLeavesNegativeWeight()
        {
            float hostile = RHAH_IncidentSchedule.TotalWeight(
                RHAH_AttitudePool.Negative,
                trust: 100,
                RHAH_IncidentSeason.Fall,
                mapHome: true,
                playerCaravan: false);
            float friendly = RHAH_IncidentSchedule.TotalWeight(
                RHAH_AttitudePool.Negative,
                trust: -100,
                RHAH_IncidentSeason.Fall,
                mapHome: true,
                playerCaravan: false);

            Assert.True(hostile > 0f);
            Assert.True(friendly > hostile);
        }
    }
}
