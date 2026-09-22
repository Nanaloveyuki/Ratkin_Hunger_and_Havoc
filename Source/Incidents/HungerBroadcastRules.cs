using System.Collections.Generic;

namespace HungerAndHavoc.Incidents
{
    internal static class HungerBroadcastRules
    {
        internal const int MinCooldownDays = 0;
        internal const int MaxCooldownDays = 10;
        internal const int DefaultCooldownDays = 3;

        internal static int ClampDays(int days)
        {
            if (days < MinCooldownDays)
            {
                return MinCooldownDays;
            }

            return days > MaxCooldownDays ? MaxCooldownDays : days;
        }

        internal static bool Ready(int tick, int cooldownUntilTick)
        {
            return tick >= cooldownUntilTick;
        }

        internal static int NextCooldown(int tick, int days)
        {
            return tick + HungerRequestRules.TicksPerDay * ClampDays(days);
        }

        internal static List<string> Candidates(IReadOnlyList<HungerIncidentEntry> entries, ISet<string> disabled)
        {
            List<string> result = new List<string>();
            if (entries == null)
            {
                return result;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                HungerIncidentEntry entry = entries[i];
                if (entry == null || !entry.BroadcastEligible)
                {
                    continue;
                }

                if (disabled != null && disabled.Contains(entry.DisplayId))
                {
                    continue;
                }

                result.Add(entry.DisplayId);
            }

            return result;
        }

        internal static string Pick(IReadOnlyList<string> candidates, int roll)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            int index = roll;
            if (index < 0)
            {
                index = 0;
            }

            if (index >= candidates.Count)
            {
                index = candidates.Count - 1;
            }

            return candidates[index];
        }
    }

    internal static class HungerGenerationQueue
    {
        internal const int DefaultBatchSize = 1;

        internal static int Take(int pending, int batchSize, bool enabled)
        {
            if (!enabled || pending <= 0)
            {
                return pending <= 0 ? 0 : pending;
            }

            int size = batchSize < 1 ? 1 : batchSize;
            return pending < size ? pending : size;
        }
    }
}
