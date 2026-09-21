using System.Linq;
using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerIncidentCatalogTests
    {
        [Fact]
        public void DisplayIdsAreStableAndUnique()
        {
            Assert.Equal(51, HungerIncidentCatalog.All.Count);
            Assert.Equal(51, HungerIncidentCatalog.All.Select(entry => entry.DisplayId).Distinct().Count());
            Assert.Equal("I-001", HungerIncidentCatalog.All[0].DisplayId);
            Assert.Equal("RHAH_LargeRefugeeWave", HungerIncidentCatalog.All[0].DefName);
            Assert.Equal("I-051", HungerIncidentCatalog.All[50].DisplayId);
            Assert.Equal("RHAH_RefugeeMassacre", HungerIncidentCatalog.All[50].DefName);
            Assert.All(HungerIncidentCatalog.All, entry => Assert.StartsWith("RHAH_", entry.DefName));
            Assert.Equal(HungerIncidentOrigin.Original, HungerIncidentCatalog.GetByDisplayId("I-014").Origin);
            Assert.Equal(HungerIncidentOrigin.Sequel, HungerIncidentCatalog.GetByDisplayId("I-015").Origin);
            Assert.Equal(HungerIncidentCategory.Plague, HungerIncidentCatalog.GetByDefName("RHAH_PlagueBeggarGroup").Category);
            Assert.Equal(HungerIncidentTarget.Caravan, HungerIncidentCatalog.GetByDisplayId("I-035").Target);
        }

        [Fact]
        public void OriginalEventsExposeStableDefinitionsAndRoles()
        {
            string[] expected =
            {
                "RHAH_LargeRefugeeWave", "RHAH_AbandonedRatkinChildren", "RHAH_ShatteredMother",
                "RHAH_BeggarFamily", "RHAH_BeggarGroup", "RHAH_ThiefRatkinGroup",
                "RHAH_ThiefRatkinChildGroup", "RHAH_WildRatkinWandersIn", "RHAH_WildRatkinChildWandersIn",
                "RHAH_WildRatkinGroupWandersIn", "RHAH_FamineRefugees", "RHAH_RatkinTraderCaravan",
                "RHAH_ChildExchange", "RHAH_BeggarSiege"
            };

            Assert.Equal(expected, HungerIncidentCatalog.All.Take(14).Select(entry => entry.DefName));
            Assert.All(HungerIncidentCatalog.All.Take(14), entry =>
            {
                Assert.Equal(HungerIncidentOrigin.Original, entry.Origin);
                Assert.Equal(HungerIncidentCategory.Hunger, entry.Category);
                Assert.Equal(HungerIncidentTarget.Map, entry.Target);
                Assert.NotEmpty(entry.LabelKey);
                Assert.True(entry.DebugPoints > 0f);
            });
        }
    }
}
