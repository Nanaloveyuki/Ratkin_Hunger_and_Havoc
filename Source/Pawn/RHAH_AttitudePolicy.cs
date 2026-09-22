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
        internal static RHAH_AttitudeShift React(HungerAttitude attitude, bool forcedAway)
        {
            switch (attitude)
            {
                case HungerAttitude.Hostile:
                    return RHAH_AttitudeShift.Hostile;
                case HungerAttitude.LeaningHostile:
                    return RHAH_AttitudeShift.Hostile;
                case HungerAttitude.LeaningFriendly:
                case HungerAttitude.Friendly:
                    return RHAH_AttitudeShift.Leave;
                default:
                    return forcedAway ? RHAH_AttitudeShift.Hostile : RHAH_AttitudeShift.None;
            }
        }
    }
}
