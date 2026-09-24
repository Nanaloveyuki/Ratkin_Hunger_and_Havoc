using System.Collections.Generic;

namespace HungerAndHavoc.Narrative
{
    internal enum RHAH_EnvoyHold
    {
        None = 0,
        Stay = 1,
        Leave = 2
    }

    internal static class RHAH_NarrativePace
    {
        internal const int Spread = 250;

        internal static bool Due(int tick, int interval, int phase)
        {
            if (tick < 0 || interval <= 1)
            {
                return false;
            }

            int slot = phase < 0 ? 0 : phase % interval;
            return tick % interval == slot;
        }

        internal static RHAH_EnvoyHold HoldFor(SuiyinN008Outcome outcome)
        {
            if (outcome == SuiyinN008Outcome.Waiting || outcome == SuiyinN008Outcome.Checking)
            {
                return RHAH_EnvoyHold.Stay;
            }

            if (outcome == SuiyinN008Outcome.Refused ||
                outcome == SuiyinN008Outcome.Driven ||
                outcome == SuiyinN008Outcome.NoProof ||
                outcome == SuiyinN008Outcome.TimedOut)
            {
                return RHAH_EnvoyHold.Leave;
            }

            return RHAH_EnvoyHold.None;
        }

        internal static List<int> MemberIds(IList<int> loadIds, int count)
        {
            List<int> ids = new List<int>();
            if (loadIds == null || count <= 0)
            {
                return ids;
            }

            int take = count < loadIds.Count ? count : loadIds.Count;
            for (int i = 0; i < take; i++)
            {
                if (loadIds[i] > 0)
                {
                    ids.Add(loadIds[i]);
                }
            }

            return ids;
        }
    }
}
