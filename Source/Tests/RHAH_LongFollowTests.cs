using System;
using System.Collections.Generic;
using HungerAndHavoc.EventMgr;
using Oxygendsl.Core.Events;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public sealed class RHAH_LongFollowTests : IDisposable
    {
        readonly Game previousGame;

        static RHAH_LongFollowTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        public RHAH_LongFollowTests()
        {
            previousGame = Current.Game;
            Current.Game = null;
        }

        public void Dispose()
        {
            Current.Game = previousGame;
        }

        [Fact]
        public void InteractiveStagesKeepPublishedDefinitionAndPayloadIdentity()
        {
            RHAH_EventChainProvider provider = new RHAH_EventChainProvider();
            Assert.Equal(1, provider.StateVersion);
            Assert.Equal(54, provider.Definitions.Count);
            foreach (IEventChainDefinition definition in provider.Definitions)
            {
                if (!RHAH_EventFollowRules.HasLongFollow(definition.Id)) continue;
                EventChainInstance instance = new EventChainInstance
                {
                    Id = 7,
                    DefinitionId = definition.Id,
                    DefinitionVersion = 1,
                    SiteId = 0,
                    Stage = 3,
                    Payload = RHAH_EventFollowRules.Payload(definition.Id, 31, 0)
                };
                definition.Prepare(instance);
                EventChainDecision decision = definition.Evaluate(instance, 60000);
                Assert.Equal("advance_long", decision.ActionId);
                Assert.Equal(62500, decision.NextTick);
                Assert.Equal(definition.Id + "|31|0", instance.Payload);
            }
        }


        [Fact]
        public void NewLongClockSaturatesInsteadOfReturningAnExpiredTime()
        {
            Assert.Equal(int.MaxValue, RHAH_LongFollowRuntime.AddTicks(int.MaxValue - 5, 10));
            Assert.Equal(int.MaxValue, RHAH_EventFollowRules.LongDeadline("I-002", int.MaxValue - 5));
        }

        [Fact]
        public void LivingMotherMeetingKeepsItsSavedDepartureDeadline()
        {
            RHAH_LongFollowRecord record = new RHAH_LongFollowRecord
            {
                DisplayId = "I-037",
                Phase = 5,
                DeadlineTick = 50000
            };

            Assert.True(RHAH_LongMotherFollow.AcknowledgeLivingMeeting(record, null, 30000));
            Assert.True(record.ClaimAcknowledged);
            Assert.Equal(50000, record.DeadlineTick);
            Assert.False(RHAH_LongMotherFollow.AcknowledgeLivingMeeting(record, null, 55000));
            Assert.Equal(50000, record.DeadlineTick);
        }

        [Theory]
        [InlineData("I-002", 2, false)]
        [InlineData("I-037", 6, false)]
        [InlineData("I-037", 7, false)]
        [InlineData("I-037", 5, true)]
        public void MeetingNotificationDoesNotRestartOtherMotherPhases(string id, int phase, bool completed)
        {
            RHAH_LongFollowRecord record = new RHAH_LongFollowRecord
            {
                DisplayId = id,
                Phase = phase,
                Completed = completed,
                ClaimAcknowledged = true,
                DueTick = 90000
            };

            Assert.False(RHAH_LongMotherFollow.AcknowledgeLivingMeeting(record, null, 30000));
            Assert.Equal(-1, record.DeadlineTick);
            Assert.Equal(90000, record.DueTick);
        }
    }
}
