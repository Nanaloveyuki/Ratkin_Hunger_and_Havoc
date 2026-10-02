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
        public void PendingVisitorsBecomeMissingOnlyAfterTheGracePeriod()
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case record = Open(state, SuiyinN007Outcome.Pending, true);
            record.Visitors.Add(Member(41));
            Assert.False(RHAH_Quarantine.Refresh(state, record, 10));
            Assert.Equal(SuiyinPresence.Unknown, record.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.Pending, record.Outcome);
            int end = 10 + state.Book.Config.MissingDays * 60000;
            Assert.False(RHAH_Quarantine.Refresh(state, record, end - 1));
            Assert.Equal(SuiyinN007Outcome.Pending, record.Outcome);
            Assert.False(RHAH_Quarantine.Refresh(state, record, end));
            Assert.Equal(SuiyinPresence.Missing, record.Visitors[0].Presence);
            Assert.Equal(SuiyinN007Outcome.Missing, record.Outcome);
        }

        [Theory]
        [InlineData((int)SuiyinN007Outcome.Quarantine)]
        [InlineData((int)SuiyinN007Outcome.Release)]
        public void ChosenQuarantineClosesWhenEveryConfirmedMemberDied(int value)
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case record = Open(state, (SuiyinN007Outcome)value, false);
            record.Visitors.Add(new SuiyinMember { LoadId = 51, Presence = SuiyinPresence.Dead });
            record.Visitors.Add(new SuiyinMember { LoadId = 52, Presence = SuiyinPresence.Dead });
            Assert.False(RHAH_Quarantine.Refresh(state, record, 100));
            Assert.Equal(SuiyinN007Outcome.AllDead, record.Outcome);
        }



        [Fact]
        public void DeferredRecordsDoNotStartASecondAutomaticSettlement()
        {
            NarrativeState state = new NarrativeState(null);
            SuiyinN007Case deferred = Open(state, SuiyinN007Outcome.Defer, false);
            deferred.Visitors.Add(Member(44));
            RHAH_Quarantine.Tick(state, GenDate.TicksPerHour);
            Assert.Equal(SuiyinPresence.Here, deferred.Visitors[0].Presence);
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
