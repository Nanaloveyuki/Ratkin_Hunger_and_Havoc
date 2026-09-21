using HungerAndHavoc.Api;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnBehaviorsTests
    {
        static HungerPawnBehaviorsTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void QueryUsesLaterRegistrationAndSkipsNull()
        {
            HungerPawnBehaviors.ResetForTests();
            try
            {
                HungerPawnSnapshot snapshot = new HungerPawnSnapshot(
                    "I-001",
                    1,
                    1,
                    HungerPawnRole.Beggar,
                    HungerLifecycle.SeekingFood,
                    false,
                    -1,
                    false,
                    HungerAttitude.Neutral,
                    0,
                    null);
                StubBehavior first = new StubBehavior { AllowsResult = true };
                StubBehavior second = new StubBehavior { AllowsResult = false };
                StubBehavior third = new StubBehavior { AllowsResult = null };

                HungerPawnBehaviors.Register(null);
                HungerPawnBehaviors.Register(first);
                HungerPawnBehaviors.Register(first);
                HungerPawnBehaviors.Register(second);
                HungerPawnBehaviors.Register(third);

                Assert.False(HungerPawnBehaviors.Query(behavior =>
                    behavior.Allows(null, snapshot, HungerBehaviorGate.Beg)));

                HungerPawnBehaviors.Unregister(second);
                Assert.True(HungerPawnBehaviors.Query(behavior =>
                    behavior.Allows(null, snapshot, HungerBehaviorGate.Beg)));

                HungerPawnBehaviors.Unregister(first);
                HungerPawnBehaviors.Unregister(third);
                HungerPawnBehaviors.Unregister(null);
                Assert.Null(HungerPawnBehaviors.Query(behavior =>
                    behavior.Allows(null, snapshot, HungerBehaviorGate.Beg)));
                Assert.Null(HungerPawnBehaviors.Query(null));
            }
            finally
            {
                HungerPawnBehaviors.ResetForTests();
            }
        }

        [Fact]
        public void ResetForTestsClearsHandlers()
        {
            HungerPawnBehaviors.ResetForTests();
            try
            {
                HungerPawnBehaviors.Register(new StubBehavior { AllowsResult = true });
                HungerPawnBehaviors.ResetForTests();
                Assert.Null(HungerPawnBehaviors.Query(behavior =>
                    behavior.Allows(null, DummySnapshot(), HungerBehaviorGate.Steal)));
            }
            finally
            {
                HungerPawnBehaviors.ResetForTests();
            }
        }

        static HungerPawnSnapshot DummySnapshot()
        {
            return new HungerPawnSnapshot(
                "I-001",
                0,
                0,
                HungerPawnRole.Unspecified,
                HungerLifecycle.Arriving,
                false,
                -1,
                false,
                HungerAttitude.Neutral,
                0,
                null);
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
