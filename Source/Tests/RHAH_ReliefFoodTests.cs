using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using HungerAndHavoc.Core;
using HungerAndHavoc.Pawn;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_ReliefFoodTests
    {
        [Fact]
        public void EmptyDenylistAllowsFoodAndDisableAllKeepsLaterFoods()
        {
            RHAH_Settings settings = new RHAH_Settings();
            Assert.True(settings.IsReliefFoodEnabled("MealSimple"));
            settings.SetAllReliefFood(false, new System.Collections.Generic.List<string> { "MealSimple" });
            Assert.False(settings.IsReliefFoodEnabled("MealSimple"));
            Assert.True(settings.IsReliefFoodEnabled("MealFine"));
            settings.SetAllReliefFood(true, null);
            Assert.True(settings.IsReliefFoodEnabled("MealSimple"));
        }

        [Fact]
        public void SatietyAndRefeedThresholdsStayAtThePlannedValues()
        {
            Assert.Equal(0.82f, RHAH_ReliefFood.SatisfiedLevel);
            Assert.Equal(0.4f, RHAH_ReliefFood.RefeedMalnutrition);
            Assert.True(0.81f < RHAH_ReliefFood.SatisfiedLevel);
            Assert.True(0.39f < RHAH_ReliefFood.RefeedMalnutrition);
            Assert.False(0.39f >= RHAH_ReliefFood.RefeedMalnutrition);
        }

        [Fact]
        public void MissingFoodIsRejectedBeforeAJobCanBeBuilt()
        {
            Assert.Equal(RHAH_FoodReject.Missing, RHAH_ReliefFood.Reject(null, null, true));
            Assert.Null(RHAH_ReliefFood.MakeJob(null, null));
            Assert.Null(JobGiver_RHAH_Feed.TryCreate(null));
            Assert.Null(JobGiver_RHAH_WaitFood.TryCreate(null));
        }

        [Fact]
        public void FoodsGroupBySourceModAndKeepOrder()
        {
            ThingDef meal = Food("MealSimple", "Core");
            ThingDef fine = Food("MealFine", "Core");
            ThingDef paste = Food("MealNutrientPaste", "Ratkin: Hunger and Havoc");
            ThingDef loose = Food("LooseBerry", null);
            List<List<ThingDef>> groups = RHAH_ReliefFood.GroupBySourceMod(new List<ThingDef>
            {
                meal,
                paste,
                fine,
                loose
            });
            Assert.Equal(3, groups.Count);
            Assert.Equal(new[] { meal, fine }, groups[0]);
            Assert.Equal(new[] { paste }, groups[1]);
            Assert.Equal(new[] { loose }, groups[2]);
            Assert.Equal("Core", RHAH_ReliefFood.SourceModName(meal));
            Assert.Null(RHAH_ReliefFood.SourceModName(loose));
            Assert.Empty(RHAH_ReliefFood.GroupBySourceMod(null));
        }

        [Fact]
        public void FoodQueryMatchesLabelDefNameAndMod()
        {
            ThingDef meal = Food("MealSimple", "Core");
            meal.label = "simple meal";
            Assert.True(RHAH_ReliefFood.MatchesQuery(meal, null));
            Assert.True(RHAH_ReliefFood.MatchesQuery(meal, "simple core"));
            Assert.True(RHAH_ReliefFood.MatchesQuery(meal, "mealsimple"));
            Assert.False(RHAH_ReliefFood.MatchesQuery(meal, "simple paste"));
            Assert.False(RHAH_ReliefFood.MatchesQuery(null, "meal"));
        }
        static ThingDef Food(string defName, string modName)
        {
            ThingDef food = (ThingDef)FormatterServices.GetUninitializedObject(typeof(ThingDef));
            food.defName = defName;
            if (modName != null)
            {
                ModContentPack pack = (ModContentPack)FormatterServices.GetUninitializedObject(typeof(ModContentPack));
                typeof(ModContentPack).GetField("nameInt", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(pack, modName);
                food.modContentPack = pack;
            }

            return food;
        }
    }
}
