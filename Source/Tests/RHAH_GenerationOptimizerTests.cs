using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using HungerAndHavoc.Core;
using HungerAndHavoc.Generation;
using RimWorld;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_GenerationOptimizerTests
    {
        static RHAH_GenerationOptimizerTests()
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
                XenotypeDef resolved = RHAH_GenerationOptimizer.ResolveXenotype(new RHAH_PawnProfile
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
                Assert.Null(RHAH_GenerationOptimizer.ResolveXenotype(null));
                Assert.Null(RHAH_GenerationOptimizer.ResolveXenotype(new RHAH_PawnProfile()));
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
                XenotypeDef xenotype = RHAH_GenerationOptimizer.ResolveXenotype(new RHAH_PawnProfile
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
            Assert.Equal("RK_XenoType_Ratkin", RHAH_XenotypeWeightTable.Choose(names, weights, 0f));
            Assert.Equal("RK_XenoType_Ratkin", RHAH_XenotypeWeightTable.Choose(names, weights, 99.9f));
            Assert.Equal("RHAH_Xenotype_Ratkin", RHAH_XenotypeWeightTable.Choose(names, weights, 100f));
            Assert.Null(RHAH_XenotypeWeightTable.Choose(names, new[] { 0f, 0f, 0f }, 1f));
        }

        [Fact]
        public void ResolveFallsBackOnlyAfterEmptyWeightDraw()
        {
            Assert.Equal("Ratkin_OA", RHAH_XenotypeWeightTable.Resolve(true, "Ratkin_OA", true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Null(RHAH_XenotypeWeightTable.Resolve(true, null, true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Null(RHAH_XenotypeWeightTable.Resolve(false, null, false, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Equal("RK_XenoType_Ratkin", RHAH_XenotypeWeightTable.Resolve(false, null, true, "RK_XenoType_Ratkin", "RHAH_Xenotype_Ratkin"));
            Assert.Equal("RHAH_Xenotype_Ratkin", RHAH_XenotypeWeightTable.Resolve(false, null, true, null, "RHAH_Xenotype_Ratkin"));
        }

        [Fact]
        public void ExternalRegistrationDoesNotReplaceBuiltinWeight()
        {
            RHAH_GeneCatalog.ResetForTests();
            RHAH_GeneCatalog.RegisterXenotype("Ratkin_OA", 40f, false);
            RHAH_GeneCatalog.RegisterXenotype(RHAH_GeneCatalog.DefaultXenotypeDefName, 1f, false);
            Assert.Equal(40f, RHAH_GeneCatalog.SuggestedWeight("Ratkin_OA"));
            Assert.False(RHAH_GeneCatalog.IsBuiltin(RHAH_GeneCatalog.FallbackXenotypeDefName));
        }

        [Fact]
        public void XenotypesGroupBySourceModAndKeepOrder()
        {
            XenotypeDef first = Xenotype("RK_XenoType_Ratkin", "New Ratkin Plus");
            XenotypeDef second = Xenotype("RHAH_Xenotype_Ratkin", "Ratkin: Hunger and Havoc");
            XenotypeDef third = Xenotype("Ratkin_OA", "New Ratkin Plus");
            XenotypeDef unknown = new XenotypeDef { defName = "LooseRatkin" };
            List<List<XenotypeDef>> groups = RHAH_XenotypeResolver.GroupBySourceMod(new List<XenotypeDef>
            {
                first,
                second,
                third,
                unknown
            });
            Assert.Equal(3, groups.Count);
            Assert.Equal(new[] { first, third }, groups[0]);
            Assert.Equal(new[] { second }, groups[1]);
            Assert.Equal(new[] { unknown }, groups[2]);
            Assert.Equal("New Ratkin Plus", RHAH_XenotypeResolver.SourceModName(first));
            Assert.Null(RHAH_XenotypeResolver.SourceModName(unknown));
            Assert.Empty(RHAH_XenotypeResolver.GroupBySourceMod(null));
        }

        static XenotypeDef Xenotype(string defName, string modName)
        {
            ModContentPack pack = (ModContentPack)FormatterServices.GetUninitializedObject(typeof(ModContentPack));
            typeof(ModContentPack).GetField("nameInt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pack, modName);
            return new XenotypeDef
            {
                defName = defName,
                modContentPack = pack
            };
        }

        static bool SetOptimization(bool enabled)
        {
            RHAH_Settings settings = RHAH_Mod.Settings ?? new RHAH_Settings();
            bool previous = settings.optimizeGeneration;
            settings.optimizeGeneration = enabled;
            RHAH_Mod.Settings = settings;
            return previous;
        }
    }
}
