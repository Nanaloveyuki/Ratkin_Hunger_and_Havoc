using System.Collections.Generic;
using HungerAndHavoc.Incidents;
using RimWorld.Planet;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_ApproachRulesTests
    {
        [Fact]
        public void OnlyMapIncidentsWalkIn()
        {
            Assert.True(RHAH_ApproachRules.UsesApproach(RHAH_IncidentTarget.Map));
            Assert.False(RHAH_ApproachRules.UsesApproach(RHAH_IncidentTarget.Caravan));
        }

        [Fact]
        public void FlatTileTakesAThirdOfADay()
        {
            Assert.Equal(20000, RHAH_ApproachRules.MoveTicks(1f, 1f));
            Assert.Equal(10000, RHAH_ApproachRules.MoveTicks(1f, 0.5f));
            Assert.Equal(20000, RHAH_ApproachRules.MoveTicks(0f, 1f));
            Assert.Equal(30000, RHAH_ApproachRules.MoveTicks(2f, 1f));
        }

        [Fact]
        public void SpendingStopsAtZero()
        {
            Assert.Equal(64, RHAH_ApproachRules.Spend(128, 64));
            Assert.Equal(0, RHAH_ApproachRules.Spend(10, 64));
            Assert.Equal(10, RHAH_ApproachRules.Spend(10, 0));
        }

        [Fact]
        public void DistanceStaysBetweenFourAndSix()
        {
            Assert.Equal(4, RHAH_ApproachRules.PickDistance(0));
            Assert.Equal(5, RHAH_ApproachRules.PickDistance(1));
            Assert.Equal(6, RHAH_ApproachRules.PickDistance(2));
            Assert.Equal(4, RHAH_ApproachRules.PickDistance(3));
            Assert.Equal(6, RHAH_ApproachRules.PickDistance(-1));
        }

        [Fact]
        public void IslandCrossingStartsOnTheNearShore()
        {
            Assert.False(RHAH_ApproachRules.CanWalk(true, false));
            Assert.True(RHAH_ApproachRules.CanWalk(true, true));
            Assert.Equal(-1, RHAH_ApproachRules.PickCrossing(-1, 8, int.MaxValue, 3));
            Assert.Equal(8, RHAH_ApproachRules.PickCrossing(-1, 8, int.MaxValue, 4));
            Assert.Equal(8, RHAH_ApproachRules.PickCrossing(8, 9, 4, 6));
            Assert.False(RHAH_ApproachRules.ReleasePath(true, false));
            Assert.False(RHAH_ApproachRules.ReleasePath(false, true));
            Assert.True(RHAH_ApproachRules.ReleasePath(false, false));
        }

        [Fact]
        public void SingleHomeIsTheOnlyTarget()
        {
            Assert.Equal(7, RHAH_ApproachRules.PickHome(new List<int> { 7 }, 4));
            Assert.Equal(9, RHAH_ApproachRules.PickHome(new List<int> { 7, 9 }, 1));
            Assert.Equal(0, RHAH_ApproachRules.PickHome(null, 1));
        }

        [Fact]
        public void AdjacentPathYieldsTheColonyOnce()
        {
            using (WorldPath path = Path(8, 3))
            {
                Assert.True(RHAH_ApproachRules.TryConsumeNext(path, out PlanetTile next));
                Assert.Equal(8, next.tileId);
                Assert.Equal(1, path.NodesLeftCount);
            }

            using (WorldPath longer = Path(8, 5, 3))
            {
                Assert.True(RHAH_ApproachRules.TryConsumeNext(longer, out PlanetTile next));
                Assert.Equal(5, next.tileId);
                Assert.Equal(2, longer.NodesLeftCount);
            }

            using (WorldPath arrived = Path(8))
            {
                Assert.False(RHAH_ApproachRules.TryConsumeNext(arrived, out _));
            }

            Assert.False(RHAH_ApproachRules.TryConsumeNext(null, out _));
            Assert.False(RHAH_ApproachRules.TryConsumeNext(WorldPath.NotFound, out _));

            using (WorldPath corrupt = Path(8, 3))
            {
                corrupt.NodesReversed.Clear();
                Assert.False(RHAH_ApproachRules.TryConsumeNext(corrupt, out _));
            }
        }

        static WorldPath Path(params int[] destinationFirst)
        {
            WorldPath path = new WorldPath();
            for (int i = 0; i < destinationFirst.Length; i++)
            {
                path.AddNodeAtStart(new PlanetTile(destinationFirst[i]));
            }

            path.SetupFound(1f, null);
            return path;
        }
    }
}
