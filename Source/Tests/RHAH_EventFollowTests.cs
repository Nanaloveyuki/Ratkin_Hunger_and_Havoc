using RimWorld;
using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.EventMgr;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_EventFollowTests
    {
        [Fact]
        public void ShortChainRollsAPredatorAndLongChainDoesNot()
        {
            Assert.True(RHAH_EventFollowRules.UsesShortPredator("I-001"));
            Assert.True(RHAH_EventFollowRules.RollsPredator(false, false, 10));
            Assert.False(RHAH_EventFollowRules.RollsPredator(true, false, 10));
            Assert.False(RHAH_EventFollowRules.RollsPredator(false, true, 10));
            Assert.False(RHAH_EventFollowRules.RollsPredator(false, false, 0));
            Assert.True(RHAH_EventFollowRules.PredatorSelected(10, 0.09f));
            Assert.False(RHAH_EventFollowRules.PredatorSelected(10, 0.1f));
            Assert.Equal(0, RHAH_EventFollowRules.Clamp(-4));
            Assert.Equal(100, RHAH_EventFollowRules.Clamp(140));
        }

        [Fact]
        public void EventClosesWhenTheLastYoungOutcomeFinishes()
        {
            Assert.False(RHAH_EventFollowRules.BatchOutcomeDone(1, 1));
            Assert.True(RHAH_EventFollowRules.BatchOutcomeDone(0, 2));
            Assert.False(RHAH_EventFollowRules.BatchOutcomeDone(0, 0));
        }

        [Fact]
        public void SavedIncidentRecordsReachTheFollowHandlerOnce()
        {
            RHAH_EventChains.Clear();
            RHAH_EventChains.Register(new RHAH_EventFollowChain());
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 4;
            string payload = RHAH_EventFollowRules.Payload("I-002", 9, 4);
            int id = RHAH_EventChainRuntime.Start(records, ref next, "I-002", 4, 10, -1, payload);
            Assert.True(RHAH_EventChainRuntime.SetStage(records, id, RHAH_EventFollowChain.StageShort, -1, payload));

            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, 11));
            Assert.Equal(RHAH_EventFollowChain.StageDone, records[0].stage);
            Assert.False(records[0].closed);
            Assert.Equal("I-002", records[0].displayId);

            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, 12));
            Assert.Equal(RHAH_EventFollowChain.StageDone, records[0].stage);
            Assert.False(records[0].closed);
            RHAH_EventChains.Clear();
        }

        [Fact]
        public void LoadedLongChainStaysOpenAtStageOneAcrossHours()
        {
            RHAH_EventChains.Clear();
            RHAH_EventChains.Register(new RHAH_EventFollowChain());
            List<RHAH_EventChainRecord> records = new List<RHAH_EventChainRecord>();
            int next = 1;
            string payload = RHAH_EventFollowRules.Payload("I-019", 9, 4);
            int deadline = RHAH_EventFollowRules.LongDeadline("I-019", 0);
            int id = RHAH_EventChainRuntime.Start(records, ref next, "I-019", 4, 0, deadline, payload);
            Assert.True(RHAH_EventChainRuntime.SetStage(records, id, RHAH_EventFollowChain.StageLong, deadline, payload));
            RHAH_EventChainRuntime.Repair(records, ref next);

            int hour = GenDate.TicksPerHour;
            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, hour - 1));
            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, hour));
            Assert.Equal(0, RHAH_EventChainRuntime.TickDue(records, hour + hour));
            Assert.Equal(RHAH_EventFollowChain.StageLong, records[0].stage);
            Assert.False(records[0].closed);
            Assert.Equal("I-019", records[0].displayId);
            RHAH_EventChains.Clear();
        }

        [Fact]
        public void OnlyDocumentedIncidentsOpenALongFollow()
        {
            Assert.True(RHAH_EventFollowRules.OpensLong("I-002", true));
            Assert.True(RHAH_EventFollowRules.OpensLong("I-041", true));
            Assert.False(RHAH_EventFollowRules.OpensLong("I-002", false));
            Assert.False(RHAH_EventFollowRules.OpensLong("I-001", true));
            Assert.False(RHAH_EventFollowRules.OpensLong("I-051", true));
            Assert.Equal(10 + 15 * RHAH_EventFollowRules.TicksPerDay, RHAH_EventFollowRules.LongDeadline("I-037", 10));
            Assert.Equal(10 + 120 * RHAH_EventFollowRules.TicksPerDay, RHAH_EventFollowRules.LongDeadline("I-002", 10));
            Assert.Equal(-1, RHAH_EventFollowRules.LongDeadline("I-001", 10));
        }

        [Fact]
        public void PayloadRoundTripsBatchAndMap()
        {
            Assert.True(RHAH_EventFollowRules.ReadPayload(RHAH_EventFollowRules.Payload("I-012", 9, 4), out string id, out int batch, out int map));
            Assert.Equal("I-012", id);
            Assert.Equal(9, batch);
            Assert.Equal(4, map);
            Assert.False(RHAH_EventFollowRules.ReadPayload("I-012|0|4", out id, out batch, out map));
            Assert.False(RHAH_EventFollowRules.ReadPayload("", out id, out batch, out map));
        }

        [Fact]
        public void FourteenUsesTraitsBeforeTheCoinAndSkipsTheWrongPeople()
        {
            RHAH_FollowOutcome kind = RHAH_EventFollowRules.AtFourteen(Subject("I-002", 14f, true, kind: true), false, false, 0.9f);
            Assert.Equal(RHAH_EventFollowRules.MotherGoneBad, kind.Thought);
            Assert.Equal(RHAH_FollowMood.Permanent, kind.Duration);
            Assert.Equal(RHAH_FollowTrait.Kind, kind.Trait);

            RHAH_FollowOutcome twisted = RHAH_EventFollowRules.AtFourteen(Subject("I-003", 14f, true, twisted: true), false, false, 0.1f);
            Assert.Equal(RHAH_EventFollowRules.AteMotherFine, twisted.Thought);
            Assert.Equal(RHAH_FollowTrait.Cannibal, twisted.Trait);

            RHAH_FollowOutcome coin = RHAH_EventFollowRules.AtFourteen(Subject("I-002", 14f, true), false, false, 0.8f);
            Assert.Equal(RHAH_EventFollowRules.MotherGoneGood, coin.Thought);
            Assert.Equal(RHAH_FollowMood.Temporary, coin.Duration);
            Assert.False(RHAH_EventFollowRules.AtFourteen(Subject("I-002", 13.9f, true), false, false, 0f).Applies);
            Assert.False(RHAH_EventFollowRules.AtFourteen(Subject("I-002", 14f, true), true, true, 0f).Applies);
            Assert.False(RHAH_EventFollowRules.AtFourteen(Subject("I-013", 14f, true, gaveChild: true, food: true), false, false, 0f).Applies);
            Assert.False(RHAH_EventFollowRules.AtFourteen(Subject("I-007", 14f, true), false, false, 0f).Applies);
        }

        [Fact]
        public void ReliefBirthAndHarmKeepTheirGates()
        {
            Assert.Equal(RHAH_EventFollowRules.AidAgain, RHAH_EventFollowRules.OnRelief("I-015", true, true, false, false, 0.1f).Thought);
            Assert.False(RHAH_EventFollowRules.OnRelief("I-015", false, true, false, false, 0.1f).Applies);
            Assert.False(RHAH_EventFollowRules.OnRelief("I-017", true, true, false, false, 0.1f).Applies);
            Assert.False(RHAH_EventFollowRules.OnRelief("I-018", true, true, false, false, 0.1f).Applies);
            Assert.Equal(RHAH_EventFollowRules.SilverHard, RHAH_EventFollowRules.OnRelief("I-018", true, true, false, true, 0.9f).Thought);
            Assert.Equal(RHAH_EventFollowRules.BornAlive, RHAH_EventFollowRules.OnBirth("I-029", true, true, false, 3, true, false, 0.1f).Thought);
            Assert.False(RHAH_EventFollowRules.OnBirth("I-029", true, true, false, 2, false, false, 0.1f).Applies);
            Assert.Equal(RHAH_EventFollowRules.BornSick, RHAH_EventFollowRules.OnBirth("I-044", true, true, true, 1, false, false, 0.9f).Thought);
            Assert.Equal(RHAH_EventFollowRules.TraderSilent, RHAH_EventFollowRules.OnTraderPlague(true, true).Thought);
            Assert.False(RHAH_EventFollowRules.OnTraderPlague(true, false).Applies);
            Assert.False(RHAH_EventFollowRules.OnJoinedAfterHarm(Subject("I-006", 20f, true), 0f).Applies);
            Assert.True(RHAH_EventFollowRules.OnJoinedAfterHarm(Subject("I-006", 20f, true, joined: true, hurt: true), 0.2f).Applies);
        }

        [Fact]
        public void FamilyLinksOnlyTheMotherEvents()
        {
            Assert.True(RHAH_EventFollowRules.LinksFamily("I-004", 0, RHAH_PawnRole.BeggarMother));
            Assert.True(RHAH_EventFollowRules.LinksFamily("I-004", 2, RHAH_PawnRole.BeggarChild));
            Assert.False(RHAH_EventFollowRules.LinksFamily("I-003", 0, RHAH_PawnRole.RatkinYoung));
            Assert.False(RHAH_EventFollowRules.LinksFamily("I-002", 1, RHAH_PawnRole.BeggarChild));
        }

        static RHAH_FollowSubject Subject(
            string id,
            float age,
            bool alive,
            bool kind = false,
            bool twisted = false,
            bool joined = false,
            bool hurt = false,
            bool gaveChild = false,
            bool food = false)
        {
            return new RHAH_FollowSubject(id, RHAH_PawnRole.BeggarChild, 1, 2, age, alive, true, joined, hurt, false, false, kind, twisted, false, true, true, gaveChild, food, false, false, false);
        }
    }
}
