using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnReleaseTests
    {
        static HungerPawnReleaseTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void HostWithoutMark_ReleaseFails()
        {
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                Assert.False(HungerAndHavocApi.ReleaseToColony(null, HungerReleaseReason.Recruited));
                Assert.False(new HungerApiHost().ReleaseToColony(null, HungerReleaseReason.Recruited));
            }
            finally
            {
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void ReleaseToColonyAlreadyReleasedPolicyRejectAndSuccess()
        {
            HungerPawnBehaviors.ResetForTests();
            int releasedEvents = 0;
            int lifecycleEvents = 0;
            HungerAndHavocApi.ReleasedToColony += OnReleased;
            HungerAndHavocApi.LifecycleChanged += OnLifecycle;
            try
            {
                HungerPawnState state = new HungerPawnState();
                state.ApplySeed(new HungerPawnSeed(
                    sourceIncidentDisplayId: "I-001",
                    role: HungerPawnRole.Refugee,
                    lifecycle: HungerLifecycle.Arriving));

                HungerPawnBehaviors.Register(new StubBehavior { ReleaseResult = false });
                Assert.False(state.ReleaseToColony(HungerReleaseReason.Recruited));
                Assert.Equal(HungerLifecycle.Arriving, state.lifecycle);
                Assert.Equal(0, releasedEvents);
                Assert.Equal(0, lifecycleEvents);

                HungerPawnBehaviors.ResetForTests();
                Assert.True(state.ReleaseToColony(HungerReleaseReason.Recruited));
                Assert.Equal(HungerLifecycle.Released, state.lifecycle);
                Assert.True(state.IsReleased);
                Assert.Equal(1, releasedEvents);
                Assert.Equal(1, lifecycleEvents);

                Assert.True(state.ReleaseToColony(HungerReleaseReason.Imprisoned));
                Assert.Equal(HungerLifecycle.Released, state.lifecycle);
                Assert.Equal(1, releasedEvents);
                Assert.Equal(1, lifecycleEvents);
            }
            finally
            {
                HungerAndHavocApi.ReleasedToColony -= OnReleased;
                HungerAndHavocApi.LifecycleChanged -= OnLifecycle;
                HungerPawnBehaviors.ResetForTests();
            }

            void OnReleased(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
            {
                releasedEvents++;
                Assert.NotNull(snapshot);
                Assert.True(snapshot.IsReleased);
            }

            void OnLifecycle(Pawn pawn, IHungerPawn snapshot, HungerLifecycle lifecycle)
            {
                lifecycleEvents++;
                Assert.Equal(HungerLifecycle.Released, lifecycle);
            }
        }

        [Fact]
        public void IllegalLifecycleDoesNotRaiseEvent()
        {
            int lifecycleEvents = 0;
            HungerAndHavocApi.LifecycleChanged += OnLifecycle;
            try
            {
                HungerPawnState state = new HungerPawnState();
                state.ApplySeed(new HungerPawnSeed(lifecycle: HungerLifecycle.Arriving));
                Assert.False(state.TrySetLifecycle(HungerLifecycle.Fed));
                Assert.Equal(HungerLifecycle.Arriving, state.lifecycle);
                Assert.Equal(0, lifecycleEvents);

                Assert.True(state.TrySetLifecycle(HungerLifecycle.SeekingFood));
                Assert.Equal(0, lifecycleEvents);

                Assert.False(state.TrySetLifecycle(HungerLifecycle.SeekingFood));
                Assert.Equal(HungerLifecycle.SeekingFood, state.lifecycle);
                Assert.Equal(0, lifecycleEvents);
            }
            finally
            {
                HungerAndHavocApi.LifecycleChanged -= OnLifecycle;
            }

            void OnLifecycle(Pawn pawn, IHungerPawn snapshot, HungerLifecycle lifecycle)
            {
                lifecycleEvents++;
            }
        }

        sealed class StubBehavior : IHungerPawnBehavior
        {
            public bool? ReleaseResult;

            public bool? Allows(Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate)
            {
                return null;
            }

            public bool? ShouldReleaseToColony(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason)
            {
                return ReleaseResult;
            }
        }
    }
}
