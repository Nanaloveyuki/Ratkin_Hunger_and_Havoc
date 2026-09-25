using System;
using System.Collections.Generic;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class RHAH_IncidentSchedule
    {
        internal const float DefaultDays = 15f;
        internal const float MinDays = 0f;
        internal const float MaxDays = 60f;
        internal const float GraceDays = 1f;
        internal const float MinWindowDays = 5f;
        internal const int ChecksPerDay = 60;

        internal static float OccurrenceChance(float days)
        {
            days = ClampDays(days);
            if (days <= 0f)
            {
                return 0f;
            }

            return RimWorld.Storyteller.CheckInterval / (days * GenDate.TicksPerDay);
        }

        internal static float DailyOccurrenceChance(float averageDays)
        {
            float perCheck = OccurrenceChance(averageDays);
            if (perCheck <= 0f)
            {
                return 0f;
            }

            return 1f - (float)Math.Pow(1d - perCheck, ChecksPerDay);
        }

        internal static float ClampWindowDays(float days)
        {
            if (float.IsNaN(days) || days < MinWindowDays)
            {
                return MinWindowDays;
            }

            if (days > MaxDays)
            {
                return MaxDays;
            }

            return days;
        }

        internal static float ClampDays(float days)
        {
            if (float.IsNaN(days) || days < MinDays)
            {
                return DefaultDays;
            }

            if (days > MaxDays)
            {
                return MaxDays;
            }

            return days;
        }

        internal static void QueueDueIncidents(float daysPassed, int trust, RHAH_IncidentSeason season, int checkIntervalTicks)
        {
            if (!RHAH_Runtime.AllowsNewContent || daysPassed <= GraceDays || Current.Game == null)
            {
                return;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings == null)
            {
                return;
            }

            GameComponent_RHAH_Game game = Current.Game.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                return;
            }

            float positiveDays = ClampDays(settings.positiveIncidentDays);
            float negativeDays = ClampDays(settings.negativeIncidentDays);
            List<IIncidentTarget> targets = Find.Storyteller?.AllIncidentTargets;
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                IIncidentTarget target = targets[i];
                if (target == null)
                {
                    continue;
                }

                bool mapHome = target is Map map && map.IsPlayerHome;
                bool playerCaravan = target is RimWorld.Planet.Caravan;
                if (!mapHome && !playerCaravan)
                {
                    continue;
                }

                TryQueue(game, target, RHAH_AttitudePool.Positive, positiveDays, trust, season, mapHome, playerCaravan, checkIntervalTicks);
                TryQueue(game, target, RHAH_AttitudePool.Negative, negativeDays, trust, season, mapHome, playerCaravan, checkIntervalTicks);
            }
        }

        static void TryQueue(
            GameComponent_RHAH_Game game,
            IIncidentTarget target,
            RHAH_AttitudePool pool,
            float days,
            int trust,
            RHAH_IncidentSeason season,
            bool mapHome,
            bool playerCaravan,
            int checkIntervalTicks)
        {
            if (days <= 0f || !Rand.MTBEventOccurs(days, GenDate.TicksPerDay, checkIntervalTicks))
            {
                return;
            }

            float total = TotalWeight(pool, trust, season, mapHome, playerCaravan);
            if (total <= 0f)
            {
                return;
            }

            string displayId = Select(pool, trust, season, mapHome, playerCaravan, Rand.Range(0f, total));
            if (displayId != null)
            {
                game.QueueIncident(displayId, PointsFor(displayId), TargetId(playerCaravan, RawId(target)));
            }
        }

        internal static float TotalWeight(
            RHAH_AttitudePool pool,
            int trust,
            RHAH_IncidentSeason season,
            bool mapHome,
            bool playerCaravan)
        {
            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                total += WeightOf(entries[i], pool, trust, season, mapHome, playerCaravan);
            }

            return total;
        }

        internal static string Select(
            RHAH_AttitudePool pool,
            int trust,
            RHAH_IncidentSeason season,
            bool mapHome,
            bool playerCaravan,
            float roll)
        {
            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            float[] weights = new float[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                weights[i] = WeightOf(entries[i], pool, trust, season, mapHome, playerCaravan);
            }

            int index = RHAH_IncidentWeight.Select(weights, roll);
            return index < 0 ? null : entries[index].DisplayId;
        }

        static float WeightOf(
            RHAH_IncidentEntry entry,
            RHAH_AttitudePool pool,
            int trust,
            RHAH_IncidentSeason season,
            bool mapHome,
            bool playerCaravan)
        {
            if (entry.DefaultAttitudePool != pool)
            {
                return 0f;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            RHAH_WeightFactors factors = settings == null ? RHAH_WeightFactors.Defaults() : settings.WeightFactors();
            float formula = RHAH_IncidentWeight.Evaluate(new RHAH_IncidentWeightInput(
                entry.Family,
                entry.Category,
                entry.Target,
                entry.DefaultAttitudePool,
                season,
                trust,
                mapHome,
                playerCaravan), factors);
            bool enabled = settings == null || settings.IsIncidentEnabled(entry.DisplayId);
            float playerWeight = settings == null
                ? RHAH_IncidentTuning.DefaultWeight
                : settings.IncidentWeight(entry.DisplayId);
            return RHAH_IncidentTuning.Scale(formula, enabled, playerWeight);
        }

        internal static RHAH_IncidentSeason SeasonOf(IIncidentTarget target)
        {
            if (target == null || !target.Tile.Valid)
            {
                return RHAH_IncidentSeason.Undefined;
            }

            switch (GenLocalDate.Season(target.Tile))
            {
                case Season.Spring: return RHAH_IncidentSeason.Spring;
                case Season.Summer: return RHAH_IncidentSeason.Summer;
                case Season.Fall: return RHAH_IncidentSeason.Fall;
                case Season.Winter: return RHAH_IncidentSeason.Winter;
                default: return RHAH_IncidentSeason.Undefined;
            }
        }

        internal const int UnspecifiedTargetId = 0;

        internal static int RawId(IIncidentTarget target)
        {
            if (target is RimWorld.Planet.Caravan caravan)
            {
                return caravan.ID;
            }

            if (target is Map map)
            {
                return map.uniqueID;
            }

            return UnspecifiedTargetId;
        }

        static float PointsFor(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            float catalog = entry == null ? RHAH_IncidentTuning.MinDebugPoints : entry.DebugPoints;
            return RHAH_Mod.Settings == null
                ? catalog
                : RHAH_Mod.Settings.IncidentDebugPoints(displayId, catalog);
        }

        internal static int TargetId(bool caravan, int rawId)
        {
            if (rawId <= 0)
            {
                return UnspecifiedTargetId;
            }

            return caravan ? -rawId : rawId;
        }

        internal static int DecodeTargetId(int savedId, RHAH_IncidentTarget kind)
        {
            if (savedId == UnspecifiedTargetId)
            {
                return UnspecifiedTargetId;
            }

            if (kind == RHAH_IncidentTarget.Caravan)
            {
                return savedId < 0 ? -savedId : UnspecifiedTargetId;
            }

            return savedId > 0 ? savedId : UnspecifiedTargetId;
        }

        internal static int NextExecutable(bool[] terminal, bool[] ready)
        {
            if (terminal == null || ready == null)
            {
                return -1;
            }

            int count = terminal.Length < ready.Length ? terminal.Length : ready.Length;
            for (int i = 0; i < count; i++)
            {
                if (terminal[i] || ready[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(RimWorld.Storyteller), nameof(RimWorld.Storyteller.StorytellerTick))]
    internal static class RHAH_IncidentSchedulePatch
    {
        internal static void Postfix()
        {
            if (Find.TickManager == null || Find.TickManager.TicksGame % RimWorld.Storyteller.CheckInterval != 0)
            {
                return;
            }

            int trust = Current.Game?.GetComponent<Narrative.NarrativeState>()?.Snapshot().Trust ?? 0;
            RHAH_IncidentSeason season = RHAH_IncidentSchedule.SeasonOf(Find.AnyPlayerHomeMap);
            RHAH_IncidentSchedule.QueueDueIncidents(
                GenDate.DaysPassedSinceSettleFloat,
                trust,
                season,
                RimWorld.Storyteller.CheckInterval);
        }
    }
}
