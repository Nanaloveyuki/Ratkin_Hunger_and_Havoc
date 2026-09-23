using System.Collections.Generic;
using HungerAndHavoc.Data;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerContentSelectorTests
    {
        [Fact]
        public void CatalogKeepsEveryOwnedHistoryAndTrait()
        {
            Assert.Equal(121, HungerContentCatalog.Histories.Count);
            Assert.Equal(50, HungerContentCatalog.Traits.Count);
            Assert.NotNull(HungerContentCatalog.FindHistory("A001"));
            Assert.NotNull(HungerContentCatalog.FindHistory("Y047"));
            Assert.Equal("RHAH_Trait_HardyLabor", HungerContentCatalog.FindTrait("T01").TraitDefName);
            Assert.True(HungerContentCatalog.IsOwnedHistory("RHAH_History_Newborn"));
            Assert.False(HungerContentCatalog.IsOwnedTrait("Industriousness"));
        }

        [Fact]
        public void ChildCannotDrawAnAdulthood()
        {
            HungerContentQuery query = Query(6f, false, null, null);
            for (int i = 0; i < 30; i++)
            {
                HungerHistoryRecord selected = HungerContentSelector.SelectHistory(query, () => i / 30f);
                Assert.NotNull(selected);
                Assert.Equal(HungerHistorySlot.Childhood, selected.Slot);
                Assert.True(selected.MaximumAge == null || selected.MaximumAge > 6f);
            }
        }
        [Fact]
        public void AdultStageBelowEighteenDrawsNoAdultHistory()
        {
            HungerContentQuery query = Query(16f, true, 20f, null);
            Assert.Null(HungerContentSelector.SelectHistory(query, () => 0.2f));
        }


        [Fact]
        public void DisabledHistoryStaysOutOfTheDraw()
        {
            HungerContentQuery query = Query(30f, true, 20f, "I-001", id => id != "A001");
            for (int i = 0; i < 20; i++)
            {
                HungerHistoryRecord selected = HungerContentSelector.SelectHistory(query, () => i / 20f);
                Assert.NotEqual("A001", selected?.DisplayId);
            }
        }

        [Fact]
        public void ColdAdultMissesAWarmOnlyHistory()
        {
            HungerHistoryRecord warm = HungerContentCatalog.FindHistory("A001");
            HungerContentQuery query = Query(30f, true, warm.MinimumTemperature - 5f, null);
            for (int i = 0; i < 20; i++)
            {
                Assert.NotEqual("A001", HungerContentSelector.SelectHistory(query, () => i / 20f)?.DisplayId);
            }
        }

        [Fact]
        public void TraitPrefersTheMatchingHistory()
        {
            HungerContentQuery query = Query(30f, true, 20f, "I-001");
            HungerTraitRecord selected = HungerContentSelector.SelectTrait(
                query,
                "A001",
                () => 0f,
                record => true);
            Assert.NotNull(selected);
            Assert.Contains("A001", selected.HistoryDisplayIds);
        }

        [Fact]
        public void ZeroTraitWeightDrawsNothing()
        {
            HungerContentQuery query = Query(30f, true, 20f, "I-001", _ => true, _ => 0f);
            Assert.Null(HungerContentSelector.SelectTrait(query, "A001", () => 0f, record => true));
        }

        [Fact]
        public void PlagueCategoryUsesTheNewIncidentIds()
        {
            Assert.True(HungerContentSelector.CategoryMatches(HungerContentCategory.Plague, "I-036"));
            Assert.True(HungerContentSelector.CategoryMatches(HungerContentCategory.Plague, "I-050"));
            Assert.False(HungerContentSelector.CategoryMatches(HungerContentCategory.Plague, "I-051"));
            Assert.True(HungerContentSelector.CategoryMatches(HungerContentCategory.Theft, "I-043"));
            Assert.True(HungerContentSelector.CategoryMatches(HungerContentCategory.Siege, "I-045"));
        }

        static HungerContentQuery Query(
            float age,
            bool adult,
            float? temperature,
            string incident,
            System.Func<string, bool> historyEnabled = null,
            System.Func<string, float> traitWeight = null)
        {
            return new HungerContentQuery(
                age,
                adult,
                true,
                temperature,
                incident,
                true,
                true,
                historyEnabled ?? (_ => true),
                _ => true,
                traitWeight ?? (id => HungerContentCatalog.DefaultTraitWeight(id)));
        }
    }
}
