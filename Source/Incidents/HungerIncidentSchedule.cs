using System.Collections.Generic;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class HungerIncidentSchedule
    {
        internal const float DefaultDays = 15f;
        internal const float MinDays = 0f;
        internal const float MaxDays = 60f;
        internal const float GraceDays = 1f;
        internal const string Expression = "w = family x season x plague x trust x target";

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

        internal static void QueueDueIncidents(float daysPassed, int trust, HungerIncidentSeason season, int checkIntervalTicks)
        {
            if (!HungerAndHavocRuntime.AllowsNewContent || daysPassed <= GraceDays || Current.Game == null)
            {
                return;
            }

            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (settings == null)
            {
                return;
            }

            GameComponent_HungerAndHavoc game = Current.Game.GetComponent<GameComponent_HungerAndHavoc>();
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

                TryQueue(game, target, HungerAttitudePool.Positive, positiveDays, trust, season, mapHome, playerCaravan, checkIntervalTicks);
                TryQueue(game, target, HungerAttitudePool.Negative, negativeDays, trust, season, mapHome, playerCaravan, checkIntervalTicks);
            }
        }

        static void TryQueue(
            GameComponent_HungerAndHavoc game,
            IIncidentTarget target,
            HungerAttitudePool pool,
            float days,
            int trust,
            HungerIncidentSeason season,
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
                game.QueueIncident(displayId);
            }
        }

        internal static float TotalWeight(
            HungerAttitudePool pool,
            int trust,
            HungerIncidentSeason season,
            bool mapHome,
            bool playerCaravan)
        {
            IReadOnlyList<HungerIncidentEntry> entries = HungerIncidentCatalog.All;
            float total = 0f;
            for (int i = 0; i < entries.Count; i++)
            {
                total += WeightOf(entries[i], pool, trust, season, mapHome, playerCaravan);
            }

            return total;
        }

        internal static string Select(
            HungerAttitudePool pool,
            int trust,
            HungerIncidentSeason season,
            bool mapHome,
            bool playerCaravan,
            float roll)
        {
            IReadOnlyList<HungerIncidentEntry> entries = HungerIncidentCatalog.All;
            float[] weights = new float[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                weights[i] = WeightOf(entries[i], pool, trust, season, mapHome, playerCaravan);
            }

            int index = HungerIncidentWeight.Select(weights, roll);
            return index < 0 ? null : entries[index].DisplayId;
        }

        static float WeightOf(
            HungerIncidentEntry entry,
            HungerAttitudePool pool,
            int trust,
            HungerIncidentSeason season,
            bool mapHome,
            bool playerCaravan)
        {
            if (entry.DefaultAttitudePool != pool)
            {
                return 0f;
            }

            return HungerIncidentWeight.Evaluate(new HungerIncidentWeightInput(
                entry.Family,
                entry.Category,
                entry.Target,
                entry.DefaultAttitudePool,
                season,
                trust,
                mapHome,
                playerCaravan));
        }

        internal static HungerIncidentSeason SeasonOf(IIncidentTarget target)
        {
            if (target == null || !target.Tile.Valid)
            {
                return HungerIncidentSeason.Undefined;
            }

            switch (GenLocalDate.Season(target.Tile))
            {
                case Season.Spring: return HungerIncidentSeason.Spring;
                case Season.Summer: return HungerIncidentSeason.Summer;
                case Season.Fall: return HungerIncidentSeason.Fall;
                case Season.Winter: return HungerIncidentSeason.Winter;
                default: return HungerIncidentSeason.Undefined;
            }
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(Storyteller), nameof(Storyteller.StorytellerTick))]
    internal static class HungerIncidentSchedulePatch
    {
        internal static void Postfix()
        {
            if (Find.TickManager == null || Find.TickManager.TicksGame % Storyteller.CheckInterval != 0)
            {
                return;
            }

            int trust = Current.Game?.GetComponent<Narrative.NarrativeState>()?.Snapshot().Trust ?? 0;
            HungerIncidentSeason season = HungerIncidentSchedule.SeasonOf(Find.AnyPlayerHomeMap);
            HungerIncidentSchedule.QueueDueIncidents(
                GenDate.DaysPassedSinceSettleFloat,
                trust,
                season,
                Storyteller.CheckInterval);
        }
    }
}
