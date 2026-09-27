using HungerAndHavoc.Api;
using System.Linq;
using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_IncidentCatalogTests
    {
        [Fact]
        public void DisplayIdsAreStableAndUnique()
        {
            Assert.Equal(51, RHAH_IncidentCatalog.All.Count);
            Assert.Equal(51, RHAH_IncidentCatalog.All.Select(entry => entry.DisplayId).Distinct().Count());
            Assert.Equal("I-001", RHAH_IncidentCatalog.All[0].DisplayId);
            Assert.Equal("RHAH_LargeRefugeeWave", RHAH_IncidentCatalog.All[0].DefName);
            Assert.Equal("I-051", RHAH_IncidentCatalog.All[50].DisplayId);
            Assert.Equal("RHAH_RefugeeMassacre", RHAH_IncidentCatalog.All[50].DefName);
            Assert.All(RHAH_IncidentCatalog.All, entry => Assert.StartsWith("RHAH_", entry.DefName));
            Assert.Equal(RHAH_IncidentOrigin.Original, RHAH_IncidentCatalog.GetByDisplayId("I-014").Origin);
            Assert.Equal(RHAH_IncidentOrigin.Sequel, RHAH_IncidentCatalog.GetByDisplayId("I-015").Origin);
            Assert.Equal(RHAH_IncidentCategory.Plague, RHAH_IncidentCatalog.GetByDefName("RHAH_PlagueBeggarGroup").Category);
            Assert.Equal(RHAH_IncidentTarget.Caravan, RHAH_IncidentCatalog.GetByDisplayId("I-035").Target);
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

            Assert.Equal(expected, RHAH_IncidentCatalog.All.Take(14).Select(entry => entry.DefName));
            Assert.All(RHAH_IncidentCatalog.All.Take(14), entry =>
            {
                Assert.Equal(RHAH_IncidentOrigin.Original, entry.Origin);
                Assert.Equal(RHAH_IncidentCategory.Hunger, entry.Category);
                Assert.Equal(RHAH_IncidentTarget.Map, entry.Target);
                Assert.NotEmpty(entry.LabelKey);
                Assert.True(entry.DebugPoints > 0f);
            });
        }

        [Fact]
        public void CatalogAttitude_KeepsThePreviousDefaults()
        {
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-035"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-050"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-031"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-036"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-038"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-042"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-049"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-051"));
            Assert.Equal(RHAH_Attitude.LeaningHostile, Attitude("I-034"));
            Assert.Equal(RHAH_Attitude.LeaningHostile, Attitude("I-048"));
            Assert.Equal(RHAH_Attitude.Hostile, Attitude("I-030"));
            Assert.Equal(RHAH_Attitude.Hostile, Attitude("I-043"));
            Assert.Equal(RHAH_Attitude.Hostile, Attitude("I-045"));
            Assert.Equal(RHAH_Attitude.LeaningFriendly, Attitude("I-015"));
            Assert.Equal(RHAH_Attitude.LeaningFriendly, Attitude("I-028"));
            Assert.Equal(RHAH_Attitude.Hostile, Attitude("I-006"));
            Assert.Equal(RHAH_Attitude.Neutral, Attitude("I-012"));
            Assert.Equal(RHAH_Attitude.LeaningFriendly, Attitude("I-001"));
        }

        static RHAH_Attitude Attitude(string displayId)
        {
            return RHAH_IncidentCatalog.GetByDisplayId(displayId).DefaultAttitude;
        }
    }
}
