using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerRequestRulesTests
    {
        [Fact]
        public void AidAndIntelKeepOldAmountsAndSites()
        {
            Assert.Equal(HungerRequestKind.SimpleMeal, HungerRequestRules.SpecFor("I-015").Kind);
            Assert.Equal(HungerIntelSiteKind.Treasure, HungerRequestRules.SpecFor("I-020").Site);
            Assert.Equal(HungerIntelSiteKind.Structure, HungerRequestRules.SpecFor("I-024").Site);
            Assert.Equal(HungerIntelSiteKind.Settlement, HungerRequestRules.SpecFor("I-028").Site);
            Assert.Equal(6, HungerRequestRules.Amount(HungerRequestKind.SimpleMeal, 0f, 0));
            Assert.Equal(28, HungerRequestRules.Amount(HungerRequestKind.SimpleMeal, 1000000f, 4));
            Assert.Equal(80, HungerRequestRules.Amount(HungerRequestKind.Silver, 0f, 0));
            Assert.Equal(1, HungerRequestRules.Amount(HungerRequestKind.Baby, 0f, 0));
            Assert.Equal("ItemStash", HungerRequestRules.SitePartDefName(HungerIntelSiteKind.Treasure));
        }

        [Fact]
        public void ClosedOrRepeatedChoicesDoNotSettleAgain()
        {
            Assert.Equal(HungerChoiceAction.None, HungerRequestRules.Settle(HungerChoiceAction.Deliver, false, false, true));
            Assert.Equal(HungerChoiceAction.None, HungerRequestRules.Settle(HungerChoiceAction.Deliver, true, true, true));
            Assert.Equal(HungerChoiceAction.None, HungerRequestRules.Settle(HungerChoiceAction.Deliver, true, false, false));
            Assert.Equal(HungerChoiceAction.Deliver, HungerRequestRules.Settle(HungerChoiceAction.Deliver, true, false, true));
            Assert.True(HungerRequestRules.CreatesSite(HungerChoiceAction.Deliver, HungerIntelSiteKind.Treasure));
            Assert.False(HungerRequestRules.CreatesSite(HungerChoiceAction.Reject, HungerIntelSiteKind.Treasure));
        }

        [Fact]
        public void SpecialIncidentsOfferAcceptRejectAndIgnore()
        {
            Assert.Equal(HungerChoiceKind.Abandoned, HungerRequestRules.SpecFor("I-003").Choice);
            Assert.Equal(HungerChoiceKind.Refugees, HungerRequestRules.SpecFor("I-011").Choice);
            Assert.Equal(HungerChoiceKind.ChildExchange, HungerRequestRules.SpecFor("I-013").Choice);
            Assert.Equal(HungerChoiceKind.Airdrop, HungerRequestRules.SpecFor("I-032").Choice);
            Assert.Equal(HungerChoiceKind.Kinship, HungerRequestRules.SpecFor("I-033").Choice);
            Assert.True(HungerRequestRules.OffersVisitorControl("I-008"));
            Assert.False(HungerRequestRules.OffersVisitorControl("I-051"));
        }

        [Fact]
        public void FamilyDropRecordsEachChildOnce()
        {
            List<int> dropped = new List<int>();
            Assert.True(RHAH_FamilyRules.CanDrop(HungerPawnRole.Mother, true, true));
            Assert.False(RHAH_FamilyRules.CanDrop(HungerPawnRole.Beggar, true, true));
            Assert.True(RHAH_FamilyRules.MarkDropped(dropped, 4));
            Assert.False(RHAH_FamilyRules.MarkDropped(dropped, 4));
            Assert.False(RHAH_FamilyRules.AllDropped(new[] { 4, 5 }, dropped));
            Assert.True(RHAH_FamilyRules.MarkDropped(dropped, 5));
            Assert.True(RHAH_FamilyRules.AllDropped(new[] { 4, 5 }, dropped));
            Assert.True(RHAH_FamilyRules.CanMotherFeed(HungerPawnRole.BeggarMother, true, true));
            Assert.False(RHAH_FamilyRules.CanScavenge(false, true, true));
            Assert.True(RHAH_FamilyRules.CanTailBite(true, true, true, true, 2.5f));
            Assert.False(RHAH_FamilyRules.CanTailBite(true, true, true, true, 3f));
        }

        [Fact]
        public void BroadcastUsesOnlyEnabledEligibleIncidents()
        {
            HashSet<string> disabled = new HashSet<string> { "I-001" };
            List<string> candidates = HungerBroadcastRules.Candidates(HungerIncidentCatalog.All, disabled);
            Assert.DoesNotContain("I-001", candidates);
            Assert.DoesNotContain("I-014", candidates);
            Assert.Contains("I-011", candidates);
            Assert.NotEmpty(candidates);
            Assert.Equal(3, HungerBroadcastRules.ClampDays(3));
            Assert.Equal(10, HungerBroadcastRules.ClampDays(40));
            Assert.Contains("I-011", candidates);
            Assert.Equal(candidates[0], HungerBroadcastRules.Pick(candidates, 0));
            Assert.Equal(candidates[candidates.Count - 1], HungerBroadcastRules.Pick(candidates, 99));
        }
    }
}
