using Verse;
using System.Collections.Generic;
using HungerAndHavoc.Narrative;
using HungerAndHavoc.Storyteller.Suiyin;
using RimWorld;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_NarrativeWatchTests : System.IDisposable
    {
        readonly Game previousGame = Current.Game;
        readonly ProgramState previousState = Current.ProgramState;
        public RHAH_NarrativeWatchTests()
        {
            Current.ProgramState = ProgramState.Entry;
            Current.Game = (Game)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Game));
            RHAH_PawnIndex.Clear();
        }

        public void Dispose()
        {
            Current.Game = previousGame;
            Current.ProgramState = previousState;
            RHAH_PawnIndex.Clear();
        }

        [Fact]
        public void PendingChoiceNoticesImmediatelyWithoutChangingVisitors()
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case record = Open(state, SuiyinN007Outcome.Pending, true);
            record.Visitors.Add(Member(41));

            RHAH_Quarantine.Tick(state, 1);
            RHAH_Quarantine.Tick(state, 2);

            Assert.False(record.ChoiceOpen);
            Assert.Equal(SuiyinPresence.Here, record.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.Pending, record.Outcome);
        }

        [Fact]
        public void ActiveWatchObservesOnlyOnTheHour()
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case record = Open(state, SuiyinN007Outcome.Quarantine, false);
            record.Visitors.Add(Member(41));

            RHAH_Quarantine.Tick(state, GenDate.TicksPerHour - 1);
            Assert.Equal(SuiyinPresence.Here, record.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.Quarantine, record.Outcome);

            RHAH_Quarantine.Tick(state, GenDate.TicksPerHour);
            Assert.Equal(SuiyinPresence.Unknown, record.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.Quarantine, record.Outcome);

            record.Visitors[0].Presence = SuiyinPresence.Here;
            RHAH_Quarantine.Tick(state, GenDate.TicksPerHour + 1);
            Assert.Equal(SuiyinPresence.Here, record.Visitors[0].Presence);
        }

        [Fact]
        public void TerminalAndDeferredRecordsDoNotObserve()
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case recovered = Open(state, SuiyinN007Outcome.RecoveredLeft, false);
            recovered.Visitors.Add(Member(41));
            SuiyinN007Case broken = Open(state, SuiyinN007Outcome.Broken, false);
            broken.Visitors.Add(Member(42));
            SuiyinN007Case dead = Open(state, SuiyinN007Outcome.AllDead, false);
            dead.Visitors.Add(Member(43));
            SuiyinN007Case deferred = Open(state, SuiyinN007Outcome.Defer, false);
            deferred.Visitors.Add(Member(44));

            RHAH_Quarantine.Tick(state, GenDate.TicksPerHour);

            Assert.Equal(SuiyinPresence.Here, recovered.Visitors[0].Presence);
            Assert.Equal(SuiyinPresence.Here, broken.Visitors[0].Presence);
            Assert.Equal(SuiyinPresence.Here, dead.Visitors[0].Presence);
            Assert.Equal(SuiyinPresence.Here, deferred.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.RecoveredLeft, recovered.Outcome);
            Assert.Equal(SuiyinN007Outcome.Broken, broken.Outcome);
            Assert.Equal(SuiyinN007Outcome.AllDead, dead.Outcome);
            Assert.Equal(SuiyinN007Outcome.Defer, deferred.Outcome);
        }

        [Fact]
        public void UnvisitedSiteWaitsForItsDeadlineBeforeRemoval()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.True(book.OpenRelic(0));
            book.N009.SiteId = 32;
            book.N009.MapPresent = true;
            book.N009.MapEntered = false;
            book.N009.PlayersInside = false;
            int before = book.N009.Deadline - 1;

            Assert.False(book.ExpireRelic(before));
            Assert.Equal(SuiyinN009Outcome.Pending, book.N009.Outcome);
            Assert.False(RHAH_RecordSite.ShouldRemove(book.N009.Outcome, book.N009.BoxDestroyed, false, false, book.N009.MapEntered));

            Assert.True(book.ExpireRelic(book.N009.Deadline));
            Assert.Equal(SuiyinN009Outcome.Empty, book.N009.Outcome);
            Assert.True(RHAH_RecordSite.ShouldRemove(book.N009.Outcome, book.N009.BoxDestroyed, false, false, book.N009.MapEntered));
            Assert.False(RHAH_RecordSite.ShouldRemove(book.N009.Outcome, book.N009.BoxDestroyed, true, false, book.N009.MapEntered));
            Assert.False(RHAH_RecordSite.ShouldRemove(book.N009.Outcome, book.N009.BoxDestroyed, false, true, book.N009.MapEntered));
        }

        [Fact]
        public void EnteredOrDestroyedSiteIsNotAnAbandonedRemoval()
        {
            Assert.False(RHAH_RecordSite.ShouldRemove(SuiyinN009Outcome.Empty, false, false, false, true));
            Assert.False(RHAH_RecordSite.ShouldRemove(SuiyinN009Outcome.Destroyed, true, false, false, true));
            Assert.False(RHAH_RecordSite.ShouldRemove(SuiyinN009Outcome.Empty, true, false, false, false));
        }

        static SuiyinN007Case Open(NarrativeState state, SuiyinN007Outcome outcome, bool choiceOpen)
        {
            SuiyinN007Case record = new SuiyinN007Case
            {
                MapId = 2,
                Outcome = outcome,
                ChoiceOpen = choiceOpen,
                Visitors = new List<SuiyinMember>()
            };
            state.Book.N007.Add(record);
            return record;
        }

        static SuiyinMember Member(int id)
        {
            return new SuiyinMember { LoadId = id, Presence = SuiyinPresence.Here, Care = SuiyinCare.Plague };
        }

    }
}
