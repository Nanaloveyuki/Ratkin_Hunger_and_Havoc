using HungerAndHavoc.Storyteller.Suiyin;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public sealed class RHAH_SettlementProgressTests
    {
        [Fact]
        public void CumulativeTimersIgnoreArrivalAgeAndReadingDoesNotAdvanceCare()
        {
            SuiyinBook book = new SuiyinBook();
            SuiyinMember visitor = new SuiyinMember { LoadId = 1, CareTicks = 100 };
            SuiyinMember child = new SuiyinMember { LoadId = 2, CareTicks = 60000, Care = SuiyinCare.Malnutrition };
            book.Journals.Add(new SuiyinJournalCase { Id = 14, StartedTick = 60000, People = { visitor } });
            book.N005.Add(new SuiyinN005Case { Id = 3, Outcome = SuiyinN005Outcome.Accepted, Children = { child } });

            var groups = RHAH_SettlementProgress.Build(book, 180000, 30);
            Assert.Equal(179901, groups[0].Members[0].RemainingTicks);
            Assert.Equal(1680000, groups[0].RemainingObservationTicks);
            Assert.Equal(120001, groups[1].Members[0].RemainingTicks);
            Assert.Equal(-1, groups[1].RemainingObservationTicks);
            Assert.Equal(RHAH_SettlementBlocker.Malnutrition, groups[1].Members[0].Blockers);
            Assert.Equal(100, visitor.CareTicks);
            Assert.Equal(60000, child.CareTicks);
            Assert.False(book.Journals[0].Closed);
            Assert.False(book.N005[0].CareClosed);
            Assert.Empty(book.Pending);
        }

        [Fact]
        public void ClosedHistoryAndInterceptionsStayOutWhileTerminalMembersRemainVisibleInOpenGroups()
        {
            SuiyinBook book = new SuiyinBook();
            book.Journals.Add(new SuiyinJournalCase { Id = 0, Failed = true });
            book.Journals.Add(new SuiyinJournalCase { Id = 14, Closed = true });
            book.N005.Add(new SuiyinN005Case { Outcome = SuiyinN005Outcome.Cared, CareClosed = true });
            book.N004.Add(new SuiyinN004Case { Outcome = SuiyinN004Outcome.FamilyHere });
            book.Journals.Add(new SuiyinJournalCase
            {
                Id = 5,
                BatchId = 7,
                People =
                {
                    new SuiyinMember { LoadId = 1, Presence = SuiyinPresence.Left },
                    new SuiyinMember { LoadId = 2, Presence = SuiyinPresence.Unknown },
                    new SuiyinMember { LoadId = 3, Presence = SuiyinPresence.Dead }
                }
            });

            var group = Assert.Single(RHAH_SettlementProgress.Build(book, 900000, 30));
            Assert.Equal(7, group.BatchId);
            Assert.Equal(RHAH_SettlementBlocker.Left, group.Members[0].Blockers);
            Assert.Equal(RHAH_SettlementBlocker.Unknown, group.Members[1].Blockers);
            Assert.Equal(RHAH_SettlementBlocker.Dead, group.Members[2].Blockers);
            Assert.All(group.Members, member => Assert.False(member.Ready));
        }

        [Fact]
        public void ThreeDaysIsStrictAndCaptivityDoesNotBlockSettlementWhileGrowthStaysIndependent()
        {
            SuiyinBook book = new SuiyinBook();
            book.Journals.Add(new SuiyinJournalCase
            {
                Id = 14,
                People = { new SuiyinMember { LoadId = 1, Care = SuiyinCare.Captive, CareTicks = 180000 } }
            });
            book.N005.Add(new SuiyinN005Case
            {
                Outcome = SuiyinN005Outcome.Accepted,
                Children = { new SuiyinMember { LoadId = 2, Care = SuiyinCare.Captive, CareTicks = 300000 } }
            });
            SuiyinN004Case entrust = new SuiyinN004Case
            {
                Mother = SuiyinPresence.Here,
                Children = { new SuiyinMember { LoadId = 3, Child = true } }
            };
            book.N004.Add(entrust);

            var groups = RHAH_SettlementProgress.Build(book, 300000, 30);
            Assert.False(groups[0].Members[0].Ready);
            Assert.Equal(1, groups[0].Members[0].RemainingTicks);
            Assert.True(groups[1].Members[0].Ready);
            Assert.Equal(RHAH_SettlementBlocker.None, groups[1].Members[0].Blockers);
            Assert.Equal(-1, groups[2].Members[0].RemainingTicks);
            Assert.Equal(RHAH_SettlementBlocker.Growing, groups[2].Members[0].Blockers);
            Assert.False(groups[2].Members[0].Ready);

            entrust.Children[0].Child = false;
            Assert.True(RHAH_SettlementProgress.Build(book, 300000, 30)[2].Members[0].Ready);
            entrust.Mother = SuiyinPresence.Unknown;
            Assert.False(RHAH_SettlementProgress.Build(book, 300000, 30)[2].Members[0].Ready);
        }

        [Fact]
        public void OrdinaryReliefAccumulatesThroughOtherIllnessAndDoesNotExpireWhilePresent()
        {
            SuiyinBook book = new SuiyinBook();
            SuiyinMember visitor = new SuiyinMember { LoadId = 1, Care = SuiyinCare.Plague, LastCareTick = 0 };
            SuiyinJournalCase record = new SuiyinJournalCase { Id = 14, People = { visitor } };
            book.Journals.Add(record);
            visitor.ObserveSettlement(60000, false);
            Assert.Equal(60000, visitor.CareTicks);
            visitor.ObserveSettlement(120000, true);
            visitor.ObserveSettlement(40 * 60000, true);
            Assert.False(book.CloseJournal(record, 40 * 60000));
            Assert.False(record.Closed);
            Assert.Equal(60000, visitor.CareTicks);
            visitor.ObserveSettlement(41 * 60000, false);
            visitor.ObserveSettlement(43 * 60000 + 1, false);
            Assert.True(book.CloseJournal(record, 43 * 60000 + 1));
            Assert.True(record.Counted);
        }

        [Fact]
        public void LegacyCareIsPreservedWithoutBackfillingUnobservedTimeOrMissingIntervals()
        {
            SuiyinMember member = new SuiyinMember { CareTicks = 60000 };
            member.ObserveSettlement(900000, false);
            Assert.Equal(60000, member.CareTicks);
            member.ObserveSettlement(902500, false);
            Assert.Equal(62500, member.CareTicks);
            member.Presence = SuiyinPresence.Unknown;
            member.ObserveSettlement(905000, false);
            member.Presence = SuiyinPresence.Here;
            member.ObserveSettlement(1200000, false);
            Assert.Equal(62500, member.CareTicks);
        }
    }
}
