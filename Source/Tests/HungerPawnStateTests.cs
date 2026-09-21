using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnStateTests
    {
        [Theory]
        [InlineData(HungerLifecycle.Arriving, HungerLifecycle.SeekingFood, true)]
        [InlineData(HungerLifecycle.Arriving, HungerLifecycle.Leaving, true)]
        [InlineData(HungerLifecycle.Arriving, HungerLifecycle.Released, true)]
        [InlineData(HungerLifecycle.Arriving, HungerLifecycle.Dead, true)]
        [InlineData(HungerLifecycle.Arriving, HungerLifecycle.Fed, false)]
        [InlineData(HungerLifecycle.SeekingFood, HungerLifecycle.Fed, true)]
        [InlineData(HungerLifecycle.SeekingFood, HungerLifecycle.Leaving, true)]
        [InlineData(HungerLifecycle.SeekingFood, HungerLifecycle.Released, true)]
        [InlineData(HungerLifecycle.SeekingFood, HungerLifecycle.Dead, true)]
        [InlineData(HungerLifecycle.SeekingFood, HungerLifecycle.Arriving, false)]
        [InlineData(HungerLifecycle.Fed, HungerLifecycle.Leaving, true)]
        [InlineData(HungerLifecycle.Fed, HungerLifecycle.Released, true)]
        [InlineData(HungerLifecycle.Fed, HungerLifecycle.Dead, true)]
        [InlineData(HungerLifecycle.Fed, HungerLifecycle.SeekingFood, false)]
        [InlineData(HungerLifecycle.Fed, HungerLifecycle.Arriving, false)]
        [InlineData(HungerLifecycle.Leaving, HungerLifecycle.Released, true)]
        [InlineData(HungerLifecycle.Leaving, HungerLifecycle.Dead, true)]
        [InlineData(HungerLifecycle.Leaving, HungerLifecycle.Fed, false)]
        [InlineData(HungerLifecycle.Leaving, HungerLifecycle.SeekingFood, false)]
        [InlineData(HungerLifecycle.Released, HungerLifecycle.Dead, true)]
        [InlineData(HungerLifecycle.Released, HungerLifecycle.Leaving, false)]
        [InlineData(HungerLifecycle.Released, HungerLifecycle.Arriving, false)]
        [InlineData(HungerLifecycle.Dead, HungerLifecycle.Released, false)]
        [InlineData(HungerLifecycle.Dead, HungerLifecycle.Arriving, false)]
        [InlineData(HungerLifecycle.Dead, HungerLifecycle.SeekingFood, false)]
        [InlineData(HungerLifecycle.Dead, HungerLifecycle.Fed, false)]
        [InlineData(HungerLifecycle.Dead, HungerLifecycle.Leaving, false)]
        public void TrySetLifecycle_RespectsLegalTransitions(
            HungerLifecycle from,
            HungerLifecycle to,
            bool legal)
        {
            HungerPawnState state = StateWithLifecycle(from);

            bool changed = state.TrySetLifecycle(to);

            Assert.Equal(legal, changed);
            Assert.Equal(legal ? to : from, state.lifecycle);
        }

        [Fact]
        public void TrySetLifecycle_SameValueDoesNotChange()
        {
            HungerPawnState state = StateWithLifecycle(HungerLifecycle.SeekingFood);

            bool changed = state.TrySetLifecycle(HungerLifecycle.SeekingFood);

            Assert.False(changed);
            Assert.Equal(HungerLifecycle.SeekingFood, state.lifecycle);
        }

        [Fact]
        public void TrySetLifecycle_FedSetsHasBeenFed()
        {
            HungerPawnState state = StateWithLifecycle(HungerLifecycle.SeekingFood);

            Assert.True(state.TrySetLifecycle(HungerLifecycle.Fed));
            Assert.True(state.hasBeenFed);
            Assert.True(state.TrySetLifecycle(HungerLifecycle.Leaving));
            Assert.True(state.hasBeenFed);
        }

        [Fact]
        public void TrySetLifecycle_IllegalDoesNotSetHasBeenFed()
        {
            HungerPawnState state = StateWithLifecycle(HungerLifecycle.Arriving);

            Assert.False(state.TrySetLifecycle(HungerLifecycle.Fed));
            Assert.False(state.hasBeenFed);
            Assert.Equal(HungerLifecycle.Arriving, state.lifecycle);
        }

        [Fact]
        public void ApplySeed_OverwritesIdentityAndCopiesCollections()
        {
            HungerPawnState state = new HungerPawnState();
            state.SetGate(HungerBehaviorGate.Beg, false);
            state.SetExtra("keep", "yes");
            List<int> children = new List<int> { 7, 8 };
            Dictionary<HungerBehaviorGate, bool> gates = new Dictionary<HungerBehaviorGate, bool>
            {
                { HungerBehaviorGate.Steal, true }
            };
            HungerPawnSeed seed = new HungerPawnSeed(
                "I-005",
                3,
                9,
                HungerPawnRole.Beggar,
                HungerLifecycle.SeekingFood,
                true,
                HungerAttitude.Hostile,
                42,
                11,
                children,
                gates);

            state.ApplySeed(seed);
            children.Add(99);
            gates[HungerBehaviorGate.Fight] = true;
            state.childPawnLoadIds.Add(100);
            state.SetGate(HungerBehaviorGate.LeaveAfterFed, false);

            Assert.Equal("I-005", state.sourceIncidentDisplayId);
            Assert.Equal(3, state.spawnBatchId);
            Assert.Equal(9, state.relationshipGroupId);
            Assert.Equal(HungerPawnRole.Beggar, state.role);
            Assert.Equal(HungerLifecycle.SeekingFood, state.lifecycle);
            Assert.True(state.carriesPlague);
            Assert.Equal(HungerAttitude.Hostile, state.attitudeAtArrival);
            Assert.Equal(42, state.leaveAfterGameTick);
            Assert.Equal(11, state.parentPawnLoadId);
            Assert.Equal(new[] { 7, 8, 100 }, state.childPawnLoadIds);
            Assert.Equal(new[] { 7, 8 }, seed.ChildPawnLoadIds);
            Assert.True(state.GetGateOverride(HungerBehaviorGate.Steal));
            Assert.False(state.GetGateOverride(HungerBehaviorGate.LeaveAfterFed));
            Assert.Null(state.GetGateOverride(HungerBehaviorGate.Beg));
            Assert.False(seed.GateOverrides.ContainsKey(HungerBehaviorGate.Fight));
            Assert.True(state.TryGetExtra("keep", out string kept));
            Assert.Equal("yes", kept);
        }

        [Fact]
        public void ApplySeed_ReplacesPreviousGateOverrides()
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                gateOverrides: new Dictionary<HungerBehaviorGate, bool>
                {
                    { HungerBehaviorGate.Beg, true },
                    { HungerBehaviorGate.Steal, false }
                }));
            state.ApplySeed(new HungerPawnSeed(
                role: HungerPawnRole.Thief,
                gateOverrides: new Dictionary<HungerBehaviorGate, bool>
                {
                    { HungerBehaviorGate.Steal, true }
                }));

            Assert.Equal(HungerPawnRole.Thief, state.role);
            Assert.Null(state.GetGateOverride(HungerBehaviorGate.Beg));
            Assert.True(state.GetGateOverride(HungerBehaviorGate.Steal));
        }

        [Fact]
        public void ApplySeed_IgnoresNull()
        {
            HungerPawnState state = StateWithLifecycle(HungerLifecycle.Arriving);
            state.sourceIncidentDisplayId = "I-001";

            state.ApplySeed(null);

            Assert.Equal("I-001", state.sourceIncidentDisplayId);
            Assert.Equal(HungerLifecycle.Arriving, state.lifecycle);
        }

        [Fact]
        public void SetGate_OverrideAndClear()
        {
            HungerPawnState state = new HungerPawnState();

            state.SetGate(HungerBehaviorGate.Fight, true);
            Assert.True(state.GetGateOverride(HungerBehaviorGate.Fight));

            state.SetGate(HungerBehaviorGate.Fight, false);
            Assert.False(state.GetGateOverride(HungerBehaviorGate.Fight));

            state.SetGate(HungerBehaviorGate.Fight, null);
            Assert.Null(state.GetGateOverride(HungerBehaviorGate.Fight));
        }

        [Fact]
        public void Allows_OverrideBeatsDefault()
        {
            HungerPawnBehaviors.ResetForTests();
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                role: HungerPawnRole.Beggar,
                lifecycle: HungerLifecycle.SeekingFood));

            Assert.True(state.Allows(HungerBehaviorGate.Beg));
            state.SetGate(HungerBehaviorGate.Beg, false);
            Assert.False(state.Allows(HungerBehaviorGate.Beg));
            state.SetGate(HungerBehaviorGate.Beg, null);
            Assert.True(state.Allows(HungerBehaviorGate.Beg));
        }

        [Fact]
        public void ExtraData_SetGetAndDelete()
        {
            HungerPawnState state = new HungerPawnState();

            state.SetExtra("mod.key", "a");
            Assert.True(state.TryGetExtra("mod.key", out string value));
            Assert.Equal("a", value);

            state.SetExtra("mod.key", "b");
            Assert.True(state.TryGetExtra("mod.key", out value));
            Assert.Equal("b", value);

            state.SetExtra("mod.key", null);
            Assert.False(state.TryGetExtra("mod.key", out value));
            Assert.Null(value);

            state.SetExtra(null, "x");
            state.SetExtra("", "x");
            Assert.False(state.TryGetExtra(null, out value));
            Assert.Null(value);
            Assert.False(state.TryGetExtra("", out value));
            Assert.Null(value);
            Assert.False(state.TryGetExtra("missing", out value));
            Assert.Null(value);
        }

        [Fact]
        public void ToSnapshot_IsCopy()
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                "I-014",
                1,
                2,
                HungerPawnRole.Refugee,
                HungerLifecycle.Fed,
                false,
                HungerAttitude.Neutral,
                8,
                4,
                new[] { 1, 2 }));
            state.hasBeenFed = true;

            HungerPawnSnapshot snapshot = state.ToSnapshot();
            state.TrySetLifecycle(HungerLifecycle.Leaving);
            state.childPawnLoadIds.Add(3);
            state.sourceIncidentDisplayId = "I-015";

            Assert.Equal("I-014", snapshot.SourceIncidentDisplayId);
            Assert.Equal(HungerLifecycle.Fed, snapshot.Lifecycle);
            Assert.Equal(new[] { 1, 2 }, snapshot.ChildPawnLoadIds);
            Assert.True(snapshot.HasBeenFed);
            Assert.False(snapshot.IsReleased);
            Assert.True(snapshot.IsActiveVisitor);
            Assert.Equal(HungerLifecycle.Leaving, state.lifecycle);
        }

        [Fact]
        public void IsReleasedAndIsActiveVisitor_FollowLifecycle()
        {
            HungerPawnState state = new HungerPawnState();
            Assert.False(state.IsReleased);
            Assert.True(state.IsActiveVisitor);

            state.ApplySeed(new HungerPawnSeed(lifecycle: HungerLifecycle.Released));
            Assert.True(state.IsReleased);
            Assert.False(state.IsActiveVisitor);

            state.ApplySeed(new HungerPawnSeed(lifecycle: HungerLifecycle.Dead));
            Assert.False(state.IsReleased);
            Assert.False(state.IsActiveVisitor);
        }

        [Fact]
        public void EnsureCollections_TreatsNullAsEmpty()
        {
            HungerPawnState state = new HungerPawnState();
            state.childPawnLoadIds = null;
            state.gateOverrides = null;
            state.extraData = null;

            state.EnsureCollections();

            Assert.Empty(state.childPawnLoadIds);
            Assert.Empty(state.gateOverrides);
            Assert.Empty(state.extraData);
        }

        static HungerPawnState StateWithLifecycle(HungerLifecycle lifecycle)
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(lifecycle: lifecycle));
            return state;
        }
    }
}
