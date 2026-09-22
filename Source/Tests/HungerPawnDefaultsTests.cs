using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class HungerPawnDefaultsTests
    {
        [Theory]
        [InlineData(HungerLifecycle.Released)]
        [InlineData(HungerLifecycle.Dead)]
        public void InactiveVisitor_OnlyColonyControlGates(HungerLifecycle lifecycle)
        {
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.JoinColony));
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Hire));
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Transfer));
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Imprison));
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Leash));
            Assert.True(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Carry));
            Assert.False(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.Beg));
            Assert.False(Allow(HungerPawnRole.Thief, lifecycle, HungerAttitude.Hostile, HungerBehaviorGate.Steal));
            Assert.False(Allow(HungerPawnRole.Siege, lifecycle, HungerAttitude.Hostile, HungerBehaviorGate.Fight));
            Assert.False(Allow(HungerPawnRole.Beggar, lifecycle, HungerAttitude.Neutral, HungerBehaviorGate.ExitMap));
        }

        [Theory]
        [InlineData(HungerPawnRole.Beggar)]
        [InlineData(HungerPawnRole.BeggarMother)]
        [InlineData(HungerPawnRole.BeggarChild)]
        [InlineData(HungerPawnRole.Refugee)]
        [InlineData(HungerPawnRole.Labor)]
        public void BeggarRoles_CanBeg(HungerPawnRole role)
        {
            Assert.True(AllowVisitor(role, HungerBehaviorGate.Beg));
        }

        [Theory]
        [InlineData(HungerPawnRole.Thief)]
        [InlineData(HungerPawnRole.Wild)]
        [InlineData(HungerPawnRole.Trader)]
        [InlineData(HungerPawnRole.Unspecified)]
        [InlineData(HungerPawnRole.Mother)]
        public void NonBeggarRoles_CannotBeg(HungerPawnRole role)
        {
            Assert.False(AllowVisitor(role, HungerBehaviorGate.Beg));
        }

        [Theory]
        [InlineData(HungerPawnRole.Thief)]
        [InlineData(HungerPawnRole.ThiefChild)]
        public void ThiefRoles_CanSteal(HungerPawnRole role)
        {
            Assert.True(AllowVisitor(role, HungerBehaviorGate.Steal));
        }

        [Fact]
        public void NonThief_CannotSteal()
        {
            Assert.False(AllowVisitor(HungerPawnRole.Beggar, HungerBehaviorGate.Steal));
            Assert.False(AllowVisitor(HungerPawnRole.Wild, HungerBehaviorGate.Steal));
        }

        [Fact]
        public void Fight_SiegeOrHostile()
        {
            Assert.True(AllowVisitor(HungerPawnRole.Siege, HungerBehaviorGate.Fight));
            Assert.True(Allow(
                HungerPawnRole.Wild,
                HungerLifecycle.SeekingFood,
                HungerAttitude.Hostile,
                HungerBehaviorGate.Fight));
            Assert.False(AllowVisitor(HungerPawnRole.Beggar, HungerBehaviorGate.Fight));
            Assert.False(Allow(
                HungerPawnRole.Wild,
                HungerLifecycle.SeekingFood,
                HungerAttitude.LeaningHostile,
                HungerBehaviorGate.Fight));
        }

        [Fact]
        public void EatOutsideRelief_BlockedWhenFriendly()
        {
            Assert.False(Allow(
                HungerPawnRole.Beggar,
                HungerLifecycle.Arriving,
                HungerAttitude.Friendly,
                HungerBehaviorGate.EatOutsideRelief));
            Assert.True(AllowVisitor(HungerPawnRole.Beggar, HungerBehaviorGate.EatOutsideRelief));
            Assert.True(Allow(
                HungerPawnRole.Beggar,
                HungerLifecycle.Arriving,
                HungerAttitude.LeaningFriendly,
                HungerBehaviorGate.EatOutsideRelief));
        }

        [Theory]
        [InlineData(HungerPawnRole.RatkinYoung)]
        [InlineData(HungerPawnRole.BeggarChild)]
        [InlineData(HungerPawnRole.ThiefChild)]
        [InlineData(HungerPawnRole.WildChild)]
        public void ChildRoles_CanLeashAndCarry(HungerPawnRole role)
        {
            Assert.True(AllowVisitor(role, HungerBehaviorGate.Leash));
            Assert.True(AllowVisitor(role, HungerBehaviorGate.Carry));
        }

        [Fact]
        public void AdultVisitor_CannotLeashOrCarry()
        {
            Assert.False(AllowVisitor(HungerPawnRole.Beggar, HungerBehaviorGate.Leash));
            Assert.False(AllowVisitor(HungerPawnRole.Mother, HungerBehaviorGate.Carry));
        }

        [Theory]
        [InlineData(HungerPawnRole.Mother)]
        [InlineData(HungerPawnRole.BeggarMother)]
        public void MotherRoles_CanDropOffChild(HungerPawnRole role)
        {
            Assert.True(AllowVisitor(role, HungerBehaviorGate.DropOffChild));
        }

        [Fact]
        public void NonMother_CannotDropOffChild()
        {
            Assert.False(AllowVisitor(HungerPawnRole.Beggar, HungerBehaviorGate.DropOffChild));
            Assert.False(AllowVisitor(HungerPawnRole.RatkinYoung, HungerBehaviorGate.DropOffChild));
        }

        [Fact]
        public void ActiveVisitor_SharedGates()
        {
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.LeaveAfterFed));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.FeedFromRelief));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.Gnaw));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.JoinColony));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.Hire));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.Transfer));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.Imprison));
            Assert.True(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.ExitMap));
            Assert.False(AllowVisitor(HungerPawnRole.Unspecified, HungerBehaviorGate.TailBite));
        }

        [Fact]
        public void NullSnapshot_TreatedAsInactive()
        {
            Assert.True(HungerPawnDefaults.Allows((IHungerPawn)null, HungerBehaviorGate.JoinColony));
            Assert.True(HungerPawnDefaults.Allows((IHungerPawn)null, HungerBehaviorGate.Hire));
            Assert.True(HungerPawnDefaults.Allows((IHungerPawn)null, HungerBehaviorGate.Transfer));
            Assert.False(HungerPawnDefaults.Allows((IHungerPawn)null, HungerBehaviorGate.Beg));
        }

        [Fact]
        public void SnapshotOverload_UsesRoleLifecycleAttitude()
        {
            HungerPawnSnapshot visitor = new HungerPawnSnapshot(
                "I-005",
                0,
                0,
                HungerPawnRole.Thief,
                HungerLifecycle.SeekingFood,
                false,
                -1,
                false,
                HungerAttitude.Neutral,
                0,
                null);
            HungerPawnSnapshot released = new HungerPawnSnapshot(
                "I-005",
                0,
                0,
                HungerPawnRole.Thief,
                HungerLifecycle.Released,
                true,
                -1,
                false,
                HungerAttitude.Hostile,
                0,
                null);

            Assert.True(HungerPawnDefaults.Allows(visitor, HungerBehaviorGate.Steal));
            Assert.False(HungerPawnDefaults.Allows(visitor, HungerBehaviorGate.Fight));
            Assert.False(HungerPawnDefaults.Allows(released, HungerBehaviorGate.Steal));
            Assert.True(HungerPawnDefaults.Allows(released, HungerBehaviorGate.Imprison));
        }

        static bool AllowVisitor(HungerPawnRole role, HungerBehaviorGate gate)
        {
            return Allow(role, HungerLifecycle.SeekingFood, HungerAttitude.Neutral, gate);
        }

        static bool Allow(
            HungerPawnRole role,
            HungerLifecycle lifecycle,
            HungerAttitude attitude,
            HungerBehaviorGate gate)
        {
            return HungerPawnDefaults.Allows(role, lifecycle, attitude, gate);
        }
    }
}
