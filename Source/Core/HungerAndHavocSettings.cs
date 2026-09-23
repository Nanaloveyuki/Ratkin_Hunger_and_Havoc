using System;
using System.Collections.Generic;
using HungerAndHavoc.Generation;
using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocSettings : ModSettings
    {
        public bool enableNewContent = true;
        public bool optimizeGeneration = true;
        public float positiveIncidentDays = 15f;
        public float negativeIncidentDays = 15f;
        Dictionary<string, float> xenotypeWeights = new Dictionary<string, float>();
        List<string> enabledXenotypeDefNames = new List<string>();
        List<string> enabledGeneDefNames = new List<string>();
        public bool reliefEnabled = true;
        public bool allowEatOutsideRelief;
        public bool ignoreReliefAfterFed;
        public bool leaveAfterFed = true;
        List<string> disabledReliefFoodDefNames = new List<string>();
        public bool aidRequestsEnabled = true;
        public bool intelTradesEnabled = true;
        public bool visitorChoicesEnabled = true;
        public bool familyDropEnabled = true;
        public bool motherFeedEnabled = true;
        public bool prisonerScavengeEnabled = true;
        public bool tailBiteEnabled;
        public bool broadcastEnabled = true;
        public int broadcastCooldownDays = 3;
        public bool staggerGeneration = true;
        public bool refugeeCampEnabled = true;
        public bool pawnHistoriesEnabled = true;
        public bool pawnTraitsEnabled = true;
        List<string> disabledIncidentDisplayIds = new List<string>();
        Dictionary<string, float> incidentDebugPoints = new Dictionary<string, float>();
        Dictionary<string, float> incidentWeights = new Dictionary<string, float>();
        List<string> disabledHistoryDisplayIds = new List<string>();
        List<string> disabledTraitDisplayIds = new List<string>();
        Dictionary<string, float> traitWeights = new Dictionary<string, float>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableNewContent, "enableNewContent", true);
            Scribe_Values.Look(ref optimizeGeneration, "optimizeGeneration", true);
            Scribe_Values.Look(ref positiveIncidentDays, "positiveIncidentDays", 15f);
            Scribe_Values.Look(ref negativeIncidentDays, "negativeIncidentDays", 15f);
            Scribe_Collections.Look(ref xenotypeWeights, "xenotypeWeights", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref enabledXenotypeDefNames, "enabledXenotypeDefNames", LookMode.Value);
            Scribe_Collections.Look(ref enabledGeneDefNames, "enabledGeneDefNames", LookMode.Value);
            Scribe_Values.Look(ref reliefEnabled, "reliefEnabled", true);
            Scribe_Values.Look(ref allowEatOutsideRelief, "allowEatOutsideRelief", false);
            Scribe_Values.Look(ref ignoreReliefAfterFed, "ignoreReliefAfterFed", false);
            Scribe_Values.Look(ref leaveAfterFed, "leaveAfterFed", true);
            Scribe_Collections.Look(ref disabledReliefFoodDefNames, "disabledReliefFoodDefNames", LookMode.Value);
            Scribe_Values.Look(ref aidRequestsEnabled, "aidRequestsEnabled", true);
            Scribe_Values.Look(ref intelTradesEnabled, "intelTradesEnabled", true);
            Scribe_Values.Look(ref visitorChoicesEnabled, "visitorChoicesEnabled", true);
            Scribe_Values.Look(ref familyDropEnabled, "familyDropEnabled", true);
            Scribe_Values.Look(ref motherFeedEnabled, "motherFeedEnabled", true);
            Scribe_Values.Look(ref prisonerScavengeEnabled, "prisonerScavengeEnabled", true);
            Scribe_Values.Look(ref tailBiteEnabled, "tailBiteEnabled", false);
            Scribe_Values.Look(ref broadcastEnabled, "broadcastEnabled", true);
            Scribe_Values.Look(ref broadcastCooldownDays, "broadcastCooldownDays", 3);
            Scribe_Values.Look(ref staggerGeneration, "staggerGeneration", true);
            Scribe_Values.Look(ref refugeeCampEnabled, "refugeeCampEnabled", true);
            Scribe_Values.Look(ref pawnHistoriesEnabled, "pawnHistoriesEnabled", true);
            Scribe_Values.Look(ref pawnTraitsEnabled, "pawnTraitsEnabled", true);
            Scribe_Collections.Look(ref disabledIncidentDisplayIds, "disabledIncidentDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref incidentDebugPoints, "incidentDebugPoints", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref incidentWeights, "incidentWeights", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref disabledHistoryDisplayIds, "disabledHistoryDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref disabledTraitDisplayIds, "disabledTraitDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref traitWeights, "traitWeights", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                xenotypeWeights = xenotypeWeights ?? new Dictionary<string, float>();
                enabledXenotypeDefNames = enabledXenotypeDefNames ?? new List<string>();
                enabledGeneDefNames = enabledGeneDefNames ?? new List<string>();
                disabledReliefFoodDefNames = disabledReliefFoodDefNames ?? new List<string>();
                disabledIncidentDisplayIds = disabledIncidentDisplayIds ?? new List<string>();
                incidentDebugPoints = incidentDebugPoints ?? new Dictionary<string, float>();
                incidentWeights = incidentWeights ?? new Dictionary<string, float>();
                disabledHistoryDisplayIds = disabledHistoryDisplayIds ?? new List<string>();
                disabledTraitDisplayIds = disabledTraitDisplayIds ?? new List<string>();
                traitWeights = traitWeights ?? new Dictionary<string, float>();
                broadcastCooldownDays = HungerAndHavoc.Incidents.HungerBroadcastRules.ClampDays(broadcastCooldownDays);
                Normalize();
                positiveIncidentDays = HungerAndHavoc.Incidents.HungerIncidentSchedule.ClampDays(positiveIncidentDays);
                negativeIncidentDays = HungerAndHavoc.Incidents.HungerIncidentSchedule.ClampDays(negativeIncidentDays);
            }
        }

        public float XenotypeWeight(string defName)
        {
            EnsureCollections();
            return HungerXenotypeWeightTable.StoredOrDefault(xenotypeWeights, defName, HungerGeneCatalog.SuggestedWeight(defName));
        }

        public void SetXenotypeWeight(string defName, float value)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            EnsureCollections();
            xenotypeWeights[defName] = HungerXenotypeWeightTable.Clamp(value);
        }

        public void ResetXenotypeWeights()
        {
            EnsureCollections();
            xenotypeWeights.Clear();
        }

        public bool IsXenotypeEnabled(string defName)
        {
            EnsureCollections();
            return HungerGeneCatalog.IsBuiltin(defName) || enabledXenotypeDefNames.Contains(defName);
        }

        public void SetXenotypeEnabled(string defName, bool enabled)
        {
            if (string.IsNullOrEmpty(defName) || HungerGeneCatalog.IsBuiltin(defName))
            {
                return;
            }

            EnsureCollections();
            if (enabled)
            {
                if (!enabledXenotypeDefNames.Contains(defName))
                {
                    enabledXenotypeDefNames.Add(defName);
                }

                return;
            }

            enabledXenotypeDefNames.Remove(defName);
        }

        public bool IsGeneEnabled(string defName)
        {
            EnsureCollections();
            return enabledGeneDefNames.Contains(defName);
        }

        public void SetGeneEnabled(string defName, bool enabled)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            EnsureCollections();
            if (enabled)
            {
                if (!enabledGeneDefNames.Contains(defName))
                {
                    enabledGeneDefNames.Add(defName);
                }

                return;
            }

            enabledGeneDefNames.Remove(defName);
        }

        public List<string> EnabledXenotypeNames()
        {
            EnsureCollections();
            return enabledXenotypeDefNames;
        }

        public List<string> EnabledGeneNames()
        {
            EnsureCollections();
            return enabledGeneDefNames;
        }

        public List<string> MissingXenotypeNames()
        {
            EnsureCollections();
            List<string> missing = new List<string>();
            foreach (KeyValuePair<string, float> entry in xenotypeWeights)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    missing.Add(entry.Key);
                }
            }

            for (int i = 0; i < enabledXenotypeDefNames.Count; i++)
            {
                if (!missing.Contains(enabledXenotypeDefNames[i]))
                {
                    missing.Add(enabledXenotypeDefNames[i]);
                }
            }

            return missing;
        }

        public bool IsReliefFoodEnabled(string defName)
        {
            EnsureCollections();
            return string.IsNullOrEmpty(defName) || !disabledReliefFoodDefNames.Contains(defName);
        }

        public void SetReliefFoodEnabled(string defName, bool enabled)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            EnsureCollections();
            if (enabled)
            {
                disabledReliefFoodDefNames.Remove(defName);
            }
            else if (!disabledReliefFoodDefNames.Contains(defName))
            {
                disabledReliefFoodDefNames.Add(defName);
            }

            InvalidateReliefSearch();
        }

        public void SetAllReliefFood(bool enabled, List<string> candidates)
        {
            EnsureCollections();
            disabledReliefFoodDefNames.Clear();
            if (!enabled && candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (!string.IsNullOrEmpty(candidates[i]) && !disabledReliefFoodDefNames.Contains(candidates[i]))
                    {
                        disabledReliefFoodDefNames.Add(candidates[i]);
                    }
                }
            }

            InvalidateReliefSearch();
        }

        public void InvalidateReliefSearch()
        {
            if (Current.Game == null || Current.Game.Maps == null)
            {
                return;
            }

            for (int i = 0; i < Current.Game.Maps.Count; i++)
            {
                Map map = Current.Game.Maps[i];
                MapComponent_HungerAndHavoc component = map != null
                    ? map.GetComponent<MapComponent_HungerAndHavoc>()
                    : null;
                if (component != null)
                {
                    component.InvalidateFoodSearch();
                }
            }
        }

        void Normalize()
        {
            xenotypeWeights = ClampWeights(xenotypeWeights, HungerXenotypeWeightTable.Clamp);
            incidentDebugPoints = ClampWeights(incidentDebugPoints, ClampDebugPoints);
            incidentWeights = ClampWeights(incidentWeights, ClampIncidentWeight);
            traitWeights = ClampWeights(traitWeights, ClampTraitWeight);
            enabledXenotypeDefNames = Clean(enabledXenotypeDefNames);
            enabledGeneDefNames = Clean(enabledGeneDefNames);
            disabledReliefFoodDefNames = Clean(disabledReliefFoodDefNames);
            disabledIncidentDisplayIds = Clean(disabledIncidentDisplayIds);
            disabledHistoryDisplayIds = Clean(disabledHistoryDisplayIds);
            disabledTraitDisplayIds = Clean(disabledTraitDisplayIds);
        }

        static Dictionary<string, float> ClampWeights(Dictionary<string, float> source, Func<float, float> clamp)
        {
            Dictionary<string, float> normalized = new Dictionary<string, float>();
            if (source == null)
            {
                return normalized;
            }

            foreach (KeyValuePair<string, float> entry in source)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    normalized[entry.Key] = clamp(entry.Value);
                }
            }

            return normalized;
        }

        static float ClampTraitWeight(float value)
        {
            return ClampUnit(value);
        }

        static float ClampIncidentWeight(float value)
        {
            return ClampUnit(value);
        }

        static float ClampDebugPoints(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < HungerAndHavoc.Incidents.HungerIncidentTuning.MinDebugPoints)
            {
                return HungerAndHavoc.Incidents.HungerIncidentTuning.MinDebugPoints;
            }

            return value > HungerAndHavoc.Incidents.HungerIncidentTuning.MaxDebugPoints
                ? HungerAndHavoc.Incidents.HungerIncidentTuning.MaxDebugPoints
                : value;
        }

        static float ClampUnit(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return 0f;
            }

            return value > 100f ? 100f : value;
        }

        static List<string> Clean(List<string> names)
        {
            List<string> cleaned = new List<string>();
            if (names == null)
            {
                return cleaned;
            }

            for (int i = 0; i < names.Count; i++)
            {
                if (!string.IsNullOrEmpty(names[i]) && !cleaned.Contains(names[i]))
                {
                    cleaned.Add(names[i]);
                }
            }

            return cleaned;
        }

        void EnsureCollections()
        {
            xenotypeWeights = xenotypeWeights ?? new Dictionary<string, float>();
            enabledXenotypeDefNames = enabledXenotypeDefNames ?? new List<string>();
            enabledGeneDefNames = enabledGeneDefNames ?? new List<string>();
            disabledReliefFoodDefNames = disabledReliefFoodDefNames ?? new List<string>();
            disabledIncidentDisplayIds = disabledIncidentDisplayIds ?? new List<string>();
            incidentDebugPoints = incidentDebugPoints ?? new Dictionary<string, float>();
            incidentWeights = incidentWeights ?? new Dictionary<string, float>();
            disabledHistoryDisplayIds = disabledHistoryDisplayIds ?? new List<string>();
            disabledTraitDisplayIds = disabledTraitDisplayIds ?? new List<string>();
            traitWeights = traitWeights ?? new Dictionary<string, float>();
        }
        public bool IsIncidentEnabled(string displayId)
        {
            EnsureCollections();
            return !string.IsNullOrEmpty(displayId) && !disabledIncidentDisplayIds.Contains(displayId);
        }

        public void SetIncidentEnabled(string displayId, bool enabled)
        {
            EnsureCollections();
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            if (enabled)
            {
                disabledIncidentDisplayIds.Remove(displayId);
                return;
            }

            if (!disabledIncidentDisplayIds.Contains(displayId))
            {
                disabledIncidentDisplayIds.Add(displayId);
            }
        }

        public float IncidentDebugPoints(string displayId, float catalogPoints)
        {
            EnsureCollections();
            float stored;
            if (!string.IsNullOrEmpty(displayId) && incidentDebugPoints.TryGetValue(displayId, out stored))
            {
                return ClampDebugPoints(stored);
            }

            return ClampDebugPoints(catalogPoints);
        }

        public void SetIncidentDebugPoints(string displayId, float points)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            EnsureCollections();
            incidentDebugPoints[displayId] = ClampDebugPoints(points);
        }

        public float IncidentWeight(string displayId)
        {
            EnsureCollections();
            float stored;
            if (!string.IsNullOrEmpty(displayId) && incidentWeights.TryGetValue(displayId, out stored))
            {
                return ClampIncidentWeight(stored);
            }

            return HungerAndHavoc.Incidents.HungerIncidentTuning.DefaultWeight;
        }

        public void SetIncidentWeight(string displayId, float weight)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            EnsureCollections();
            incidentWeights[displayId] = ClampIncidentWeight(weight);
        }
        public bool IsHistoryEnabled(string displayId)
        {
            EnsureCollections();
            return !string.IsNullOrEmpty(displayId) && !disabledHistoryDisplayIds.Contains(displayId);
        }

        public void SetHistoryEnabled(string displayId, bool enabled)
        {
            SetDisabled(disabledHistoryDisplayIds, displayId, enabled);
        }

        public void SetAllHistories(bool enabled, IReadOnlyList<string> displayIds)
        {
            EnsureCollections();
            disabledHistoryDisplayIds.Clear();
            if (enabled || displayIds == null)
            {
                return;
            }

            for (int i = 0; i < displayIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(displayIds[i]) && !disabledHistoryDisplayIds.Contains(displayIds[i]))
                {
                    disabledHistoryDisplayIds.Add(displayIds[i]);
                }
            }
        }

        public bool IsTraitEnabled(string displayId)
        {
            EnsureCollections();
            return !string.IsNullOrEmpty(displayId) && !disabledTraitDisplayIds.Contains(displayId);
        }

        public void SetTraitEnabled(string displayId, bool enabled)
        {
            SetDisabled(disabledTraitDisplayIds, displayId, enabled);
        }

        public void SetAllTraits(bool enabled, IReadOnlyList<string> displayIds)
        {
            EnsureCollections();
            disabledTraitDisplayIds.Clear();
            if (enabled || displayIds == null)
            {
                return;
            }

            for (int i = 0; i < displayIds.Count; i++)
            {
                if (!string.IsNullOrEmpty(displayIds[i]) && !disabledTraitDisplayIds.Contains(displayIds[i]))
                {
                    disabledTraitDisplayIds.Add(displayIds[i]);
                }
            }
        }

        public float TraitWeight(string displayId)
        {
            EnsureCollections();
            float stored;
            if (!string.IsNullOrEmpty(displayId) && traitWeights.TryGetValue(displayId, out stored))
            {
                return ClampTraitWeight(stored);
            }

            return Data.HungerContentCatalog.DefaultTraitWeight(displayId);
        }

        public void SetTraitWeight(string displayId, float weight)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            EnsureCollections();
            traitWeights[displayId] = ClampTraitWeight(weight);
        }

        void SetDisabled(List<string> disabled, string displayId, bool enabled)
        {
            EnsureCollections();
            if (string.IsNullOrEmpty(displayId) || disabled == null)
            {
                return;
            }

            if (enabled)
            {
                disabled.Remove(displayId);
                return;
            }

            if (!disabled.Contains(displayId))
            {
                disabled.Add(displayId);
            }
        }

        public bool AllowsRequest(HungerAndHavoc.Incidents.HungerChoiceKind choice)
        {
            if (choice == HungerAndHavoc.Incidents.HungerChoiceKind.Aid ||
                choice == HungerAndHavoc.Incidents.HungerChoiceKind.ChildExchange)
            {
                return aidRequestsEnabled;
            }

            if (choice == HungerAndHavoc.Incidents.HungerChoiceKind.Intel)
            {
                return intelTradesEnabled;
            }

            if (choice == HungerAndHavoc.Incidents.HungerChoiceKind.None)
            {
                return false;
            }

            return visitorChoicesEnabled;
        }
    }
}
