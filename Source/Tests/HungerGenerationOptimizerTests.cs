using HungerAndHavoc.Core;
using HungerAndHavoc.Generation;
using RimWorld;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerGenerationOptimizerTests
    {
        static HungerGenerationOptimizerTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void ExplicitXenotypeWinsOverOptimizationDefault()
        {
            bool previous = SetOptimization(true);
            try
            {
                XenotypeDef chosen = new XenotypeDef();
                XenotypeDef resolved = HungerGenerationOptimizer.ResolveXenotype(new HungerPawnProfile
                {
                    UseExplicitXenotype = true,
                    Xenotype = chosen
                });
                Assert.Same(chosen, resolved);
            }
            finally
            {
                SetOptimization(previous);
            }
        }

        [Fact]
        public void DisabledOptimizationDoesNotInventDefaultXenotype()
        {
            bool previous = SetOptimization(false);
            try
            {
                Assert.Null(HungerGenerationOptimizer.ResolveXenotype(null));
                Assert.Null(HungerGenerationOptimizer.ResolveXenotype(new HungerPawnProfile()));
            }
            finally
            {
                SetOptimization(previous);
            }
        }

        [Fact]
        public void ExplicitEmptyXenotypeStaysEmptyWhileOptimizationIsEnabled()
        {
            bool previous = SetOptimization(true);
            try
            {
                XenotypeDef xenotype = HungerGenerationOptimizer.ResolveXenotype(new HungerPawnProfile
                {
                    UseExplicitXenotype = true
                });
                Assert.Null(xenotype);
            }
            finally
            {
                SetOptimization(previous);
            }
        }

        [Fact]
        public void WeightDrawUsesShareAndSkipsZero()
        {
            string[] names = { "RK_XenoType_Ratkin", "Ratkin_OA", "RHAH_Xenotype_Ratkin" };
            float[] weights = { 100f, 0f, 100f };
            Assert.Equal("RK_XenoType_Ratkin", HungerXenotypeWeightTable.Choose(names, weights, 0f));
            Assert.Equal("RK_XenoType_Ratkin", HungerXenotypeWeightTable.Choose(names, weights, 99.9f));
            Assert.Equal("RHAH_Xenotype_Ratkin", HungerXenotypeWeightTable.Choose(names, weights, 100f));
            Assert.Null(HungerXenotypeWeightTable.Choose(names, new[] { 0f, 0f, 0f }, 1f));
        }

        [Fact]
        public void ResolveFallsBackOnlyAfterEmptyWeightDraw()
        {
            Assert.Equal("Ratkin_OA", HungerXenotypeWeightTable.Resolve(true, "Ratkin_OA", true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Null(HungerXenotypeWeightTable.Resolve(true, null, true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Null(HungerXenotypeWeightTable.Resolve(false, null, false, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Equal("RK_XenoType_Ratkin", HungerXenotypeWeightTable.Resolve(false, null, true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Equal("RHAH_Xenotype_Ratkin", HungerXenotypeWeightTable.Resolve(false, null, true, null, "RHAH_Xenotype_Ratkin"));
        }

        [Fact]
        public void ExternalRegistrationDoesNotReplaceBuiltinWeight()
        {
            HungerGeneCatalog.ResetForTests();
            HungerGeneCatalog.RegisterXenotype("Ratkin_OA", 40f, false);
            HungerGeneCatalog.RegisterXenotype(HungerGeneCatalog.DefaultXenotypeDefName, 1f, false);
            Assert.Equal(40f, HungerGeneCatalog.SuggestedWeight("Ratkin_OA"));
            Assert.False(HungerGeneCatalog.IsBuiltin(HungerGeneCatalog.FallbackXenotypeDefName));
        }

        static bool SetOptimization(bool enabled)
        {
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings ?? new HungerAndHavocSettings();
            bool previous = settings.optimizeGeneration;
            settings.optimizeGeneration = enabled;
            HungerAndHavocMod.Settings = settings;
            return previous;
        }
    }
}
