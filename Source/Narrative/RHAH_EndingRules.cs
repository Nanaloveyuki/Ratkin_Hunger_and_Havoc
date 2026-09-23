namespace HungerAndHavoc.Narrative
{
    internal enum RHAH_EndingId
    {
        None = 0,
        E01 = 1,
        E02 = 2,
        E03 = 3,
        E04 = 4,
        E05 = 5
    }

    internal enum RHAH_IdentityTier
    {
        None = 0,
        Partial = 1,
        Full = 2
    }

    // 广义接济和广播成功次数由叙事账本写入 本文件只判定
    internal readonly struct RHAH_EndingFacts
    {
        internal readonly int Trust;
        internal readonly bool Narrator;
        internal readonly int Aid;
        internal readonly int Broadcasts;
        internal readonly int Expulsions;
        internal readonly int Adults;
        internal readonly int CompletedKinds;
        internal readonly bool RelicDone;
        internal readonly int WaitedDays;
        internal readonly bool E01Shown;
        internal readonly bool E02Shown;
        internal readonly bool E03Shown;
        internal readonly bool E04Shown;
        internal readonly bool E05Shown;
        internal readonly int IdentityTier;

        internal RHAH_EndingFacts(
            int trust,
            bool narrator,
            int aid,
            int broadcasts,
            int expulsions,
            int adults,
            int completedKinds,
            bool relicDone,
            int waitedDays,
            bool e01Shown,
            bool e02Shown,
            bool e03Shown,
            bool e04Shown,
            bool e05Shown,
            int identityTier)
        {
            Trust = trust;
            Narrator = narrator;
            Aid = aid;
            Broadcasts = broadcasts;
            Expulsions = expulsions;
            Adults = adults;
            CompletedKinds = completedKinds;
            RelicDone = relicDone;
            WaitedDays = waitedDays;
            E01Shown = e01Shown;
            E02Shown = e02Shown;
            E03Shown = e03Shown;
            E04Shown = e04Shown;
            E05Shown = e05Shown;
            IdentityTier = identityTier;
        }
    }

    internal readonly struct RHAH_EndingGoals
    {
        internal readonly int Aid;
        internal readonly int Broadcasts;
        internal readonly int ExpulsionLimit;
        internal readonly int Adults;
        internal readonly int WaitDays;
        internal readonly bool CountWithoutNarrator;
        internal readonly bool EndingsWithoutNarrator;
        internal readonly bool E01Enabled;
        internal readonly bool E02Enabled;
        internal readonly bool E03Enabled;
        internal readonly bool E04Enabled;
        internal readonly bool E05Enabled;
        internal readonly bool R01Enabled;

        internal RHAH_EndingGoals(
            int aid,
            int broadcasts,
            int expulsionLimit,
            int adults,
            int waitDays,
            bool countWithoutNarrator,
            bool endingsWithoutNarrator,
            bool e01Enabled,
            bool e02Enabled,
            bool e03Enabled,
            bool e04Enabled,
            bool e05Enabled,
            bool r01Enabled)
        {
            Aid = aid;
            Broadcasts = broadcasts;
            ExpulsionLimit = expulsionLimit;
            Adults = adults;
            WaitDays = waitDays;
            CountWithoutNarrator = countWithoutNarrator;
            EndingsWithoutNarrator = endingsWithoutNarrator;
            E01Enabled = e01Enabled;
            E02Enabled = e02Enabled;
            E03Enabled = e03Enabled;
            E04Enabled = e04Enabled;
            E05Enabled = e05Enabled;
            R01Enabled = r01Enabled;
        }

        internal static RHAH_EndingGoals Defaults()
        {
            return new RHAH_EndingGoals(99, 3, 3, 100, 30, true, true, true, true, true, true, true, true);
        }
    }

    internal static class RHAH_EndingRules
    {
        internal const int TrustFloor = 50;
        internal const int HopeTrust = 75;
        internal const int HaltTrust = -75;
        internal const int LowKindMinimum = 6;
        internal const float ThreatIntervalDays = 13f;
        internal enum RHAH_EndingEvent
        {
            None = 0,
            Aid = 1,
            Broadcast = 2,
            Expulsion = 3
        }

        internal static RHAH_EndingEvent FromChoice(Incidents.RHAH_ChoiceKind choice, Incidents.RHAH_ChoiceAction action)
        {
            if (action != Incidents.RHAH_ChoiceAction.Deliver)
            {
                return RHAH_EndingEvent.None;
            }

            if (choice == Incidents.RHAH_ChoiceKind.Aid ||
                choice == Incidents.RHAH_ChoiceKind.ChildExchange ||
                choice == Incidents.RHAH_ChoiceKind.Refugees ||
                choice == Incidents.RHAH_ChoiceKind.Abandoned ||
                choice == Incidents.RHAH_ChoiceKind.Kinship ||
                choice == Incidents.RHAH_ChoiceKind.Airdrop)
            {
                return RHAH_EndingEvent.Aid;
            }

            return RHAH_EndingEvent.None;
        }

        internal static bool CountsExpulsion(bool forcedAway, bool shifted)
        {
            return forcedAway && shifted;
        }

        internal static int ClampGoal(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        internal static int ClampAid(int value)
        {
            return ClampGoal(value, 1, 999);
        }

        internal static int ClampBroadcasts(int value)
        {
            return ClampGoal(value, 1, 99);
        }

        internal static int ClampExpulsions(int value)
        {
            return ClampGoal(value, 0, 99);
        }

        internal static int ClampAdults(int value)
        {
            return ClampGoal(value, 1, 500);
        }

        internal static int ClampWait(int value)
        {
            return ClampGoal(value, 0, 120);
        }

        internal static float ThreatFactor(int trust)
        {
            int clamped = SuiyinNodes.ClampTrust(trust);
            return 1f - clamped / 400f;
        }

        internal static float ThreatInterval(int trust)
        {
            float factor = ThreatFactor(trust);
            if (factor <= 0f)
            {
                return ThreatIntervalDays;
            }

            return ThreatIntervalDays / factor;
        }

        internal static bool ScaleMet(RHAH_EndingFacts facts, RHAH_EndingGoals goals)
        {
            return facts.Aid >= goals.Aid && facts.Broadcasts >= goals.Broadcasts && facts.Expulsions <= goals.ExpulsionLimit;
        }

        internal static bool LowReady(RHAH_EndingFacts facts, RHAH_EndingGoals goals)
        {
            return facts.CompletedKinds >= LowKindMinimum && facts.WaitedDays >= goals.WaitDays;
        }

        internal static bool Shown(RHAH_EndingFacts facts, RHAH_EndingId id)
        {
            if (id == RHAH_EndingId.E01)
            {
                return facts.E01Shown;
            }

            if (id == RHAH_EndingId.E02)
            {
                return facts.E02Shown;
            }

            if (id == RHAH_EndingId.E03)
            {
                return facts.E03Shown;
            }

            if (id == RHAH_EndingId.E04)
            {
                return facts.E04Shown;
            }

            if (id == RHAH_EndingId.E05)
            {
                return facts.E05Shown;
            }

            return false;
        }

        internal static bool Enabled(RHAH_EndingGoals goals, RHAH_EndingId id)
        {
            if (id == RHAH_EndingId.E01)
            {
                return goals.E01Enabled;
            }

            if (id == RHAH_EndingId.E02)
            {
                return goals.E02Enabled;
            }

            if (id == RHAH_EndingId.E03)
            {
                return goals.E03Enabled;
            }

            if (id == RHAH_EndingId.E04)
            {
                return goals.E04Enabled;
            }

            if (id == RHAH_EndingId.E05)
            {
                return goals.E05Enabled;
            }

            return false;
        }

        // 先 E-02 再 E-01 再中低信任 低条件不挡住后来的高条件
        internal static RHAH_EndingId Next(RHAH_EndingFacts facts, RHAH_EndingGoals goals)
        {
            if (facts.Narrator || goals.EndingsWithoutNarrator)
            {
                bool scale = ScaleMet(facts, goals);
                if (scale && facts.Adults >= goals.Adults && (!facts.Narrator || facts.Trust >= HopeTrust) &&
                    goals.E02Enabled && !facts.E02Shown)
                {
                    return RHAH_EndingId.E02;
                }

                if (scale && (!facts.Narrator || facts.Trust >= TrustFloor) && goals.E01Enabled && !facts.E01Shown)
                {
                    return RHAH_EndingId.E01;
                }

                if (facts.Narrator && facts.Trust <= HaltTrust && facts.CompletedKinds > 0 &&
                    goals.E05Enabled && !facts.E05Shown)
                {
                    return RHAH_EndingId.E05;
                }

                if (LowReady(facts, goals))
                {
                    RHAH_EndingId low = facts.Narrator && facts.Trust < 0 ? RHAH_EndingId.E04 : RHAH_EndingId.E03;
                    if (low == RHAH_EndingId.E04 && facts.Trust < HaltTrust)
                    {
                        low = RHAH_EndingId.None;
                    }

                    if (low != RHAH_EndingId.None && Enabled(goals, low) && !Shown(facts, low))
                    {
                        return low;
                    }
                }
            }

            return RHAH_EndingId.None;
        }

        internal static bool AsidesClosed(int trust, bool e05Shown)
        {
            return e05Shown || SuiyinNodes.ClampTrust(trust) <= HaltTrust;
        }

        internal static bool IdentityDue(RHAH_EndingFacts facts, RHAH_EndingGoals goals)
        {
            if (!facts.Narrator || !goals.R01Enabled || facts.Trust < TrustFloor)
            {
                return false;
            }

            if (!facts.RelicDone && !facts.E01Shown && !facts.E02Shown)
            {
                return false;
            }

            int offered = facts.Trust >= HopeTrust ? (int)RHAH_IdentityTier.Full : (int)RHAH_IdentityTier.Partial;
            return facts.IdentityTier < offered;
        }

        internal static RHAH_IdentityTier IdentityOffer(int trust)
        {
            if (trust >= HopeTrust)
            {
                return RHAH_IdentityTier.Full;
            }

            if (trust >= TrustFloor)
            {
                return RHAH_IdentityTier.Partial;
            }

            return RHAH_IdentityTier.None;
        }
    }
}
