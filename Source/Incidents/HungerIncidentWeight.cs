using Verse;
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

    internal static class HungerIncidentTuning
    {
        internal const float MinDebugPoints = 1f;
        internal const float MaxDebugPoints = 10000f;
        internal const float DefaultWeight = 100f;
        internal const float MinWeight = 0f;
        internal const float MaxWeight = 100f;

        internal static float ClampPoints(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < MinDebugPoints)
            {
                return MinDebugPoints;
            }

            return value > MaxDebugPoints ? MaxDebugPoints : value;
        }

        internal static float ClampWeight(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < MinWeight)
            {
                return MinWeight;
            }

            return value > MaxWeight ? MaxWeight : value;
        }

        internal static float Scale(float formulaWeight, bool enabled, float playerWeight)
        {
            if (!enabled || formulaWeight <= 0f)
            {
                return 0f;
            }

            return formulaWeight * (ClampWeight(playerWeight) / DefaultWeight);
        }
    }

    internal static class HungerIncidentScale
    {
        internal const float ReferencePoints = 300f;

        internal static int Count(string displayId, float points)
        {
            float safe = HungerIncidentTuning.ClampPoints(points);
            switch (displayId)
            {
                case "I-001": return Fixed(safe, 10, 50, 50f);
                case "I-002": return Linear(safe, 120f, 1, 10);
                case "I-003": return Linear(safe, 150f, 1, 10) + 1;
                case "I-004": return 3;
                case "I-005":
                case "I-042": return Spread(safe, 3, 20, 85f);
                case "I-006":
                case "I-007": return Spread(safe, 3, 20, 75f);
                case "I-008":
                case "I-009":
                case "I-013":
                case "I-019":
                case "I-033":
                case "I-037":
                case "I-041":
                case "I-047":
                case "I-049": return 1;
                case "I-010": return Fixed(safe, 2, 10, 120f);
                case "I-011":
                case "I-040": return Linear(safe, 200f, 1, 4);
                case "I-012":
                case "I-038": return 3;
                case "I-014": return 5;
                case "I-015":
                case "I-016":
                case "I-017":
                case "I-018":
                case "I-020":
                case "I-021":
                case "I-022":
                case "I-023":
                case "I-024":
                case "I-025":
                case "I-026":
                case "I-027":
                case "I-028": return 1;
                case "I-029":
                case "I-044": return 2;
                case "I-030":
                case "I-045": return Spread(safe, 4, 16, 80f);
                case "I-031":
                case "I-039": return Spread(safe, 4, 12, 85f);
                case "I-032":
                case "I-046": return Spread(safe, 4, 14, 80f);
                case "I-034":
                case "I-048": return Spread(safe, 8, 28, 55f);
                case "I-035":
                case "I-050": return Spread(safe, 3, 10, 85f);
                case "I-036": return Spread(safe, 2, 8, 90f);
                case "I-043": return Spread(safe, 3, 18, 90f);
                default: return 1;
            }
        }

        internal static int ScaleAmount(int amount, float points, int min, int max)
        {
            if (amount <= 0)
            {
                return 0;
            }

            float safe = HungerIncidentTuning.ClampPoints(points);
            int scaled = Round(amount * (safe / ReferencePoints));
            if (scaled < min)
            {
                return min;
            }

            return scaled > max ? max : scaled;
        }

        static int Linear(float points, float perStep, int min, int max)
        {
            return Clamp(Round(points / perStep) + 1, min, max);
        }

        static int Fixed(float points, int min, int max, float perStep)
        {
            return Clamp(Round(points / perStep) + min, min, max);
        }

        static int Spread(float points, int min, int max, float perStep)
        {
            int ceiling = Fixed(points, min, max, perStep);
            if (ceiling <= min)
            {
                return min;
            }

            return min + Rand.Range(0, ceiling - min + 1);
        }

        static int Round(float value)
        {
            return (int)(value >= 0f ? value + 0.5f : value - 0.5f);
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
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
