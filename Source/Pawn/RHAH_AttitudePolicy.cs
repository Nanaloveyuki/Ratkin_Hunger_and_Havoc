using HungerAndHavoc.Api;

namespace HungerAndHavoc.Pawn
{
    internal enum RHAH_AttitudeShift
    {
        None = 0,
        Hostile = 1,
        Leave = 2
    }

    internal static class RHAH_AttitudePolicy
    {
        internal static RHAH_AttitudeShift React(RHAH_Attitude attitude, bool forcedAway)
        {
            switch (attitude)
            {
                case RHAH_Attitude.Hostile:
                    return RHAH_AttitudeShift.Hostile;
                case RHAH_Attitude.LeaningHostile:
                    return RHAH_AttitudeShift.Hostile;
                case RHAH_Attitude.LeaningFriendly:
                case RHAH_Attitude.Friendly:
                    return RHAH_AttitudeShift.Leave;
                default:
                    return forcedAway ? RHAH_AttitudeShift.Hostile : RHAH_AttitudeShift.None;
            }
        }
    }
}
