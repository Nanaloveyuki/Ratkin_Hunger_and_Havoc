using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Data
{
    internal readonly struct HungerContentQuery
    {
        internal HungerContentQuery(
            float ageYears,
            bool adult,
            bool female,
            float? temperature,
            string incidentDisplayId,
            bool historiesEnabled,
            bool traitsEnabled,
            Func<string, bool> historyEnabled,
            Func<string, bool> traitEnabled,
            Func<string, float> traitWeight)
        {
            AgeYears = ageYears;
            Adult = adult;
            Female = female;
            Temperature = temperature;
            IncidentDisplayId = incidentDisplayId;
            HistoriesEnabled = historiesEnabled;
            TraitsEnabled = traitsEnabled;
            HistoryEnabled = historyEnabled ?? (_ => true);
            TraitEnabled = traitEnabled ?? (_ => true);
            TraitWeight = traitWeight ?? (id => HungerContentCatalog.DefaultTraitWeight(id));
        }

        internal float AgeYears { get; }
        internal bool Adult { get; }
        internal bool Female { get; }
        internal float? Temperature { get; }
        internal string IncidentDisplayId { get; }
        internal bool HistoriesEnabled { get; }
        internal bool TraitsEnabled { get; }
        internal Func<string, bool> HistoryEnabled { get; }
        internal Func<string, bool> TraitEnabled { get; }
        internal Func<string, float> TraitWeight { get; }
    }

    internal static class HungerContentSelector
    {
        internal static HungerHistoryRecord SelectHistory(HungerContentQuery query, Func<float> random)
        {
            if (!query.HistoriesEnabled || random == null)
            {
                return null;
            }

            List<HungerHistoryRecord> candidates = new List<HungerHistoryRecord>();
            IReadOnlyList<HungerHistoryRecord> all = HungerContentCatalog.Histories;
            for (int i = 0; i < all.Count; i++)
            {
                HungerHistoryRecord record = all[i];
                if (query.HistoryEnabled(record.DisplayId) && MatchesHistory(record, query, true))
                {
                    candidates.Add(record);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            if (query.Adult)
            {
                return Weighted(candidates, record => record.Weight, random);
            }

            List<HungerHistoryRecord> adults = new List<HungerHistoryRecord>();
            for (int i = 0; i < all.Count; i++)
            {
                HungerHistoryRecord record = all[i];
                if (record.Slot == HungerHistorySlot.Adulthood &&
                    query.HistoryEnabled(record.DisplayId) &&
                    MatchesHistory(record, query, false))
                {
                    adults.Add(record);
                }
            }

            if (adults.Count == 0)
            {
                return Weighted(candidates, record => record.Weight, random);
            }

            List<HungerHistoryRecord> linked = new List<HungerHistoryRecord>();
            for (int i = 0; i < candidates.Count; i++)
            {
                if (LinksAny(candidates[i], adults))
                {
                    linked.Add(candidates[i]);
                }
            }

            if (linked.Count == 0)
            {
                return Weighted(candidates, record => record.Weight, random);
            }

            return Weighted(linked, record =>
            {
                float adultWeight = 0f;
                for (int i = 0; i < adults.Count; i++)
                {
                    if (Links(record, adults[i].DisplayId))
                    {
                        adultWeight += adults[i].Weight;
                    }
                }

                float prior = adultWeight < 0.001f ? 0.001f : adultWeight;
                return record.Weight * prior;
            }, random);
        }

        internal static HungerTraitRecord SelectTrait(
            HungerContentQuery query,
            string historyDisplayId,
            Func<float> random,
            Func<HungerTraitRecord, bool> gain)
        {
            if (!query.TraitsEnabled || random == null || gain == null)
            {
                return null;
            }

            List<HungerTraitRecord> slot = CollectSlot(query);
            if (slot.Count == 0)
            {
                return null;
            }

            List<HungerTraitRecord> history = MatchingHistory(slot, historyDisplayId);
            if (history.Count > 0)
            {
                return DrawTrait(history, query, random, gain);
            }

            List<HungerTraitRecord> incidents = MatchingIncident(slot, query.IncidentDisplayId);
            if (incidents.Count > 0)
            {
                return DrawTrait(incidents, query, random, gain);
            }

            return DrawTrait(slot, query, random, gain);
        }

        internal static bool CategoryMatches(HungerContentCategory category, string incidentDisplayId)
        {
            if (category == HungerContentCategory.None || string.IsNullOrEmpty(incidentDisplayId))
            {
                return false;
            }

            int number;
            if (incidentDisplayId.Length != 5 || incidentDisplayId[0] != 'I' || incidentDisplayId[1] != '-' ||
                !int.TryParse(incidentDisplayId.Substring(2), out number))
            {
                return false;
            }

            bool theft = incidentDisplayId == "I-006" || incidentDisplayId == "I-007" || incidentDisplayId == "I-043";
            bool siege = incidentDisplayId == "I-014" || incidentDisplayId == "I-030" || incidentDisplayId == "I-045";
            bool plague = number >= 36 && number <= 50;
            if (category == HungerContentCategory.Plague)
            {
                return plague;
            }

            if (category == HungerContentCategory.Theft)
            {
                return theft;
            }

            if (category == HungerContentCategory.Conflict)
            {
                return theft || siege;
            }

            return category == HungerContentCategory.Siege && siege;
        }

        static List<HungerTraitRecord> CollectSlot(HungerContentQuery query)
        {
            List<HungerTraitRecord> slot = new List<HungerTraitRecord>();
            IReadOnlyList<HungerTraitRecord> all = HungerContentCatalog.Traits;
            for (int i = 0; i < all.Count; i++)
            {
                HungerTraitRecord record = all[i];
                if (!MatchesTraitSlot(record, query.Adult) || !query.TraitEnabled(record.DisplayId))
                {
                    continue;
                }

                if (query.TraitWeight(record.DisplayId) <= 0f)
                {
                    continue;
                }

                slot.Add(record);
            }

            return slot;
        }

        static bool MatchesHistory(HungerHistoryRecord record, HungerContentQuery query, bool requireSlot)
        {
            if (requireSlot && (record.Slot == HungerHistorySlot.Adulthood) != query.Adult)
            {
                return false;
            }

            if (record.MinimumAge.HasValue)
            {
                bool below = record.MinimumInclusive
                    ? query.AgeYears < record.MinimumAge.Value
                    : query.AgeYears <= record.MinimumAge.Value;
                if (below)
                {
                    return false;
                }
            }

            if (record.MaximumAge.HasValue)
            {
                bool above = record.MaximumInclusive
                    ? query.AgeYears > record.MaximumAge.Value
                    : query.AgeYears >= record.MaximumAge.Value;
                if (above)
                {
                    return false;
                }
            }

            if (record.Gender == HungerContentGender.Female && !query.Female)
            {
                return false;
            }

            if (query.Temperature.HasValue &&
                (query.Temperature.Value < record.MinimumTemperature - 0.001f ||
                 query.Temperature.Value > record.MaximumTemperature + 0.001f))
            {
                return false;
            }

            return MatchesIncident(record.IncidentDisplayIds, record.Category, query.IncidentDisplayId);
        }

        static bool MatchesIncident(
            IReadOnlyList<string> incidentDisplayIds,
            HungerContentCategory category,
            string incidentDisplayId)
        {
            bool constrained = incidentDisplayIds.Count > 0 || category != HungerContentCategory.None;
            if (!constrained)
            {
                return true;
            }

            if (string.IsNullOrEmpty(incidentDisplayId))
            {
                return false;
            }

            for (int i = 0; i < incidentDisplayIds.Count; i++)
            {
                if (string.Equals(incidentDisplayIds[i], incidentDisplayId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return CategoryMatches(category, incidentDisplayId);
        }

        static bool MatchesTraitSlot(HungerTraitRecord record, bool adult)
        {
            if (record.Slot == HungerTraitSlot.Any)
            {
                return true;
            }

            return adult
                ? record.Slot == HungerTraitSlot.Adulthood
                : record.Slot == HungerTraitSlot.Childhood;
        }

        static List<HungerTraitRecord> MatchingHistory(List<HungerTraitRecord> slot, string historyDisplayId)
        {
            List<HungerTraitRecord> matched = new List<HungerTraitRecord>();
            if (string.IsNullOrEmpty(historyDisplayId))
            {
                return matched;
            }

            for (int i = 0; i < slot.Count; i++)
            {
                IReadOnlyList<string> histories = slot[i].HistoryDisplayIds;
                for (int j = 0; j < histories.Count; j++)
                {
                    if (string.Equals(histories[j], historyDisplayId, StringComparison.OrdinalIgnoreCase))
                    {
                        matched.Add(slot[i]);
                        break;
                    }
                }
            }

            return matched;
        }

        static List<HungerTraitRecord> MatchingIncident(List<HungerTraitRecord> slot, string incidentDisplayId)
        {
            List<HungerTraitRecord> matched = new List<HungerTraitRecord>();
            for (int i = 0; i < slot.Count; i++)
            {
                if ((slot[i].IncidentDisplayIds.Count > 0 || slot[i].Category != HungerContentCategory.None) &&
                    MatchesIncident(slot[i].IncidentDisplayIds, slot[i].Category, incidentDisplayId))
                {
                    matched.Add(slot[i]);
                }
            }

            return matched;
        }

        static HungerTraitRecord DrawTrait(
            List<HungerTraitRecord> candidates,
            HungerContentQuery query,
            Func<float> random,
            Func<HungerTraitRecord, bool> gain)
        {
            List<HungerTraitRecord> remaining = new List<HungerTraitRecord>(candidates);
            int attempts = remaining.Count < 8 ? remaining.Count : 8;
            for (int attempt = 0; attempt < attempts && remaining.Count > 0; attempt++)
            {
                HungerTraitRecord selected = Weighted(remaining, record =>
                {
                    float weight = query.TraitWeight(record.DisplayId);
                    return weight < 0.001f ? 0.001f : weight;
                }, random);
                if (selected == null)
                {
                    return null;
                }

                float chance = query.TraitWeight(selected.DisplayId) / 100f;
                if (chance < 0f)
                {
                    chance = 0f;
                }
                else if (chance > 1f)
                {
                    chance = 1f;
                }

                if (random() >= chance)
                {
                    return null;
                }

                if (gain(selected))
                {
                    return selected;
                }

                remaining.Remove(selected);
            }

            return null;
        }

        static bool LinksAny(HungerHistoryRecord child, List<HungerHistoryRecord> adults)
        {
            for (int i = 0; i < adults.Count; i++)
            {
                if (Links(child, adults[i].DisplayId))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Links(HungerHistoryRecord child, string adultDisplayId)
        {
            for (int i = 0; i < child.LinkedAdultDisplayIds.Count; i++)
            {
                if (string.Equals(child.LinkedAdultDisplayIds[i], adultDisplayId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        static T Weighted<T>(List<T> items, Func<T, float> weight, Func<float> random) where T : class
        {
            float total = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                float value = weight(items[i]);
                if (value > 0f)
                {
                    total += value;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            float roll = random();
            if (roll < 0f)
            {
                roll = 0f;
            }
            else if (roll >= 1f)
            {
                roll = 0.999999f;
            }

            float cursor = roll * total;
            for (int i = 0; i < items.Count; i++)
            {
                float value = weight(items[i]);
                if (value <= 0f)
                {
                    continue;
                }

                cursor -= value;
                if (cursor <= 0f)
                {
                    return items[i];
                }
            }

            return items[items.Count - 1];
        }
    }
}
