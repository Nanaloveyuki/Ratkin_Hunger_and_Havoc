using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_RequestRulesTests
    {
        [Fact]
        public void AidAndIntelKeepOldAmountsAndSites()
        {
            Assert.Equal(RHAH_RequestKind.SimpleMeal, RHAH_RequestRules.SpecFor("I-015").Kind);
            Assert.Equal(RHAH_IntelSiteKind.Treasure, RHAH_RequestRules.SpecFor("I-020").Site);
            Assert.Equal(RHAH_IntelSiteKind.Structure, RHAH_RequestRules.SpecFor("I-024").Site);
            Assert.Equal(RHAH_IntelSiteKind.Settlement, RHAH_RequestRules.SpecFor("I-028").Site);
            Assert.Equal(6, RHAH_RequestRules.Amount(RHAH_RequestKind.SimpleMeal, 0f, 0));
            Assert.Equal(28, RHAH_RequestRules.Amount(RHAH_RequestKind.SimpleMeal, 1000000f, 4));
            Assert.Equal(80, RHAH_RequestRules.Amount(RHAH_RequestKind.Silver, 0f, 0));
            Assert.Equal(1, RHAH_RequestRules.Amount(RHAH_RequestKind.Baby, 0f, 0));
            Assert.Equal("ItemStash", RHAH_RequestRules.SitePartDefName(RHAH_IntelSiteKind.Treasure));
        }

        [Fact]
        public void ClosedOrRepeatedChoicesDoNotSettleAgain()
        {
            Assert.Equal(RHAH_ChoiceAction.None, RHAH_RequestRules.Settle(RHAH_ChoiceAction.Deliver, false, false, true));
            Assert.Equal(RHAH_ChoiceAction.None, RHAH_RequestRules.Settle(RHAH_ChoiceAction.Deliver, true, true, true));
            Assert.Equal(RHAH_ChoiceAction.None, RHAH_RequestRules.Settle(RHAH_ChoiceAction.Deliver, true, false, false));
            Assert.Equal(RHAH_ChoiceAction.Deliver, RHAH_RequestRules.Settle(RHAH_ChoiceAction.Deliver, true, false, true));
            Assert.True(RHAH_RequestRules.CreatesSite(RHAH_ChoiceAction.Deliver, RHAH_IntelSiteKind.Treasure));
            Assert.False(RHAH_RequestRules.CreatesSite(RHAH_ChoiceAction.Reject, RHAH_IntelSiteKind.Treasure));
        }

        [Fact]
        public void SpecialIncidentsOfferAcceptRejectAndIgnore()
        {
            Assert.Equal(RHAH_ChoiceKind.Abandoned, RHAH_RequestRules.SpecFor("I-003").Choice);
            Assert.Equal(RHAH_ChoiceKind.Refugees, RHAH_RequestRules.SpecFor("I-011").Choice);
            Assert.Equal(RHAH_ChoiceKind.ChildExchange, RHAH_RequestRules.SpecFor("I-013").Choice);
            Assert.Equal(RHAH_ChoiceKind.Airdrop, RHAH_RequestRules.SpecFor("I-032").Choice);
            Assert.Equal(RHAH_ChoiceKind.Kinship, RHAH_RequestRules.SpecFor("I-033").Choice);
            Assert.True(RHAH_RequestRules.OffersVisitorControl("I-008"));
            Assert.False(RHAH_RequestRules.OffersVisitorControl("I-051"));
        }
        [Fact]
        public void TraderCaravansStayAndFoodReplacesChildren()
        {
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnvironmentLeave(true, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnvironmentLeave(false, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnvironmentLeave(true, false));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnclosedLeave(true, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnclosedLeave(false, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.BlocksEnclosedLeave(true, false));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.CellIsHarsh(-30f, 0f, 20f));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.CellIsHarsh(15f, 0f, 20f));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.RoomIsEnclosed(true, false, false));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.RoomIsEnclosed(true, true, false));
            Assert.Equal(20, RHAH_RequestRules.FoodForChildren(2));
            Assert.True(RHAH_RequestRules.CanSubstituteFood(true, RHAH_ChoiceKind.ChildExchange, 20, 2));
            Assert.False(RHAH_RequestRules.CanSubstituteFood(false, RHAH_ChoiceKind.ChildExchange, 20, 2));
            Assert.False(RHAH_RequestRules.CanSubstituteFood(true, RHAH_ChoiceKind.ChildExchange, 19, 2));
            Assert.False(RHAH_RequestRules.CanSubstituteFood(true, RHAH_ChoiceKind.Aid, 20, 2));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.NpcSellsFood(true, true));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.NpcSellsFood(true, false));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.ShouldLeave(false, false, false, true, true, true, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.ShouldLeave(true, true, true, true, true, true, true));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.ShouldLeave(true, false, true, true, false, false, false));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.ShouldLeave(true, true, false, false, false, false, true));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.ShouldLeave(true, false, false, false, false, false, false));
            Assert.True(HungerAndHavoc.Trade.RHAH_CaravanStay.TemperatureSevere(0.16f, 0f));
            Assert.False(HungerAndHavoc.Trade.RHAH_CaravanStay.TemperatureSevere(0.15f, 0.15f));
        }

        [Fact]
        public void FamilyDropRecordsEachChildOnce()
        {
            List<int> dropped = new List<int>();
            Assert.True(RHAH_FamilyRules.CanDrop(RHAH_PawnRole.Mother, true, true));
            Assert.False(RHAH_FamilyRules.CanDrop(RHAH_PawnRole.Beggar, true, true));
            Assert.True(RHAH_FamilyRules.MarkDropped(dropped, 4));
            Assert.False(RHAH_FamilyRules.MarkDropped(dropped, 4));
            Assert.False(RHAH_FamilyRules.AllDropped(new[] { 4, 5 }, dropped));
            Assert.True(RHAH_FamilyRules.MarkDropped(dropped, 5));
            Assert.True(RHAH_FamilyRules.AllDropped(new[] { 4, 5 }, dropped));
            Assert.True(RHAH_FamilyRules.CanMotherFeed(RHAH_PawnRole.BeggarMother, true, true));
            Assert.False(RHAH_FamilyRules.CanScavenge(false, true, true));
            Assert.True(RHAH_FamilyRules.CanTailBite(true, true, true, true, 2.5f));
            Assert.False(RHAH_FamilyRules.CanTailBite(true, true, true, true, 3f));
        }

        [Fact]
        public void BroadcastUsesOnlyEnabledEligibleIncidents()
        {
            HashSet<string> disabled = new HashSet<string> { "I-001" };
            List<string> candidates = RHAH_BroadcastRules.Candidates(RHAH_IncidentCatalog.All, disabled);
            Assert.DoesNotContain("I-001", candidates);
            Assert.DoesNotContain("I-014", candidates);
            Assert.Contains("I-011", candidates);
            Assert.NotEmpty(candidates);
            Assert.Equal(3, RHAH_BroadcastRules.ClampDays(3));
            Assert.Equal(10, RHAH_BroadcastRules.ClampDays(40));
            Assert.Contains("I-011", candidates);
            Assert.Equal(candidates[0], RHAH_BroadcastRules.Pick(candidates, 0));
            Assert.Equal(candidates[candidates.Count - 1], RHAH_BroadcastRules.Pick(candidates, 99));
        }
    }
}
