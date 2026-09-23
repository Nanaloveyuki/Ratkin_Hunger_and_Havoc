using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Pawn.Compat;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_LeashPlanTests
    {
        [Fact]
        public void MissingSnapshotsDoNotPair()
        {
            Assert.False(RHAH_LeashPlan.TryPair(null, null, null, null, out List<RHAH_LeashPair> pairs, out bool travel));
            Assert.Empty(pairs);
            Assert.False(travel);
        }

        [Theory]
        [InlineData(RHAH_PawnRole.RatkinYoung, true)]
        [InlineData(RHAH_PawnRole.BeggarChild, true)]
        [InlineData(RHAH_PawnRole.ThiefChild, true)]
        [InlineData(RHAH_PawnRole.WildChild, true)]
        [InlineData(RHAH_PawnRole.Beggar, false)]
        [InlineData(RHAH_PawnRole.Trader, false)]
        public void OnlyYoungRolesCanBePets(RHAH_PawnRole role, bool child)
        {
            Assert.Equal(child, RHAH_LeashPlan.IsLeashChild(role));
        }

        [Fact]
        public void GateDenialAndPlayerFactionSkipTheChild()
        {
            Assert.Equal(RHAH_LeashKind.None, RHAH_LeashPlan.Kind("I-004", true, true, false, false));
            Assert.Equal(RHAH_LeashKind.None, RHAH_LeashPlan.Kind("I-004", true, true, true, true));
            Assert.Equal(RHAH_LeashKind.None, RHAH_LeashPlan.Kind(null, true, true, true, false));
        }

        [Fact]
        public void BeggarFamilyParentUsesMotherSource()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-004", RHAH_PawnRole.Beggar, 4),
                Snapshot("I-004", RHAH_PawnRole.BeggarChild, 4)
            };
            bool[] parents = { false, false, true, false };

            Assert.True(RHAH_LeashPlan.TryPair(snapshots, null, parents, null, out List<RHAH_LeashPair> pairs, out bool travel));
            Assert.False(travel);
            Assert.Equal(0, pairs[0].AdultIndex);
            Assert.Equal(1, pairs[0].ChildIndex);
            Assert.Equal(RHAH_LeashPlan.BeggarFamilySource, pairs[0].SpecialSource);
        }

        [Fact]
        public void BeggarFamilyWithoutParentDoesNotUseMotherLeash()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-004", RHAH_PawnRole.Beggar, 4),
                Snapshot("I-004", RHAH_PawnRole.BeggarChild, 4)
            };

            Assert.False(RHAH_LeashPlan.TryPair(snapshots, null, null, null, out List<RHAH_LeashPair> pairs, out _));
            Assert.Empty(pairs);
            Assert.Equal(RHAH_LeashKind.None, RHAH_LeashPlan.Kind("I-004", false, true, true, false));
        }

        [Fact]
        public void OtherOriginalKinUsesChildLeashWithoutSpecialSource()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-005", RHAH_PawnRole.Beggar, 8),
                Snapshot("I-005", RHAH_PawnRole.BeggarChild, 8)
            };

            Assert.True(RHAH_LeashPlan.TryPair(snapshots, null, null, null, out List<RHAH_LeashPair> pairs, out bool travel));
            Assert.False(travel);
            Assert.Equal(RHAH_LeashKind.Child, RHAH_LeashPlan.Kind("I-005", false, true, true, false));
            Assert.Equal(0, pairs[0].SpecialSource);
        }

        [Fact]
        public void ParentBeatsKinAndDifferentGroupsStayApart()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-011", RHAH_PawnRole.Refugee, 3),
                Snapshot("I-011", RHAH_PawnRole.Beggar, 3),
                Snapshot("I-011", RHAH_PawnRole.RatkinYoung, 3),
                Snapshot("I-011", RHAH_PawnRole.RatkinYoung, 9)
            };
            bool[] parents = new bool[16];
            parents[2 * 4 + 1] = true;

            Assert.True(RHAH_LeashPlan.TryPair(snapshots, null, parents, null, out List<RHAH_LeashPair> pairs, out _));
            Assert.Single(pairs);
            Assert.Equal(1, pairs[0].AdultIndex);
            Assert.Equal(2, pairs[0].ChildIndex);
            Assert.Equal(0, pairs[0].SpecialSource);
        }

        [Fact]
        public void QuarantineDeniedChildIsNotPaired()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-005", RHAH_PawnRole.Beggar, 2),
                Snapshot("I-005", RHAH_PawnRole.BeggarChild, 2)
            };

            Assert.False(RHAH_LeashPlan.TryPair(snapshots, new[] { true, false }, null, null, out List<RHAH_LeashPair> pairs, out _));
            Assert.Empty(pairs);
        }

        [Fact]
        public void UnmarkedColonistAndAdultAreNotPets()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot(null, RHAH_PawnRole.Beggar, 1),
                Snapshot("I-005", RHAH_PawnRole.Beggar, 1)
            };

            Assert.False(RHAH_LeashPlan.TryPair(snapshots, null, null, null, out List<RHAH_LeashPair> pairs, out _));
            Assert.Empty(pairs);
        }

        [Theory]
        [InlineData("I-012", true)]
        [InlineData("I-038", true)]
        [InlineData("I-005", false)]
        [InlineData("I-013", false)]
        public void OnlyTraderCaravansUseTravelAssignment(string displayId, bool travel)
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot(displayId, RHAH_PawnRole.Trader, 1),
                Snapshot(displayId, RHAH_PawnRole.RatkinYoung, 1)
            };

            RHAH_LeashPlan.TryPair(snapshots, null, null, null, out List<RHAH_LeashPair> pairs, out bool assigned);
            Assert.Equal(travel, assigned);
            Assert.Equal(travel, RHAH_LeashPlan.IsTravel(displayId));
            if (travel)
            {
                Assert.Empty(pairs);
            }
        }
        [Fact]
        public void ChildExchangeParentUsesExchangeSource()
        {
            List<IRHAH_Pawn> snapshots = new List<IRHAH_Pawn>
            {
                Snapshot("I-013", RHAH_PawnRole.Trader, 6),
                Snapshot("I-013", RHAH_PawnRole.RatkinYoung, 6)
            };
            bool[] parents = { false, false, true, false };

            Assert.True(RHAH_LeashPlan.TryPair(snapshots, null, parents, null, out List<RHAH_LeashPair> pairs, out _));
            Assert.Equal(RHAH_LeashPlan.ChildExchangeSource, pairs[0].SpecialSource);
            Assert.Equal(RHAH_LeashKind.Mother, RHAH_LeashPlan.Kind("I-013", true, true, true, false));
        }

        [Fact]
        public void LeashGateStillAbstains()
        {
            RHAH_LeashCompat leash = new RHAH_LeashCompat();
            Assert.Null(leash.Allows(null, Snapshot("I-004", RHAH_PawnRole.BeggarChild, 1), RHAH_BehaviorGate.Leash));
            Assert.Null(leash.ShouldReleaseToColony(null, Snapshot("I-004", RHAH_PawnRole.BeggarChild, 1), RHAH_ReleaseReason.Recruited));
        }

        static RHAH_PawnSnapshot Snapshot(string displayId, RHAH_PawnRole role, int group)
        {
            return new RHAH_PawnSnapshot(
                displayId,
                1,
                group,
                role,
                RHAH_Lifecycle.Arriving,
                false,
                -1,
                false,
                RHAH_Attitude.Neutral,
                RHAH_Attitude.Neutral,
                0,
                null);
        }
    }
}
