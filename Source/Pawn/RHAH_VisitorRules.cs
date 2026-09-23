using HungerAndHavoc.Api;

namespace HungerAndHavoc.Pawn
{
    internal enum RHAH_StayKind
    {
        None = 0,
        Shelter = 1,
        Hire = 2
    }

    // 人数、年龄、停留和雇佣期限 不碰存档
    internal static class RHAH_VisitorRules
    {
        internal const int MinEventPawns = 1;
        internal const int DefaultEventPawns = 30;
        internal const int MaxEventPawns = 100;
        internal const float MinGeneratedAge = 0f;
        internal const float DefaultMinGeneratedAge = 0f;
        internal const float DefaultMaxGeneratedAge = 50f;
        internal const float MaxGeneratedAge = 100f;
        internal const float DefaultFedStayDays = 0.5f;
        internal const float DefaultNoFoodWaitDays = 0.5f;
        internal const float MaxStayDays = 5f;
        internal const float MinStayFraction = 0.5f;
        internal const float MaxStayFraction = 1.5f;
        internal const int MinShelterDays = 5;
        internal const int DefaultShelterDays = 5;
        internal const int MaxShelterDays = 60;
        internal const int MinHireDays = 5;
        internal const int DefaultHireDays = 60;
        internal const int MaxHireDays = 600;
        internal const int TicksPerDay = 60000;
        internal const float DefaultReliefScoreBonus = 0.1f;
        internal const float DefaultMinimumTemperature = -35f;
        internal const float DefaultMaximumTemperature = 70f;
        internal const float MinimumTemperatureBound = -35f;
        internal const float MaximumTemperatureBound = 70f;

        internal static int LimitCount(int requested, int minimum, int cap, bool keepTogether)
        {
            int safeMinimum = minimum < 1 ? 1 : minimum;
            int safeRequested = requested < safeMinimum ? safeMinimum : requested;
            if (keepTogether)
            {
                return safeRequested;
            }

            int safeCap = ClampCount(cap);
            if (safeCap < safeMinimum)
            {
                return safeRequested;
            }

            return safeRequested < safeCap ? safeRequested : safeCap;
        }

        internal static int ClampCount(int cap)
        {
            if (cap < MinEventPawns)
            {
                return MinEventPawns;
            }

            return cap > MaxEventPawns ? MaxEventPawns : cap;
        }

        internal static bool KeepsAge(RHAH_PawnRole role, bool fixedAge)
        {
            return fixedAge ||
                role == RHAH_PawnRole.RatkinYoung ||
                role == RHAH_PawnRole.Mother ||
                role == RHAH_PawnRole.BeggarMother;
        }

        internal static float? GenerationAge(RHAH_PawnRole role, float? fixedAge, float minAge, float maxAge, float roll)
        {
            if (KeepsAge(role, fixedAge.HasValue))
            {
                return fixedAge;
            }

            float lower = ClampAge(float.IsNaN(minAge) ? DefaultMinGeneratedAge : minAge);
            float upper = ClampAge(float.IsNaN(maxAge) ? DefaultMaxGeneratedAge : maxAge);
            if (upper < lower)
            {
                float swap = lower;
                lower = upper;
                upper = swap;
            }

            float span = upper - lower;
            float safeRoll = roll < 0f || float.IsNaN(roll) ? 0f : roll > 1f ? 1f : roll;
            return lower + span * safeRoll;
        }

        internal static float ClampAge(float age)
        {
            if (float.IsNaN(age) || float.IsInfinity(age) || age < MinGeneratedAge)
            {
                return MinGeneratedAge;
            }

            return age > MaxGeneratedAge ? MaxGeneratedAge : age;
        }

        internal static float Score(float distanceScore, float suitability, bool inRelief, float bonus)
        {
            if (float.IsNaN(distanceScore) || float.IsInfinity(distanceScore))
            {
                return float.NegativeInfinity;
            }

            float fit = float.IsNaN(suitability) || float.IsInfinity(suitability) ? 0f : suitability;
            float total = distanceScore + fit;
            if (!inRelief)
            {
                return total;
            }

            float safeBonus = ClampBonus(bonus);
            return total + Abs(total) * safeBonus;
        }

        internal static float ClampBonus(float bonus)
        {
            if (float.IsNaN(bonus) || float.IsInfinity(bonus) || bonus < 0f)
            {
                return 0f;
            }

            return bonus > 1f ? 1f : bonus;
        }

