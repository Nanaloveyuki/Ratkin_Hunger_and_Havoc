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
