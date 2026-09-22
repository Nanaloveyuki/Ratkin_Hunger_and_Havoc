using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPlagueTests
    {
        [Fact]
        public void SpreadDay_RunsEveryThirdDayOnce()
        {
            Assert.False(HungerPlague.IsSpreadDay(1, -1));
            Assert.False(HungerPlague.IsSpreadDay(2, -1));
            Assert.True(HungerPlague.IsSpreadDay(3, -1));
            Assert.False(HungerPlague.IsSpreadDay(3, 3));
            Assert.True(HungerPlague.IsSpreadDay(6, 3));
        }

        [Fact]
        public void SpreadChance_CapsAtThirtyPercent()
        {
            Assert.Equal(0f, HungerPlague.SpreadChance(0));
            Assert.Equal(0.015f, HungerPlague.SpreadChance(3));
            Assert.Equal(0.30f, HungerPlague.SpreadChance(80));
        }

        [Fact]
        public void BloodPumping_AtLeast120Skips()
        {
            Assert.False(HungerPlague.SkipsBloodPumping(119.9f));
            Assert.True(HungerPlague.SkipsBloodPumping(120f));
        }

        [Fact]
        public void Quarantine_BlocksOnlyJoinHireTransfer()
        {
            List<int> ids = new List<int> { 7 };
            Assert.True(HungerPlague.IsQuarantined(ids, 7));
            Assert.False(HungerPlague.IsQuarantined(ids, 8));
            Assert.True(HungerPlague.BlocksGate(HungerBehaviorGate.JoinColony, true));
            Assert.True(HungerPlague.BlocksGate(HungerBehaviorGate.Hire, true));
            Assert.True(HungerPlague.BlocksGate(HungerBehaviorGate.Transfer, true));
            Assert.False(HungerPlague.BlocksGate(HungerBehaviorGate.Beg, true));
            Assert.False(HungerPlague.BlocksGate(HungerBehaviorGate.JoinColony, false));
        }

        [Fact]
        public void Resolve_CountsOnlyCureAndDeathWhileSick()
        {
            PlagueWatch watch = new PlagueWatch();
            watch.Entries.Add(new PlagueWatchEntry { LoadId = 1, Dead = false, StillSick = false });
            watch.Entries.Add(new PlagueWatchEntry { LoadId = 2, Dead = true, StillSick = true });
            watch.Entries.Add(new PlagueWatchEntry { LoadId = 3, Dead = false, LeftMap = true, StillSick = true });
            watch.Entries.Add(new PlagueWatchEntry { LoadId = 4, Keep = true, StillSick = true });
            watch.Entries.Add(new PlagueWatchEntry { LoadId = 5, Missing = true });

            watch.Entries.Add(new PlagueWatchEntry { LoadId = 2, Dead = true, StillSick = true, Counted = true });

            PlagueTally first = HungerPlague.ResolveQuarantine(watch);
            PlagueTally second = HungerPlague.ResolveQuarantine(watch);

            Assert.Equal(1, first.Recovered);
            Assert.Equal(1, first.Died);
            Assert.Equal(0, second.Recovered);
            Assert.Equal(0, second.Died);
            Assert.False(watch.Entries[3].Counted);
            Assert.True(watch.Entries[2].Counted);
            Assert.True(watch.Entries[4].Counted);
        }

        [Fact]
        public void Return_PicksOneRecoveredSurvivorOnce()
        {
            List<int> recovered = new List<int> { 9, 11 };

            Assert.Equal(9, HungerPlague.ChooseReturn(0, recovered));
            Assert.Equal(0, HungerPlague.ChooseReturn(9, recovered));
            Assert.Equal(0, HungerPlague.ChooseReturn(0, new List<int>()));
        }
    }
}
