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
    }
}
