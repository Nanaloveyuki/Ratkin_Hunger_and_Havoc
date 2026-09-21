using System.IO;
using System.Runtime.CompilerServices;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnGateOrderTests
    {
        static HungerPawnGateOrderTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void NoMark_AllowsFalse()
        {
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                Assert.False(HungerAndHavocApi.Allows(null, HungerBehaviorGate.Beg));
                Assert.False(new HungerApiHost().Allows(null, HungerBehaviorGate.JoinColony));
            }
            finally
            {
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void HostWiresOverrideThenBehaviorThenDefault()
        {
            string source = File.ReadAllText(HostPath());
            int allows = source.IndexOf("public bool Allows(");
            Assert.True(allows >= 0, "HungerApiHost.Allows missing");
            string body = source.Substring(allows);
            int overrideIndex = body.IndexOf("GetGateOverride");
            int behaviorIndex = body.IndexOf("HungerPawnBehaviors.Query");
            int defaultIndex = body.IndexOf("HungerPawnDefaults.Allows");
            Assert.True(overrideIndex >= 0 && behaviorIndex > overrideIndex && defaultIndex > behaviorIndex);
        }

        [Fact]
        public void OverrideBeatsLaterBehaviorThenDefault()
        {
            HungerPawnBehaviors.ResetForTests();
            try
            {
                HungerPawnState state = new HungerPawnState();
                state.ApplySeed(new HungerPawnSeed(
                    sourceIncidentDisplayId: "I-005",
                    role: HungerPawnRole.Beggar,
                    lifecycle: HungerLifecycle.SeekingFood,
                    attitudeAtArrival: HungerAttitude.Neutral));
                bool baseline = state.Allows(HungerBehaviorGate.Beg);
                Assert.Equal(
                    HungerPawnDefaults.Allows(state.ToSnapshot(), HungerBehaviorGate.Beg),
                    baseline);

                StubBehavior first = new StubBehavior { AllowsResult = !baseline };
                StubBehavior later = new StubBehavior { AllowsResult = baseline };
                StubBehavior skip = new StubBehavior { AllowsResult = null };
                HungerPawnBehaviors.Register(first);
                Assert.Equal(!baseline, state.Allows(HungerBehaviorGate.Beg));

                HungerPawnBehaviors.Register(later);
                Assert.Equal(baseline, state.Allows(HungerBehaviorGate.Beg));

                HungerPawnBehaviors.Register(skip);
                Assert.Equal(baseline, state.Allows(HungerBehaviorGate.Beg));

                state.SetGate(HungerBehaviorGate.Beg, !baseline);
                Assert.Equal(!baseline, state.Allows(HungerBehaviorGate.Beg));

                HungerPawnBehaviors.Unregister(later);
                HungerPawnBehaviors.Unregister(first);
                HungerPawnBehaviors.Unregister(skip);
                Assert.Equal(!baseline, state.Allows(HungerBehaviorGate.Beg));

                state.SetGate(HungerBehaviorGate.Beg, null);
                Assert.Equal(baseline, state.Allows(HungerBehaviorGate.Beg));
            }
            finally
            {
                HungerPawnBehaviors.ResetForTests();
            }
        }

        static string HostPath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Identity", "HungerApiHost.cs"));
        }

        sealed class StubBehavior : IHungerPawnBehavior
        {
            public bool? AllowsResult;

            public bool? Allows(Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate)
            {
                return AllowsResult;
            }

            public bool? ShouldReleaseToColony(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
            {
                return null;
            }
        }
    }
}
