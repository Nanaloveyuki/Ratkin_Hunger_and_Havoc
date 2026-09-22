using System;

namespace HungerAndHavoc.Incidents
{
    internal enum HungerIncidentSeason
    {
        Spring = 0,
        Summer = 1,
        Fall = 2,
        Winter = 3,
        Undefined = 4
    }

    internal readonly struct HungerIncidentWeightInput
    {
        public HungerIncidentFamily Family { get; }
        public HungerIncidentCategory Category { get; }
        public HungerIncidentTarget Target { get; }
        public HungerAttitudePool Pool { get; }
        public HungerIncidentSeason Season { get; }
        public int Trust { get; }
        public bool MapHome { get; }
        public bool PlayerCaravan { get; }

        public HungerIncidentWeightInput(
            HungerIncidentFamily family,
            HungerIncidentCategory category,
            HungerIncidentTarget target,
            HungerAttitudePool pool,
            HungerIncidentSeason season,
            int trust,
            bool mapHome,
            bool playerCaravan)
        {
            Family = family;
            Category = category;
            Target = target;
            Pool = pool;
            Season = season;
            Trust = trust;
            MapHome = mapHome;
            PlayerCaravan = playerCaravan;
        }
    }

    internal static class HungerIncidentWeight
    {
        internal const float PlagueFactor = 0.5f;
        internal const float SeasonFactor = 1.1f;
        internal const int TrustLimit = 100;

        internal static float FamilyBase(HungerIncidentFamily family)
        {
            switch (family)
            {
                case HungerIncidentFamily.Wild: return 1.4f;
                case HungerIncidentFamily.Beggar: return 1f;
                case HungerIncidentFamily.Thief: return 0.7f;
                case HungerIncidentFamily.Trade: return 0.5f;
                case HungerIncidentFamily.Siege: return 0.35f;
                case HungerIncidentFamily.Aid: return 0.25f;
                case HungerIncidentFamily.Special: return 0.2f;
                case HungerIncidentFamily.Intel: return 0.12f;
                default: return 0f;
            }
        }

        internal static float Evaluate(HungerIncidentWeightInput input)
        {
            if (!TargetMatches(input.Target, input.MapHome, input.PlayerCaravan))
            {
                return 0f;
            }

            float weight = FamilyBase(input.Family);
            if (weight <= 0f)
            {
                return 0f;
            }

            if (input.Season == HungerIncidentSeason.Spring || input.Season == HungerIncidentSeason.Winter)
            {
                weight *= SeasonFactor;
            }

            if (input.Category == HungerIncidentCategory.Plague)
            {
                weight *= PlagueFactor;
            }

            if (input.Pool == HungerAttitudePool.Negative)
            {
                weight *= TrustFactor(input.Trust);
            }

            return weight;
        }

        internal static float TrustFactor(int trust)
        {
            int clamped = trust;
            if (clamped < -TrustLimit)
            {
                clamped = -TrustLimit;
            }
            else if (clamped > TrustLimit)
            {
                clamped = TrustLimit;
            }

            return 1f - clamped / 400f;
        }

        internal static bool TargetMatches(HungerIncidentTarget target, bool mapHome, bool playerCaravan)
        {
            if (target == HungerIncidentTarget.Caravan)
            {
                return playerCaravan;
            }

            return target == HungerIncidentTarget.Map && mapHome;
        }

        internal static int Select(float[] weights, float roll)
        {
            if (weights == null || weights.Length == 0 || roll < 0f)
            {
                return -1;
            }

            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] > 0f)
                {
                    total += weights[i];
                }
            }

            if (total <= 0f || roll >= total)
            {
                return -1;
            }

            float cursor = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f)
                {
                    continue;
                }

                cursor += weights[i];
                if (roll < cursor)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
