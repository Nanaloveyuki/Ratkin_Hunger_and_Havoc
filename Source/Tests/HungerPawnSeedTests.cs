using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HungerAndHavoc.Api;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnSeedTests
    {
        [Fact]
        public void ConstructorCopiesCollectionsAndRejectsLaterMutation()
        {
            List<int> children = new List<int> { 11, 22 };
            Dictionary<HungerBehaviorGate, bool> gates = new Dictionary<HungerBehaviorGate, bool>
            {
                { HungerBehaviorGate.Beg, true }
            };

            HungerPawnSeed seed = new HungerPawnSeed(
                sourceIncidentDisplayId: "I-005",
                spawnBatchId: 3,
                relationshipGroupId: 7,
                role: HungerPawnRole.Beggar,
                lifecycle: HungerLifecycle.SeekingFood,
                carriesPlague: true,
                attitudeAtArrival: HungerAttitude.Hostile,
                leaveAfterGameTick: 40,
                parentPawnLoadId: 9,
                childPawnLoadIds: children,
                gateOverrides: gates);

            children.Add(33);
            gates[HungerBehaviorGate.Beg] = false;
            gates[HungerBehaviorGate.Steal] = true;

            Assert.Equal(new[] { 11, 22 }, seed.ChildPawnLoadIds.ToArray());
            Assert.True(seed.GateOverrides[HungerBehaviorGate.Beg]);
            Assert.False(seed.GateOverrides.ContainsKey(HungerBehaviorGate.Steal));
            Assert.NotNull(new HungerPawnSeed().ChildPawnLoadIds);
            Assert.Empty(new HungerPawnSeed().ChildPawnLoadIds);
            Assert.NotNull(new HungerPawnSeed().GateOverrides);
            Assert.Empty(new HungerPawnSeed().GateOverrides);
        }

        [Fact]
        public void CloneCopiesCollectionsIndependently()
        {
            HungerPawnSeed original = new HungerPawnSeed(
                childPawnLoadIds: new[] { 1, 2 },
                gateOverrides: new Dictionary<HungerBehaviorGate, bool>
                {
                    { HungerBehaviorGate.Leash, true }
                });
            HungerPawnSeed clone = original.Clone();

            Assert.NotSame(original, clone);
            Assert.Equal(original.ChildPawnLoadIds, clone.ChildPawnLoadIds);
            Assert.Equal(original.GateOverrides, clone.GateOverrides);
            Assert.NotSame(original.ChildPawnLoadIds, clone.ChildPawnLoadIds);
            Assert.NotSame(original.GateOverrides, clone.GateOverrides);

            IList<int> cloneChildren = clone.ChildPawnLoadIds as IList<int>;
            if (cloneChildren != null && !cloneChildren.IsReadOnly)
            {
                cloneChildren.Add(99);
                Assert.DoesNotContain(99, original.ChildPawnLoadIds);
            }

            IDictionary<HungerBehaviorGate, bool> cloneGates =
                clone.GateOverrides as IDictionary<HungerBehaviorGate, bool>;
            if (cloneGates != null && !cloneGates.IsReadOnly)
            {
                cloneGates[HungerBehaviorGate.Leash] = false;
                cloneGates[HungerBehaviorGate.Beg] = true;
                Assert.True(original.GateOverrides[HungerBehaviorGate.Leash]);
                Assert.False(original.GateOverrides.ContainsKey(HungerBehaviorGate.Beg));
            }
        }

        [Fact]
        public void PublicSurfaceIsImmutable()
        {
            foreach (PropertyInfo property in typeof(HungerPawnSeed).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Assert.True(property.GetMethod != null && property.GetMethod.IsPublic, property.Name);
                Assert.True(property.SetMethod == null || !property.SetMethod.IsPublic, property.Name);
            }

            Assert.Empty(typeof(HungerPawnSeed).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
        }
    }
}
