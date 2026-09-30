using System.Collections.Generic;
using HungerAndHavoc.Core;
using HungerAndHavoc.EventMgr;
using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_EventChainTests
    {
        [Fact]
        public void SitesKeepMapAndCaravanApart()
        {
            Assert.Equal(4, RHAH_EventChainRuntime.SiteId(4, 9));
            Assert.Equal(-9, RHAH_EventChainRuntime.SiteId(0, 9));
            Assert.Equal(7, RHAH_EventChainRuntime.SiteId(RHAH_IncidentTarget.Map, 7));
            Assert.Equal(-7, RHAH_EventChainRuntime.SiteId(RHAH_IncidentTarget.Caravan, -7));
            Assert.Equal(0, RHAH_EventChainRuntime.SiteId(RHAH_IncidentTarget.Map, 0));
        }

        [Fact]
        public void SameChainCanRunOnTwoSites()
        {
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 1;
            int map = RHAH_EventChainRuntime.Start(records, ref next, "rift", 4, 10, 40, "hole");
            int caravan = RHAH_EventChainRuntime.Start(records, ref next, "rift", -9, 10, -1, null);

            Assert.Equal(1, map);
            Assert.Equal(2, caravan);
            Assert.Equal(3, next);
            Assert.Equal(2, RHAH_EventChainRuntime.ActiveCount(records, "rift", int.MinValue));
            Assert.Equal("hole", records[0].payload);
            Assert.Equal("", records[1].payload);
        }

        [Fact]
        public void DueTickExpiresOnceAndStageStaysUntilTheEventWritesIt()
        {
            RHAH_EventChains.Clear();
            Probe probe = new Probe { Accept = true };
            RHAH_EventChains.Register(probe);
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 1;
            int id = RHAH_EventChainRuntime.Start(records, ref next, "rift", 4, 10, 20, "");

            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, 19));
            Assert.False(records[0].closed);
            Assert.Equal(1, probe.Ticks);

            Assert.True(RHAH_EventChainRuntime.SetStage(records, id, 2, 30, "dig"));
            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, 20));
            Assert.Equal(2, records[0].stage);
            Assert.Equal("dig", records[0].payload);

            Assert.Equal(1, RHAH_EventChainRuntime.TickDue(records, 30));
            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, 31));
            Assert.Equal((int)RHAH_EventChainEnd.Expired, records[0].end);
            Assert.Equal(1, probe.Ends);
            RHAH_EventChains.Clear();
        }

        [Fact]
        public void CheckpointStartsOnlyChainsThatAcceptTheSite()
        {
            RHAH_EventChains.Clear();
            Probe ready = new Probe { Display = "ready", Accept = true, Deadline = 50, Stage = 1, Payload = "mark" };
            Probe blocked = new Probe { Display = "blocked", Accept = false };
            Probe refused = new Probe { Display = "refused", Accept = true, Start = false };
            RHAH_EventChains.Register(ready);
            RHAH_EventChains.Register(blocked);
            RHAH_EventChains.Register(refused);
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 1;

            Assert.Equal(1, RHAH_EventChainRuntime.Check(records, ref next, new RHAH_EventChainSite(4, 0, 8), 12));
            Assert.Equal(1, RHAH_EventChainRuntime.ActiveCount(records, null, int.MinValue));
            Assert.Equal("ready", records[0].displayId);
            Assert.Equal(4, records[0].siteId);
            Assert.Equal(1, records[0].stage);
            Assert.Equal(50, records[0].deadlineTick);
            Assert.Equal("mark", records[0].payload);
            Assert.True(records[1].closed);
            Assert.Equal((int)RHAH_EventChainEnd.Cancelled, records[1].end);
            Assert.Equal(0, blocked.Starts);
            RHAH_EventChains.Clear();
        }

        [Fact]
        public void ExplicitEndNotifiesOnceAndASecondEndDoesNothing()
        {
            RHAH_EventChains.Clear();
            Probe probe = new Probe { Accept = true };
            RHAH_EventChains.Register(probe);
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 1;
            int id = RHAH_EventChainRuntime.Start(records, ref next, "rift", -3, 5, -1, "");

            Assert.True(RHAH_EventChainRuntime.End(records, id, 9, RHAH_EventChainEnd.Completed));
            Assert.False(RHAH_EventChainRuntime.End(records, id, 10, RHAH_EventChainEnd.Failed));
            Assert.Equal(1, probe.Ends);
            Assert.Equal(RHAH_EventChainEnd.Completed, probe.LastEnd);
            Assert.Equal(0, RHAH_EventChainRuntime.ActiveCount(records, "rift", -3));
            RHAH_EventChains.Clear();
        }

        [Fact]
        public void LoadDropsBrokenRecordsAndKeepsTheNextIdAboveThem()
        {
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>
            {
                null,
                new RHAH_EventChainRecord { id = 0, displayId = "rift" },
                new RHAH_EventChainRecord { id = 4, displayId = "rift", payload = null, deadlineTick = -8 }
            };
            int next = 1;

            RHAH_EventChainRuntime.Repair(records, ref next);

            Assert.Single(records);
            Assert.Equal("rift", records[0].displayId);
            Assert.Equal("", records[0].payload);
            Assert.Equal(-1, records[0].deadlineTick);
            Assert.Equal(5, next);
        }

        [Fact]
        public void CheckpointIntervalMatchesTheStorytellerCheck()
        {
            Assert.True(RHAH_EventChainClock.Due(1000, 1000));
            Assert.False(RHAH_EventChainClock.Due(999, 1000));
            Assert.False(RHAH_EventChainClock.Due(0, 0));
        }

        [Fact]
        public void GameComponentStoresAndEndsAChainWithoutTouchingTheIncidentQueue()
        {
            RHAH_EventChains.Clear();
            Probe probe = new Probe { Accept = true };
            RHAH_EventChains.Register(probe);
            GameComponent_RHAH_Game game = new GameComponent_RHAH_Game(null);

            int id = game.StartEventChain("rift", 4, 10, 20, "hole");
            Assert.Equal(1, id);
            Assert.Empty(game.PendingIncidentDisplayIds);
            Assert.True(game.SetEventChainStage(id, 3, 40, "open"));
            Assert.True(game.EndEventChain(id, 15, RHAH_EventChainEnd.Failed));
            Assert.False(game.EndEventChain(id, 16, RHAH_EventChainEnd.Completed));
            Assert.Equal(1, probe.Ends);
            Assert.Equal(RHAH_EventChainEnd.Failed, probe.LastEnd);
            RHAH_EventChains.Clear();
        }

        sealed class Probe : IRHAH_EventChain
        {
            internal string Display = "rift";
            internal bool Accept;
            internal bool Start = true;
            internal int Deadline = -1;
            internal int Stage;
            internal string Payload = "";
            internal int Ticks;
            internal int Starts;
            internal int Ends;
            internal RHAH_EventChainEnd LastEnd;

            public string DisplayId => Display;

            public bool Owns(string displayId)
            {
                return displayId == DisplayId;
            }

            public bool CanStart(RHAH_EventChainSite site, int tick)
            {
                return Accept;
            }

            public bool OnStart(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context)
            {
                Starts++;
                if (!Start)
                {
                    return false;
                }

                return RHAH_EventChainRuntime.SetStage(records, context.InstanceId, Stage, Deadline, Payload);
            }

            public bool OnTick(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context)
            {
                Ticks++;
                return true;
            }

            public void OnEnd(RHAH_EventChainContext context, RHAH_EventChainEnd end)
            {
                Ends++;
                LastEnd = end;
            }
        }
    }
}
