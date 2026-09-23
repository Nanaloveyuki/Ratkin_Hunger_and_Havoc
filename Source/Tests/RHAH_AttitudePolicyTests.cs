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
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(RHAH_Attitude.Hostile, false));
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(RHAH_Attitude.LeaningHostile, false));
            Assert.Equal(RHAH_AttitudeShift.None, RHAH_AttitudePolicy.React(RHAH_Attitude.Neutral, false));
            Assert.Equal(RHAH_AttitudeShift.Hostile, RHAH_AttitudePolicy.React(RHAH_Attitude.Neutral, true));
            Assert.Equal(RHAH_AttitudeShift.Leave, RHAH_AttitudePolicy.React(RHAH_Attitude.LeaningFriendly, false));
            Assert.Equal(RHAH_AttitudeShift.Leave, RHAH_AttitudePolicy.React(RHAH_Attitude.Friendly, true));
        }


        [Fact]
        public void CurrentAttitude_DrivesFightGate()
        {
            RHAH_PawnState state = new RHAH_PawnState();
            state.ApplySeed(new RHAH_PawnSeed(
                sourceIncidentDisplayId: "I-005",
                spawnBatchId: 1,
                relationshipGroupId: 1,
                role: RHAH_PawnRole.Wild,
                lifecycle: RHAH_Lifecycle.SeekingFood,
                attitudeAtArrival: RHAH_Attitude.Neutral));
            Assert.False(state.Allows(RHAH_BehaviorGate.Fight));

            state.SetAttitude(RHAH_Attitude.Hostile);
            Assert.Equal(RHAH_Attitude.Neutral, state.attitudeAtArrival);
            Assert.Equal(RHAH_Attitude.Neutral, state.ToSnapshot().AttitudeAtArrival);
            Assert.Equal(RHAH_Attitude.Hostile, state.ToSnapshot().Attitude);
            Assert.True(state.Allows(RHAH_BehaviorGate.Fight));
        }

        [Fact]
        public void FriendlyShift_BlocksOutsideRelief()
        {
            RHAH_PawnState state = new RHAH_PawnState();
            state.ApplySeed(new RHAH_PawnSeed(
                sourceIncidentDisplayId: "I-005",
                spawnBatchId: 1,
                relationshipGroupId: 1,
                role: RHAH_PawnRole.Beggar,
                lifecycle: RHAH_Lifecycle.SeekingFood,
                attitudeAtArrival: RHAH_Attitude.Neutral));
            Assert.True(state.Allows(RHAH_BehaviorGate.EatOutsideRelief));

            state.SetAttitude(RHAH_Attitude.Friendly);
            Assert.False(state.Allows(RHAH_BehaviorGate.EatOutsideRelief));
        }
    }
}
