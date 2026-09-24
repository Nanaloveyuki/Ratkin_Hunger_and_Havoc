using HungerAndHavoc.Incidents;
using HungerAndHavoc.Narrative;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_EndingRulesTests
    {
        [Fact]
        public void ThreatFactor_MatchesTrustAnchors()
        {
            Assert.Equal(1.25f, RHAH_EndingRules.ThreatFactor(-100));
            Assert.Equal(1f, RHAH_EndingRules.ThreatFactor(0));
            Assert.Equal(0.75f, RHAH_EndingRules.ThreatFactor(100));
            Assert.Equal(13f / 1.25f, RHAH_EndingRules.ThreatInterval(-100), 3);
            Assert.Equal(13f, RHAH_EndingRules.ThreatInterval(0), 3);
            Assert.Equal(13f / 0.75f, RHAH_EndingRules.ThreatInterval(100), 3);
        }

        [Fact]
        public void LowEnding_DoesNotBlockLaterHighEnding()
        {
            RHAH_EndingFacts low = Facts(10, true, 10, 0, 0, 0, 6, false, 30, false, false, false, false, false, 0);
            Assert.Equal(RHAH_EndingId.E03, RHAH_EndingRules.Next(low, RHAH_EndingGoals.Defaults()));

            RHAH_EndingFacts after = Facts(80, true, 99, 3, 3, 100, 6, false, 30, false, false, true, false, false, 0);
            Assert.Equal(RHAH_EndingId.E02, RHAH_EndingRules.Next(after, RHAH_EndingGoals.Defaults()));
            Assert.Equal(RHAH_EndingId.E01, RHAH_EndingRules.Next(
                Facts(60, true, 99, 3, 1, 10, 6, false, 30, false, false, true, false, false, 0),
                RHAH_EndingGoals.Defaults()));
        }

        [Fact]
        public void Hope_RequiresAdultsAndHigherTrust()
        {
            Assert.Equal(RHAH_EndingId.E01, RHAH_EndingRules.Next(
                Facts(80, true, 99, 3, 0, 99, 6, false, 30, false, false, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
            Assert.Equal(RHAH_EndingId.E02, RHAH_EndingRules.Next(
                Facts(75, true, 99, 3, 3, 100, 6, false, 0, false, false, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
            Assert.Equal(RHAH_EndingId.E01, RHAH_EndingRules.Next(
                Facts(74, true, 99, 3, 3, 100, 0, false, 0, false, false, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
            Assert.Equal(RHAH_EndingId.None, RHAH_EndingRules.Next(
                Facts(49, true, 99, 3, 3, 100, 0, false, 0, false, false, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
        }

        [Fact]
        public void OtherNarrator_IgnoresSuiyinTrustAndIdentity()
        {
            Assert.Equal(RHAH_EndingId.E02, RHAH_EndingRules.Next(
                Facts(0, false, 99, 3, 0, 100, 6, true, 30, false, false, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
            Assert.False(RHAH_EndingRules.IdentityDue(
                Facts(90, false, 99, 3, 0, 100, 6, true, 30, true, true, false, false, false, 0),
                RHAH_EndingGoals.Defaults()));
            Assert.Equal("RHAH_Ending_Public", RHAH_EndingRuntime.TextKey(RHAH_EndingId.E02, false));
            Assert.Equal("RHAH_Ending_E03", RHAH_EndingRuntime.TextKey(RHAH_EndingId.E03, false));
        }

        [Fact]
        public void Halt_FiresOnceAndDoesNotCloseTheSchedule()
        {
            RHAH_EndingFacts halt = Facts(-75, true, 0, 0, 9, 0, 1, false, 0, false, false, false, false, false, 0);
            Assert.Equal(RHAH_EndingId.E05, RHAH_EndingRules.Next(halt, RHAH_EndingGoals.Defaults()));
            RHAH_EndingFacts again = Facts(-100, true, 0, 0, 9, 0, 1, false, 0, false, false, false, false, true, 0);
            Assert.Equal(RHAH_EndingId.None, RHAH_EndingRules.Next(again, RHAH_EndingGoals.Defaults()));
            Assert.True(RHAH_EndingRules.AsidesClosed(-80, false));
            Assert.False(RHAH_EndingRuntime.IsNarrator("Randy"));
        }

        [Fact]
        public void DisabledEnding_DoesNotStartAndShownEndingStays()
        {
            RHAH_EndingGoals closed = new RHAH_EndingGoals(99, 3, 3, 100, 30, true, false, true, true, true, true, true, true);
            Assert.Equal(RHAH_EndingId.None, RHAH_EndingRules.Next(
                Facts(80, false, 99, 3, 0, 100, 6, false, 30, false, false, false, false, false, 0),
                closed));
            Assert.Equal(RHAH_EndingId.None, RHAH_EndingRules.Next(
                Facts(10, true, 0, 0, 0, 0, 6, false, 30, false, false, false, false, false, 0),
                new RHAH_EndingGoals(99, 3, 3, 100, 30, true, true, true, true, false, true, true, true)));
        }

        [Fact]
        public void Identity_AsksOnceAndRefusalDoesNotLowerTrust()
        {
            RHAH_EndingGoals goals = RHAH_EndingGoals.Defaults();
            Assert.False(RHAH_EndingRules.IdentityDue(Facts(49, true, 0, 0, 0, 0, 0, true, 0, false, false, false, false, false, 0), goals));
            Assert.True(RHAH_EndingRules.IdentityDue(Facts(50, true, 0, 0, 0, 0, 0, true, 0, false, false, false, false, false, 0), goals));
            Assert.Equal(RHAH_IdentityTier.Partial, RHAH_EndingRules.IdentityOffer(50));
            Assert.Equal(RHAH_IdentityTier.Full, RHAH_EndingRules.IdentityOffer(75));
            Assert.False(RHAH_EndingRules.IdentityDue(Facts(80, true, 0, 0, 0, 0, 0, true, 0, false, false, false, false, false, 2), goals));
            Assert.Equal(50, Facts(50, true, 1, 0, 0, 0, 0, false, 0, false, false, false, false, false, 0).Trust);
        }

        [Fact]
        public void SaveRoundTrip_KeepsShownEndingAndCounters()
        {
            NarrativeState state = new NarrativeState(null);
            state.RecordTrust(60);
            state.NoteAid(0);
            state.NoteBroadcast(60000);
            state.NoteExpulsion(120000);
            state.NoteCompletedKind(120000, 1);
            state.NoteRelicDone(180000);
            state.SetAdultCount(12, 180000);
            state.MarkEnding(RHAH_EndingId.E03);
            state.MarkIdentity(RHAH_IdentityTier.Partial, true);

            NarrativeState loaded = new NarrativeState(null);
            loaded.RecordTrust(state.Snapshot().Trust);
            for (int i = 0; i < state.AidCount; i++)
            {
                loaded.NoteAid(0);
            }

            for (int i = 0; i < state.BroadcastCount; i++)
            {
                loaded.NoteBroadcast(60000);
            }

            for (int i = 0; i < state.ExpulsionCount; i++)
            {
                loaded.NoteExpulsion(120000);
            }

            for (int journal = 1; journal <= state.CompletedKindCount; journal++)
            {
                loaded.NoteCompletedKind(120000, journal);
            }

            if (state.RelicDone)
            {
                loaded.NoteRelicDone(180000);
            }

            loaded.SetAdultCount(state.AdultCount, 180000);
            loaded.MarkEnding(RHAH_EndingId.E03);
            loaded.MarkIdentity(RHAH_IdentityTier.Partial, true);

            Assert.Equal(1, loaded.AidCount);
            Assert.Equal(1, loaded.BroadcastCount);
            Assert.Equal(12, loaded.AdultCount);
            Assert.True(loaded.EndingShown(RHAH_EndingId.E03));
            Assert.False(loaded.IdentityDue(200000, true, RHAH_EndingGoals.Defaults()));
            Assert.Equal(RHAH_EndingId.E01, loaded.PendingEnding(200000, true, new RHAH_EndingGoals(1, 1, 3, 100, 30, true, true, true, true, true, true, true, true)));
        }

        static RHAH_EndingFacts Facts(
            int trust,
            bool narrator,
            int aid,
            int broadcasts,
            int expulsions,
            int adults,
            int kinds,
            bool relic,
            int waited,
            bool e01,
            bool e02,
            bool e03,
            bool e04,
            bool e05,
            int identity)
        {
            return new RHAH_EndingFacts(trust, narrator, aid, broadcasts, expulsions, adults, kinds, relic, waited, e01, e02, e03, e04, e05, identity);
        }

        [Fact]
        public void CompletedAidAndForcedExpulsionCountOnce()
        {
            Assert.Equal(RHAH_EndingRules.RHAH_EndingEvent.Aid, RHAH_EndingRules.FromChoice(RHAH_ChoiceKind.Aid, RHAH_ChoiceAction.Deliver));
            Assert.Equal(RHAH_EndingRules.RHAH_EndingEvent.Aid, RHAH_EndingRules.FromChoice(RHAH_ChoiceKind.ChildExchange, RHAH_ChoiceAction.Deliver));
            Assert.Equal(RHAH_EndingRules.RHAH_EndingEvent.None, RHAH_EndingRules.FromChoice(RHAH_ChoiceKind.Aid, RHAH_ChoiceAction.Reject));
            Assert.Equal(RHAH_EndingRules.RHAH_EndingEvent.None, RHAH_EndingRules.FromChoice(RHAH_ChoiceKind.Intel, RHAH_ChoiceAction.Deliver));
            Assert.True(RHAH_EndingRules.CountsExpulsion(true, true));
            Assert.False(RHAH_EndingRules.CountsExpulsion(false, true));
        }
    }
}
