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
        List<string> disabledIncidentDisplayIds = new List<string>();

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
            Scribe_Collections.Look(ref disabledIncidentDisplayIds, "disabledIncidentDisplayIds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                xenotypeWeights = xenotypeWeights ?? new Dictionary<string, float>();
                enabledXenotypeDefNames = enabledXenotypeDefNames ?? new List<string>();
                enabledGeneDefNames = enabledGeneDefNames ?? new List<string>();
                disabledReliefFoodDefNames = disabledReliefFoodDefNames ?? new List<string>();
                disabledIncidentDisplayIds = disabledIncidentDisplayIds ?? new List<string>();
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
            Dictionary<string, float> normalized = new Dictionary<string, float>();
            foreach (KeyValuePair<string, float> entry in xenotypeWeights)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    normalized[entry.Key] = HungerXenotypeWeightTable.Clamp(entry.Value);
                }
            }

            xenotypeWeights = normalized;
            enabledXenotypeDefNames = Clean(enabledXenotypeDefNames);
            enabledGeneDefNames = Clean(enabledGeneDefNames);
            disabledReliefFoodDefNames = Clean(disabledReliefFoodDefNames);
            disabledIncidentDisplayIds = Clean(disabledIncidentDisplayIds);
        }

        static List<string> Clean(List<string> names)
        {
            List<string> cleaned = new List<string>();
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