        internal static int FedStayTicks(float days, float roll)
        {
            float safeDays = ClampDays(days, DefaultFedStayDays);
            float safeRoll = roll < 0f || float.IsNaN(roll) ? MinStayFraction : roll > 1f ? 1f : roll;
            float fraction = MinStayFraction + (MaxStayFraction - MinStayFraction) * safeRoll;
            return (int)(safeDays * TicksPerDay * fraction);
        }

        internal static int WaitTicks(bool wait, float days)
        {
            if (!wait)
            {
                return 0;
            }

            return (int)(ClampDays(days, DefaultNoFoodWaitDays) * TicksPerDay);
        }

        internal static int NextWaitTick(int now, int previousDeadline, bool foundFood, bool wait, float days)
        {
            if (!wait)
            {
                return -1;
            }

            if (foundFood || previousDeadline < 0)
            {
                return now + WaitTicks(true, days);
            }

            return previousDeadline;
        }

        internal static bool FedLeaveDue(bool leaveEnabled, bool hasBeenFed, int now, int leaveTick, bool downed)
        {
            return leaveEnabled && hasBeenFed && !downed && leaveTick >= 0 && now >= leaveTick;
        }

        internal static int BeginStay(int now, RHAH_StayKind kind, int shelterDays, int hireDays)
        {
            int days = kind == RHAH_StayKind.Hire
                ? ClampHireDays(hireDays)
                : ClampShelterDays(shelterDays);
            return now + days * TicksPerDay;
        }

        internal static int ResumeStay(int now, int savedDeadline, int savedRemaining, bool downed)
        {
            if (downed)
            {
                return savedDeadline;
            }

            if (savedRemaining > 0)
            {
                return now + savedRemaining;
            }

            return savedDeadline;
        }

        internal static bool StayExpired(int now, int deadline, bool downed)
        {
            return !downed && deadline >= 0 && now >= deadline;
        }

        internal static int RemainingStay(int now, int deadline, bool downed, int savedRemaining)
        {
            if (downed)
            {
                return savedRemaining > 0 ? savedRemaining : deadline > now ? deadline - now : 0;
            }

            if (deadline < 0)
            {
                return 0;
            }

            return deadline > now ? deadline - now : 0;
        }

        internal static bool ClearsTrade(RHAH_ReleaseReason reason, bool letterBatch)
        {
            return letterBatch ||
                reason == RHAH_ReleaseReason.Recruited ||
                reason == RHAH_ReleaseReason.JoinedPlayerFaction ||
                reason == RHAH_ReleaseReason.Imprisoned ||
                reason == RHAH_ReleaseReason.Enslaved;
        }

        internal static bool LiftsAgeImmobility(bool toddlersActive, bool eventBaby, bool ageDowned, bool injuredDowned)
        {
            return !toddlersActive && eventBaby && ageDowned && !injuredDowned;
        }

        internal static bool AcceptsStone(bool stoneChunk, bool slag)
        {
            return stoneChunk && !slag;
        }

        internal static bool TemperatureAllows(float temperature, float minimum, float maximum, bool caravan)
        {
            if (caravan || float.IsNaN(temperature) || float.IsInfinity(temperature))
            {
                return caravan || float.IsNaN(temperature) || float.IsInfinity(temperature);
            }

            float lower = ClampTemperature(minimum, DefaultMinimumTemperature);
            float upper = ClampTemperature(maximum, DefaultMaximumTemperature);
            if (upper < lower)
            {
                float swap = lower;
                lower = upper;
                upper = swap;
            }

            return temperature >= lower && temperature <= upper;
        }

        internal static bool WantsColdClothes(bool enabled, float temperature, float comfortableMinimum)
        {
            return enabled && !float.IsNaN(temperature) && temperature < comfortableMinimum;
        }

        internal static int ClampShelterDays(int days)
        {
            if (days < MinShelterDays)
            {
                return MinShelterDays;
            }

            return days > MaxShelterDays ? MaxShelterDays : days;
        }

        internal static int ClampHireDays(int days)
        {
            if (days < MinHireDays)
            {
                return MinHireDays;
            }

            return days > MaxHireDays ? MaxHireDays : days;
        }

        static float ClampDays(float days, float fallback)
        {
            if (float.IsNaN(days) || float.IsInfinity(days))
            {
                return fallback;
            }

            if (days < 0f)
            {
                return 0f;
            }

            return days > MaxStayDays ? MaxStayDays : days;
        }

        static float ClampTemperature(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return fallback;
            }

            if (value < MinimumTemperatureBound)
            {
                return MinimumTemperatureBound;
            }

            return value > MaximumTemperatureBound ? MaximumTemperatureBound : value;
        }

        static float Abs(float value)
        {
            return value < 0f ? -value : value;
        }
    }
}
