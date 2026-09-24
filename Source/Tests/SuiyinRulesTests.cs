using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HungerAndHavoc.Narrative;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class SuiyinRulesTests
    {
        [Fact]
        public void DistinctKindsOpenLettersOnceAndPaySilverOnce()
        {
            SuiyinBook book = new SuiyinBook();
            book.Note("I-001", 1, 0, false, true);
            book.Note("I-001", 1, 10, false, true);
            Assert.Equal(1, book.Distinct);
            Assert.True(book.OpeningSent);
            Assert.False(book.ProgressSent);
            Assert.Equal(1, Count(book, SuiyinLetter.N001));

            book.TakeNotices(new List<SuiyinNotice>());
            book.Note("I-006", 1, 20, false, true);
            book.Note("I-013", 1, 30, false, true);
            Assert.True(book.ProgressSent);
            Assert.Equal(3, Arg(book, SuiyinLetter.N002));
            book.Note("I-013", 1, 40, false, true);
            Assert.Equal(1, Count(book, SuiyinLetter.N002));

            book.TakeNotices(new List<SuiyinNotice>());
            for (int i = 8; i <= 12; i++)
            {
                book.Note("I-" + i.ToString("000"), 1, i, false, true);
            }

            Assert.True(book.RewardClaimed);
            Assert.Equal(300, book.RewardPaid);
            Assert.Equal(300, Arg(book, SuiyinLetter.N003));
            book.TakeNotices(new List<SuiyinNotice>());
            book.Note("I-015", 1, 90, false, true);
            Assert.Equal(0, Count(book, SuiyinLetter.N003));
            Assert.Equal(300, book.RewardPaid);
        }

        [Fact]
        public void ChoiceTrustReturnsToTheNarrativeState()
        {
            NarrativeState state = new NarrativeState(null);
            state.RecordTrust(4);
            SuiyinN004Case record = new SuiyinN004Case();
            state.Book.N004.Add(record);
            record.Children.Add(new SuiyinMember { LoadId = 8, Child = false, Presence = SuiyinPresence.Here });
            Assert.Equal(SuiyinN004Outcome.FamilyHere, state.Commit(book => book.ResolveEntrust(record, 10, SuiyinPresence.Here)));
            Assert.Equal(9, state.Snapshot().Trust);
            Assert.Equal(9, state.Book.Trust);
        }

        [Fact]
        public void PlagueVisitorsOpenOneChoiceAndKeepTheirIds()
        {
            SuiyinBook book = new SuiyinBook();
            book.Note("I-042", 2, 10, true, true, 900, new[] { 41, 0, 41, 42 });
            Assert.Equal(1, book.N007.Count);
            Assert.True(book.N007[0].ChoiceOpen);
            Assert.Equal(2, book.N007[0].Visitors.Count);
            Assert.Equal(41, book.N007[0].Visitors[0].LoadId);
            Assert.Equal(42, book.N007[0].Visitors[1].LoadId);
            Assert.Equal(0, Count(book, SuiyinLetter.N007Opened));
            book.QueueOpened(2);
            Assert.Equal(2, Arg(book, SuiyinLetter.N007Opened));
            book.Note("I-042", 2, 20, true, true, 901, new[] { 43 });
            Assert.Equal(1, book.N007.Count);
        }

        [Fact]
        public void RewardDueMatchesTheLedgerOnce()
        {
            NarrativeState state = new NarrativeState(null);
            for (int i = 1; i <= 8; i++)
            {
                state.NoteIncident(new SuiyinIncidentFact("I-" + i.ToString("000"), 1, i * 10, i, 1, false, null));
            }

            Assert.Equal(300, state.Book.RewardPaid);
            Assert.Equal(300, state.TakeRewardDue());
            Assert.Equal(0, state.TakeRewardDue());
            state.NoteIncident(new SuiyinIncidentFact("I-009", 1, 90, 9, 1, false, null));
            Assert.Equal(0, state.TakeRewardDue());
        }

        [Fact]
        public void CompletedJournalKindsCountOnce()
        {
            NarrativeState state = new NarrativeState(null);
            Assert.True(state.NoteCompletedKind(10, 14));
            Assert.False(state.NoteCompletedKind(20, 14));
            Assert.True(state.NoteCompletedKind(30, 5));
            Assert.False(state.NoteCompletedKind(40, 0));
            Assert.Equal(2, state.CompletedKindCount);
            Assert.Equal(2, state.EndingFacts(60000, true).CompletedKinds);
        }


        [Fact]
        public void DisabledNodesDoNotSendPrivateLetters()
        {
            SuiyinBook book = new SuiyinBook();
            book.Gates = new SuiyinLedger();
            book.Gates.SetEnabled(SuiyinNode.N001, false);
            book.Gates.SetEnabled(SuiyinNode.N002, false);
            book.Note("I-001", 1, 0, false, true);
            book.Note("I-006", 1, 1, false, true);
            book.Note("I-008", 1, 2, false, true);
            Assert.False(book.OpeningSent);
            Assert.False(book.ProgressSent);
            Assert.Equal(0, Count(book, SuiyinLetter.N001));
        }

        [Fact]
        public void OtherNarratorsSkipPrivateLettersButKeepObjectiveOnes()
        {
            SuiyinBook book = new SuiyinBook();
            book.Narrator = false;
            book.Note("I-001", 1, 0, false, true);
            Assert.True(book.OpeningSent);
            Assert.Equal(0, Count(book, SuiyinLetter.N001));
            book.Note("I-036", 1, 1, true, true);
            Assert.Equal(1, book.N007.Count);
            Assert.True(book.N007[0].ChoiceOpen);
        }

        [Fact]
        public void EntrustOutcomesStayExclusive()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.True(book.OpenEntrust(7, 1, 0, new[] { 8 }));
            Assert.False(book.OpenEntrust(7, 1, 1, new[] { 8 }));
            SuiyinN004Case record = book.N004[0];
            record.Children[0].Child = false;
            Assert.Equal(SuiyinN004Outcome.FamilyHere, book.ResolveEntrust(record, 10, SuiyinPresence.Here));
            record.Children[0].Presence = SuiyinPresence.Left;
            Assert.Equal(SuiyinN004Outcome.FamilyHere, book.ResolveEntrust(record, 20, SuiyinPresence.Left));
            Assert.Equal(5, book.Trust);
            Assert.Equal(1, Count(book, SuiyinLetter.N004FamilyHere));

            SuiyinBook dead = new SuiyinBook();
            dead.OpenEntrust(1, 1, 0, new[] { 2 });
            dead.N004[0].Children[0].Presence = SuiyinPresence.Dead;
            Assert.Equal(SuiyinN004Outcome.Story, dead.ResolveEntrust(dead.N004[0], 10, SuiyinPresence.Dead));
            Assert.Equal(-2, dead.Trust);

            SuiyinBook captive = new SuiyinBook();
            captive.OpenEntrust(1, 1, 0, new[] { 2 });
            captive.N004[0].Children[0].Child = false;
            captive.N004[0].Children[0].Care = SuiyinCare.Captive;
            Assert.Equal(SuiyinN004Outcome.Captive, captive.ResolveEntrust(captive.N004[0], 10, SuiyinPresence.Dead));

            SuiyinBook alone = new SuiyinBook();
            alone.OpenEntrust(1, 1, 0, new[] { 2 });
            alone.N004[0].Children[0].Child = false;
            Assert.Equal(SuiyinN004Outcome.ChildAlone, alone.ResolveEntrust(alone.N004[0], 10, SuiyinPresence.Dead));

            SuiyinBook regret = new SuiyinBook();
            regret.OpenEntrust(1, 1, 0, new[] { 2 });
            regret.N004[0].Children[0].Presence = SuiyinPresence.Dead;
            Assert.Equal(SuiyinN004Outcome.Regret, regret.ResolveEntrust(regret.N004[0], 10, SuiyinPresence.Here));
        }

        [Fact]
        public void BanishedRescuePaysOnceAfterFourYears()
        {
            SuiyinBook book = new SuiyinBook();
            book.OpenEntrust(1, 1, 0, new[] { 2 });
            SuiyinN004Case record = book.N004[0];
            record.Children[0].Presence = SuiyinPresence.Left;
            Assert.Equal(SuiyinN004Outcome.Banished, book.ResolveEntrust(record, 100, SuiyinPresence.Left));
            record.RevisitSeen = true;
            Assert.False(book.ChooseRevisit(record, SuiyinN004Revisit.Rescue, 200, false));
            Assert.True(book.ChooseRevisit(record, SuiyinN004Revisit.Rescue, 200, true));
            Assert.False(book.ChooseRevisit(record, SuiyinN004Revisit.Kill, 300, true));
            Assert.Equal(0, book.ClaimRescue(record, record.RescueDueTick - 1));
            Assert.Equal(2500, book.ClaimRescue(record, record.RescueDueTick));
            Assert.Equal(0, book.ClaimRescue(record, record.RescueDueTick + 10));
        }

        [Fact]
        public void CarePausesWhileHungryOrSickAndMissingIsNotDeath()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.True(book.AcceptExchange(4, 1, 0, new[] { 9 }));
            Assert.False(book.AcceptExchange(4, 1, 1, new[] { 9 }));
            SuiyinN005Case record = book.N005[0];
            record.Children[0].Care = SuiyinCare.Hungry;
            book.AdvanceCare(record, 1000, 60000 * 5);
            Assert.Equal(SuiyinN005Outcome.Accepted, record.Outcome);
            Assert.Equal(0, record.Children[0].CareTicks);

            record.Children[0].Care = SuiyinCare.Free;
            Assert.Equal(SuiyinN005Outcome.Cared, book.AdvanceCare(record, 2000, 60000 * 5));
            Assert.Equal(3, book.Trust);
            book.AdvanceCare(record, 3000, 60000);
            Assert.Equal(1, Count(book, SuiyinLetter.N005Cared));

            SuiyinBook missing = new SuiyinBook();
            missing.AcceptExchange(5, 1, 0, new[] { 9 });
            missing.N005[0].Children[0].Presence = SuiyinPresence.Unknown;
            Assert.Equal(SuiyinN005Outcome.Accepted, missing.AdvanceCare(missing.N005[0], 100, 0));
            Assert.Equal(SuiyinN005Outcome.Missing, missing.AdvanceCare(missing.N005[0], 100 + 60000, 0));
            Assert.NotEqual(SuiyinN005Outcome.Dead, missing.N005[0].Outcome);
        }

        [Fact]
        public void SecondTheftOpensOneHoleAndSealNeedsWood()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.False(book.NoteTheft(3, 0));
            Assert.True(book.NoteTheft(3, 10));
            Assert.False(book.NoteTheft(3, 20));
            SuiyinN006Case hole = book.N006[0];
            Assert.False(book.PlaceHole(hole));
            hole.FoodPresent = true;
            Assert.True(book.PlaceHole(hole));
            Assert.False(book.ChooseHole(hole, SuiyinN006Action.Seal, 30));
            hole.Wood = 20;
            Assert.True(book.ChooseHole(hole, SuiyinN006Action.Seal, 30));
            Assert.Equal(0, hole.Wood);
            Assert.False(hole.Hole);
            Assert.False(book.ChooseHole(hole, SuiyinN006Action.Clean, 40));
        }

        [Fact]
        public void QuarantineReturnUsesTheSamePawnOnce()
        {
            SuiyinBook book = new SuiyinBook();
            book.Note("I-042", 2, 0, true, true);
            book.Note("I-042", 2, 10, true, true);
            Assert.Equal(1, book.N007.Count);
            SuiyinN007Case record = book.N007[0];
            Assert.True(book.ChooseQuarantine(record, SuiyinN007Action.Quarantine));
            Assert.True(book.CloseQuarantine(record, SuiyinN007Outcome.RecoveredLeft, 100, 44));
            Assert.False(book.CloseQuarantine(record, SuiyinN007Outcome.RecoveredLeft, 200, 44));
            Assert.False(book.ClaimReturn(record, record.ReturnDueTick - 1, true));
            Assert.True(book.ClaimReturn(record, record.ReturnDueTick, true));
            Assert.Equal(44, Arg(book, SuiyinLetter.N007Return));
            Assert.False(book.ClaimReturn(record, record.ReturnDueTick + 1, true));
        }

        [Fact]
        public void PositiveTrustRaisesNarrativeSilverByAtMostAQuarter()
        {
            SuiyinBook book = new SuiyinBook();
            book.Trust = 100;
            Assert.Equal(375, book.Scaled(300));
            book.Trust = 0;
            Assert.Equal(300, book.Scaled(300));
            book.Trust = -40;
            Assert.Equal(300, book.Scaled(300));
            book.OpenRelic(0);
            book.Trust = 100;
            Assert.Equal(250, book.ChooseRelic(SuiyinN009Action.Take, 10));
            Assert.Equal(0, book.ChooseRelic(SuiyinN009Action.Take, 20));
        }

        [Fact]
        public void AsidesFireOnceAndStopWhenTrustIsClosed()
        {
            SuiyinBook book = new SuiyinBook();
            book.OpenEntrust(1, 1, 0, new[] { 2 });
            book.N004[0].Children[0].Child = false;
            book.ResolveEntrust(book.N004[0], 10, SuiyinPresence.Here);
            book.AcceptExchange(3, 1, 20, new[] { 4 });
            Assert.Equal(1, Count(book, SuiyinLetter.Aside));
            book.TakeNotices(new List<SuiyinNotice>());
            book.Trust = -75;
            book.TryAsideForTest(20 + 60000 * 4);
            Assert.Equal(0, Count(book, SuiyinLetter.Aside));
        }

        [Fact]
        public void JournalCountsAidOnceAfterSafeLeave()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.Equal(14, SuiyinBook.JournalFor("I-015"));
            Assert.Equal(5, SuiyinBook.JournalFor("I-006"));
            Assert.True(book.OpenJournal(14, 1, 9, 0, new[] { 41 }, true));
            Assert.False(book.OpenJournal(14, 1, 9, 1, new[] { 41 }, true));
            SuiyinJournalCase record = book.Journals[0];
            Assert.False(book.CloseJournal(record, 1000));
            record.People[0].Presence = SuiyinPresence.Left;
            Assert.True(book.CloseJournal(record, 2000));
            Assert.True(record.Counted);
            Assert.False(book.CloseJournal(record, 3000));
        }

        [Fact]
        public void QueuedLettersUseExistingKeys()
        {
            Assert.Equal("RHAH_Suiyin_N001", SuiyinBook.LetterKey(SuiyinLetter.N001, 0));
            Assert.Equal("RHAH_Suiyin_N009Empty", SuiyinBook.LetterKey(SuiyinLetter.N009Empty, 0));
            Assert.Equal("RHAH_Suiyin_Aside_1", SuiyinBook.LetterKey(SuiyinLetter.Aside, 1));
            Assert.Null(SuiyinBook.LetterKey(SuiyinLetter.Aside, 0));
            Assert.Null(SuiyinBook.LetterKey(SuiyinLetter.None, 0));
            Assert.Null(SuiyinBook.LetterKey(SuiyinLetter.N007Opened, 2));

            HashSet<string> keys = Keyed("Languages/English/Keyed/RHAH_Suiyin.xml");
            HashSet<string> chinese = Keyed("Languages/ChineseSimplified/Keyed/RHAH_Suiyin.xml");
            foreach (SuiyinLetter letter in Enum.GetValues(typeof(SuiyinLetter)))
            {
                string key = letter == SuiyinLetter.Aside
                    ? SuiyinBook.LetterKey(letter, 1)
                    : SuiyinBook.LetterKey(letter, 0);
                if (key == null)
                {
                    continue;
                }

                Assert.Contains(key + "_Label", keys);
                Assert.Contains(key + "_Text", keys);
                Assert.Contains(key + "_Label", chinese);
                Assert.Contains(key + "_Text", chinese);
            }
        }

        static HashSet<string> Keyed(string relative)
        {
            string path = Path.Combine(FindRoot(), relative);
            XmlDocument document = new XmlDocument();
            document.Load(path);
            HashSet<string> keys = new HashSet<string>();
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    keys.Add(node.Name);
                }
            }

            return keys;
        }

        static string FindRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Languages/English/Keyed/RHAH_Suiyin.xml")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return ".";
        }
        [Fact]
        public void SavedClaimsDoNotReset()
        {
            SuiyinBook book = new SuiyinBook();
            book.Note("I-001", 1, 0, false, true);
            book.RewardClaimed = true;
            book.RewardPaid = 300;
            book.N009 = new SuiyinN009Case { StartedTick = 10, Deadline = 900000, Outcome = SuiyinN009Outcome.Taken };
            SuiyinBook loaded = Copy(book);
            loaded.Note("I-002", 1, 50, false, true);
            Assert.True(loaded.RewardClaimed);
            Assert.Equal(300, loaded.RewardPaid);
            Assert.Equal(900000, loaded.N009.Deadline);
            Assert.Equal(SuiyinN009Outcome.Taken, loaded.N009.Outcome);
        }

        [Fact]
        public void ProofStillAllowsOneTrade()
        {
            SuiyinBook book = new SuiyinBook();
            for (int i = 1; i <= 5; i++)
            {
                book.Note("I-" + i.ToString("000"), 1, i, false, true);
            }

            Assert.True(book.ArriveEnvoy(4, 9, 10));
            SuiyinN008Case envoy = book.N008[0];
            envoy.ProofAvailable = true;
            Assert.True(book.ChooseEnvoy(envoy, SuiyinN008Action.Proof, 20));
            Assert.Equal(SuiyinN008Outcome.Checking, envoy.Outcome);
            envoy.MealsReady = false;
            Assert.False(book.ChooseEnvoy(envoy, SuiyinN008Action.Trade, 30));
            envoy.MealsReady = true;
            Assert.True(book.ChooseEnvoy(envoy, SuiyinN008Action.Trade, 40));
            Assert.Equal(SuiyinN008Outcome.Traded, envoy.Outcome);
            Assert.True(book.RelicClue);
        }

        [Fact]
        public void EnvoyLeavesAfterRefusalTimeoutAndAFailedCheck()
        {
            Assert.Equal(RHAH_EnvoyHold.Stay, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.Waiting));
            Assert.Equal(RHAH_EnvoyHold.Stay, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.Checking));
            Assert.Equal(RHAH_EnvoyHold.Leave, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.Refused));
            Assert.Equal(RHAH_EnvoyHold.Leave, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.Driven));
            Assert.Equal(RHAH_EnvoyHold.Leave, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.NoProof));
            Assert.Equal(RHAH_EnvoyHold.Leave, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.TimedOut));
            Assert.Equal(RHAH_EnvoyHold.None, RHAH_NarrativePace.HoldFor(SuiyinN008Outcome.Traded));
        }

        [Fact]
        public void NarrativeScansUseSeparateSlots()
        {
            Assert.True(RHAH_NarrativePace.Due(250, 2500, 250));
            Assert.False(RHAH_NarrativePace.Due(250, 2500, 500));
            Assert.True(RHAH_NarrativePace.Due(500, 2500, 500));
            Assert.False(RHAH_NarrativePace.Due(0, 2500, 250));
        }

        [Fact]
        public void JournalKeepsRealPawnIds()
        {
            SuiyinBook book = new SuiyinBook();
            Assert.True(book.OpenJournal(14, 1, 9, 0, new[] { 41, 42 }, true));
            Assert.Equal(41, book.Journals[0].People[0].LoadId);
            Assert.Equal(42, book.Journals[0].People[1].LoadId);
            Assert.False(book.OpenJournal(14, 1, 9, 1, new[] { 41 }, true));
            Assert.False(book.OpenJournal(14, 1, 10, 1, new int[0], true));
        }
        static SuiyinBook Copy(SuiyinBook source)
        {
            SuiyinBook copy = new SuiyinBook();
            copy.Seen.AddRange(source.Seen);
            copy.OpeningSent = source.OpeningSent;
            copy.ProgressSent = source.ProgressSent;
            copy.RewardClaimed = source.RewardClaimed;
            copy.RewardPaid = source.RewardPaid;
            copy.Trust = source.Trust;
            if (source.N009 != null)
            {
                copy.N009 = new SuiyinN009Case
                {
                    StartedTick = source.N009.StartedTick,
                    Deadline = source.N009.Deadline,
                    Outcome = source.N009.Outcome
                };
            }

            return copy;
        }

        static int Count(SuiyinBook book, SuiyinLetter letter)
        {
            int count = 0;
            for (int i = 0; i < book.Pending.Count; i++)
            {
                if (book.Pending[i].Letter == letter)
                {
                    count++;
                }
            }

            return count;
        }

        static int Arg(SuiyinBook book, SuiyinLetter letter)
        {
            for (int i = 0; i < book.Pending.Count; i++)
            {
                if (book.Pending[i].Letter == letter)
                {
                    return book.Pending[i].Arg;
                }
            }

            return -1;
        }
    }
}
