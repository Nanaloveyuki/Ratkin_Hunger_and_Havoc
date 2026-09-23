using System.Collections.Generic;
using HungerAndHavoc.Api;
using RimWorld;
using UnityEngine;
using Verse;

namespace HungerAndHavoc.Identity
{
    internal static class RHAH_Plague
    {
        internal const float SeverityMin = 0f;
        internal const float SeverityMax = 0.1f;
        internal const float SpreadChancePerCarrier = 0.005f;
        internal const float SpreadChanceCap = 0.30f;
        internal const float BloodPumpingSkipPercent = 120f;
        internal const int SpreadHour = 6;
        internal const int CheckIntervalTicks = 2500;
        internal const int SpreadDayInterval = 3;
        internal const int ReturnDelayDays = 15;
        internal const int ReturnStayDays = 1;

        internal static HediffDef Def =>
            DefDatabase<HediffDef>.GetNamedSilentFail("RHAH_Plague");

        internal static bool HasActive(Verse.Pawn pawn)
        {
            HediffDef def = Def;
            return def != null &&
                   pawn?.health?.hediffSet?.HasHediff(def) == true;
        }

        // 已有同病时不新增，也不把严重度往下压
        internal static bool TryInfect(Verse.Pawn pawn, float? severity = null)
        {
            HediffDef def = Def;
            if (pawn?.health == null || def == null)
            {
                return false;
            }

            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(def);
            if (existing != null)
            {
                if (severity.HasValue)
                {
                    existing.Severity = Mathf.Max(existing.Severity, severity.Value);
                }

                return false;
            }

            Hediff hediff = HediffMaker.MakeHediff(def, pawn);
            hediff.Severity = severity ?? Rand.Range(SeverityMin, SeverityMax);
            pawn.health.AddHediff(hediff);
            return true;
        }

        internal static void InfectCarrier(Verse.Pawn pawn, bool carriesPlague)
        {
            if (carriesPlague)
            {
                TryInfect(pawn, null);
            }
        }

        internal static bool IsSpreadDay(int absoluteDay, int lastSpreadDay, int intervalDays)
        {
            int interval = intervalDays < 1 ? 1 : intervalDays;
            return absoluteDay % interval == 0 && absoluteDay != lastSpreadDay;
        }

        internal static bool IsSpreadDay(int absoluteDay, int lastSpreadDay)
        {
            return IsSpreadDay(absoluteDay, lastSpreadDay, SpreadDayInterval);
        }

        internal static float SpreadChance(int carrierCount, float perCarrier, float cap)
        {
            if (carrierCount <= 0 || perCarrier <= 0f || cap <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(perCarrier * carrierCount, cap);
        }

        internal static float SpreadChance(int carrierCount)
        {
            return SpreadChance(carrierCount, SpreadChancePerCarrier, SpreadChanceCap);
        }

        internal static bool SkipsBloodPumping(float bloodPumpingPercent, float skipPercent)
        {
            return bloodPumpingPercent >= skipPercent;
        }

        internal static bool SkipsBloodPumping(float bloodPumpingPercent)
        {
            return SkipsBloodPumping(bloodPumpingPercent, BloodPumpingSkipPercent);
        }


        internal static bool IsCarrier(Verse.Pawn pawn, bool isRatkin)
        {
            return pawn != null &&
                   !pawn.Dead &&
                   pawn.Spawned &&
                   isRatkin &&
                   HasActive(pawn);
        }

        internal static bool CanReceive(Verse.Pawn pawn, float bloodPumpingPercent, float skipPercent)
        {
            return pawn != null &&
                   !pawn.Dead &&
                   pawn.IsFreeColonist &&
                   pawn.health?.capacities != null &&
                   !HasActive(pawn) &&
                   !SkipsBloodPumping(bloodPumpingPercent, skipPercent);
        }

        internal static bool CanReceive(Verse.Pawn pawn, float bloodPumpingPercent)
        {
            return CanReceive(pawn, bloodPumpingPercent, BloodPumpingSkipPercent);
        }

        internal static PlagueTally ResolveQuarantine(PlagueWatch watch)
        {
            int recovered = 0;
            int died = 0;
            if (watch?.Entries == null)
            {
                return new PlagueTally(recovered, died);
            }

            for (int i = 0; i < watch.Entries.Count; i++)
            {
                PlagueWatchEntry entry = watch.Entries[i];
                if (entry == null || entry.Counted)
                {
                    continue;
                }

                PlagueOutcome outcome = Classify(entry);
                if (outcome == PlagueOutcome.Keep)
                {
                    continue;
                }

                entry.Counted = true;
                if (outcome == PlagueOutcome.Recovered)
                {
                    recovered++;
                }
                else if (outcome == PlagueOutcome.Died)
                {
                    died++;
                }
            }

            return new PlagueTally(recovered, died);
        }

        internal static PlagueOutcome Classify(PlagueWatchEntry entry)
        {
            if (entry == null || entry.Keep)
            {
                return PlagueOutcome.Keep;
            }

            if (entry.Dead && entry.StillSick)
            {
                return PlagueOutcome.Died;
            }

            if (entry.Missing || (entry.StillSick && !entry.Dead))
            {
                return PlagueOutcome.Released;
            }


            if (!entry.Dead && !entry.StillSick)
            {
                return PlagueOutcome.Recovered;
            }

            return PlagueOutcome.Released;
        }


        internal static bool QuarantineOpen(IList<int> loadIds)
        {
            return loadIds != null && loadIds.Count > 0;
        }

        internal static bool IsQuarantined(IList<int> loadIds, int pawnLoadId)
        {
            if (pawnLoadId <= 0 || loadIds == null)
            {
                return false;
            }

            for (int i = 0; i < loadIds.Count; i++)
            {
                if (loadIds[i] == pawnLoadId)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool BlocksGate(RHAH_BehaviorGate gate, bool quarantined, bool blockJoin)
        {
            if (!quarantined || !blockJoin)
            {
                return false;
            }

            return gate == RHAH_BehaviorGate.JoinColony ||
                   gate == RHAH_BehaviorGate.Hire ||
                   gate == RHAH_BehaviorGate.Transfer;
        }

        internal static bool BlocksGate(RHAH_BehaviorGate gate, bool quarantined)
        {
            return BlocksGate(gate, quarantined, true);
        }

        internal static int ChooseReturn(int alreadyReturnedLoadId, IList<int> recoveredLoadIds)
        {
            if (alreadyReturnedLoadId != 0 || recoveredLoadIds == null)
            {
                return 0;
            }

            for (int i = 0; i < recoveredLoadIds.Count; i++)
            {
                if (recoveredLoadIds[i] > 0)
                {
                    return recoveredLoadIds[i];
                }
            }

            return 0;
        }

    }

    internal sealed class PlagueWatchEntry
    {
        internal int LoadId;
        internal bool Dead;
        internal bool LeftMap;
        internal bool StillSick;
        internal bool Keep;
        internal bool Missing;
        internal bool Counted;
    }



    internal enum PlagueOutcome
    {
        Keep,
        Recovered,
        Died,
        Released
    }

    internal sealed class PlagueWatch
    {
        internal List<PlagueWatchEntry> Entries = new List<PlagueWatchEntry>();
        internal List<int> RecoveredLoadIds = new List<int>();
    }


    internal readonly struct PlagueTally
    {
        internal readonly int Recovered;
        internal readonly int Died;

        internal PlagueTally(int recovered, int died)
        {
            Recovered = recovered;
            Died = died;
        }
    }
}
