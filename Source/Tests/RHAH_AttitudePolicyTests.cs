using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Pawn;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_AttitudePolicyTests
    {
        [Fact]
        public void DamageAndExpulsion_FollowAttitude()
        {
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(HungerAttitude.Hostile, false));
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(HungerAttitude.LeaningHostile, false));
            Assert.Equal(RHAH_AttitudeShift.None, RHAH_AttitudePolicy.React(HungerAttitude.Neutral, false));
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(HungerAttitude.Neutral, true));
            Assert.Equal(RHAH_AttitudeShift.Leave, RHAH_AttitudePolicy.React(HungerAttitude.LeaningFriendly, false));
            Assert.Equal(RHAH_AttitudeShift.Leave, RHAH_AttitudePolicy.React(HungerAttitude.Friendly, true));
        }


        [Fact]
        public void CurrentAttitude_DrivesFightGate()
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                sourceIncidentDisplayId: "I-005",
                spawnBatchId: 1,
                relationshipGroupId: 1,
                role: HungerPawnRole.Wild,
                lifecycle: HungerLifecycle.SeekingFood,
                attitudeAtArrival: HungerAttitude.Neutral));
            Assert.False(state.Allows(HungerBehaviorGate.Fight));

            state.SetAttitude(HungerAttitude.Hostile);
            Assert.Equal(HungerAttitude.Neutral, state.attitudeAtArrival);
            Assert.True(state.Allows(HungerBehaviorGate.Fight));
        }

        [Fact]
        public void FriendlyShift_BlocksOutsideRelief()
        {
            HungerPawnState state = new HungerPawnState();
            state.ApplySeed(new HungerPawnSeed(
                sourceIncidentDisplayId: "I-005",
                spawnBatchId: 1,
                relationshipGroupId: 1,
                role: HungerPawnRole.Beggar,
                lifecycle: HungerLifecycle.SeekingFood,
                attitudeAtArrival: HungerAttitude.Neutral));
            Assert.True(state.Allows(HungerBehaviorGate.EatOutsideRelief));

            state.SetAttitude(HungerAttitude.Friendly);
            Assert.False(state.Allows(HungerBehaviorGate.EatOutsideRelief));
        }
    }
}
