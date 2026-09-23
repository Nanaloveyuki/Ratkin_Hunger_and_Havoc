using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_PlagueTests
    {
        [Fact]
        public void SpreadDay_RunsEveryThirdDayOnce()
        {
            Assert.False(RHAH_Plague.IsSpreadDay(1, -1));
            Assert.False(RHAH_Plague.IsSpreadDay(2, -1));
            Assert.True(RHAH_Plague.IsSpreadDay(3, -1));
            Assert.False(RHAH_Plague.IsSpreadDay(3, 3));
            Assert.True(RHAH_Plague.IsSpreadDay(6, 3));
        }

        [Fact]
        public void SpreadChance_CapsAtThirtyPercent()
        {
            Assert.Equal(0f, RHAH_Plague.SpreadChance(0));
            Assert.Equal(0.015f, RHAH_Plague.SpreadChance(3));
            Assert.Equal(0.30f, RHAH_Plague.SpreadChance(80));
        }

        [Fact]
        public void BloodPumping_AtLeast120Skips()
        {
            Assert.False(RHAH_Plague.SkipsBloodPumping(119.9f));
            Assert.True(RHAH_Plague.SkipsBloodPumping(120f));
        }

        [Fact]
        public void Quarantine_BlocksOnlyJoinHireTransfer()
        {
            List<int> ids = new List<int> { 7 };
            Assert.True(RHAH_Plague.IsQuarantined(ids, 7));
            Assert.False(RHAH_Plague.IsQuarantined(ids, 8));
            Assert.True(RHAH_Plague.BlocksGate(RHAH_BehaviorGate.JoinColony, true));
            Assert.True(RHAH_Plague.BlocksGate(RHAH_BehaviorGate.Hire, true));
            Assert.True(RHAH_Plague.BlocksGate(RHAH_BehaviorGate.Transfer, true));
            Assert.False(RHAH_Plague.BlocksGate(RHAH_BehaviorGate.Beg, true));
            Assert.False(RHAH_Plague.BlocksGate(RHAH_BehaviorGate.JoinColony, false));
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

            PlagueTally first = RHAH_Plague.ResolveQuarantine(watch);
            PlagueTally second = RHAH_Plague.ResolveQuarantine(watch);

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

            Assert.Equal(9, RHAH_Plague.ChooseReturn(0, recovered));
            Assert.Equal(0, RHAH_Plague.ChooseReturn(9, recovered));
            Assert.Equal(0, RHAH_Plague.ChooseReturn(0, new List<int>()));
        }
    }
}
