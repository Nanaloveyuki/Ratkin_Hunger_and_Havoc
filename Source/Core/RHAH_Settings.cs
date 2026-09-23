using System;
using System.Collections.Generic;
using HungerAndHavoc.Generation;
using Verse;

namespace HungerAndHavoc.Core
{
    public class RHAH_Settings : ModSettings
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
        public bool traderIgnoresHarshEnvironment = true;
        public bool traderIgnoresEnclosedSpace = true;
        public bool childExchangeFoodSubstitution = true;
        public bool familyDropEnabled = true;
        public bool motherFeedEnabled = true;
        public bool prisonerScavengeEnabled = true;
        public bool tailBiteEnabled;
        public bool broadcastEnabled = true;
        public int broadcastCooldownDays = 3;
        public bool staggerGeneration = true;
        public bool refugeeCampEnabled = true;
        public float refugeePredationChancePercent = 10f;
        public bool refugeePredationFightBack = true;
        public bool outsidePredatorsFollowDifficulty;
        public bool pawnHistoriesEnabled = true;
        public bool pawnTraitsEnabled = true;
        List<string> disabledIncidentDisplayIds = new List<string>();
        Dictionary<string, float> incidentDebugPoints = new Dictionary<string, float>();
        Dictionary<string, float> incidentWeights = new Dictionary<string, float>();
        List<string> disabledHistoryDisplayIds = new List<string>();
        List<string> disabledTraitDisplayIds = new List<string>();
        Dictionary<string, float> traitWeights = new Dictionary<string, float>();
        public int maxEventPawns = 30;
        public float minGeneratedAge;
        public float maxGeneratedAge = 50f;
        public float reliefFoodScoreBonus = 0.1f;
        public float fedStayDays = 0.5f;
        public bool waitWhenNoFood = true;
        public float noFoodWaitDays = 0.5f;
        public int shelterDays = 5;
        public int hireDays = 60;
        public bool coldClothesEnabled = true;
        public float minimumEventTemperature = -35f;
        public float maximumEventTemperature = 70f;
        public bool countEndingsWithoutNarrator = true;
        public bool endingsWithoutNarrator = true;
        public int endingAidGoal = 99;
        public int endingBroadcastGoal = 3;
        public int endingExpulsionLimit = 3;
        public int endingAdultGoal = 100;
        public int endingWaitDays = 30;
        public bool endingE01 = true;
        public bool endingE02 = true;
        public bool endingE03 = true;
        public bool endingE04 = true;
        public bool endingE05 = true;
        public bool endingIdentity = true;
        public bool plagueEnabled = true;
        public float plagueSpreadChancePerCarrier = 0.005f;
        public float plagueSpreadChanceCap = 0.30f;
        public int plagueSpreadDayInterval = 3;
        public int plagueSpreadHour = 6;
        public float plagueBloodPumpingSkipPercent = 120f;
        public bool plagueQuarantineBlocksJoin = true;
        public bool plagueReturnEnabled = true;
        public int plagueReturnDelayDays = 15;
        public int plagueReturnStayDays = 1;
        public bool beggingEnabled = true;
        public bool stealingEnabled = true;
        public bool fightingEnabled = true;
        public bool gnawingEnabled = true;
        public bool batchTurnsHostile = true;
        public bool batchLeavesTogether = true;
        public bool suiYinThreatTempo = true;
        public float weightWild = 1.4f;
        public float weightBeggar = 1f;
        public float weightThief = 0.7f;
        public float weightTrade = 0.5f;
        public float weightSiege = 0.35f;
        public float weightAid = 0.25f;
        public float weightSpecial = 0.2f;
        public float weightIntel = 0.12f;
        public float weightSeason = 1.1f;
        public float weightPlague = 0.5f;


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
            Scribe_Values.Look(ref traderIgnoresHarshEnvironment, "traderIgnoresHarshEnvironment", true);
            Scribe_Values.Look(ref traderIgnoresEnclosedSpace, "traderIgnoresEnclosedSpace", true);
            Scribe_Values.Look(ref childExchangeFoodSubstitution, "childExchangeFoodSubstitution", true);
            Scribe_Values.Look(ref familyDropEnabled, "familyDropEnabled", true);
            Scribe_Values.Look(ref motherFeedEnabled, "motherFeedEnabled", true);
            Scribe_Values.Look(ref prisonerScavengeEnabled, "prisonerScavengeEnabled", true);
            Scribe_Values.Look(ref tailBiteEnabled, "tailBiteEnabled", false);
            Scribe_Values.Look(ref broadcastEnabled, "broadcastEnabled", true);
            Scribe_Values.Look(ref broadcastCooldownDays, "broadcastCooldownDays", 3);
            Scribe_Values.Look(ref staggerGeneration, "staggerGeneration", true);
            Scribe_Values.Look(ref refugeeCampEnabled, "refugeeCampEnabled", true);
            Scribe_Values.Look(ref refugeePredationChancePercent, "refugeePredationChancePercent", 10f);
            Scribe_Values.Look(ref refugeePredationFightBack, "refugeePredationFightBack", true);
            Scribe_Values.Look(ref outsidePredatorsFollowDifficulty, "outsidePredatorsFollowDifficulty", false);
            Scribe_Values.Look(ref pawnHistoriesEnabled, "pawnHistoriesEnabled", true);
            Scribe_Values.Look(ref pawnTraitsEnabled, "pawnTraitsEnabled", true);
            Scribe_Collections.Look(ref disabledIncidentDisplayIds, "disabledIncidentDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref incidentDebugPoints, "incidentDebugPoints", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref incidentWeights, "incidentWeights", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref disabledHistoryDisplayIds, "disabledHistoryDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref disabledTraitDisplayIds, "disabledTraitDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref traitWeights, "traitWeights", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref maxEventPawns, "maxEventPawns", 30);
            Scribe_Values.Look(ref minGeneratedAge, "minGeneratedAge", 0f);
            Scribe_Values.Look(ref maxGeneratedAge, "maxGeneratedAge", 50f);
            Scribe_Values.Look(ref reliefFoodScoreBonus, "reliefFoodScoreBonus", 0.1f);
            Scribe_Values.Look(ref fedStayDays, "fedStayDays", 0.5f);
            Scribe_Values.Look(ref waitWhenNoFood, "waitWhenNoFood", true);
            Scribe_Values.Look(ref noFoodWaitDays, "noFoodWaitDays", 0.5f);
            Scribe_Values.Look(ref shelterDays, "shelterDays", 5);
            Scribe_Values.Look(ref hireDays, "hireDays", 60);
            Scribe_Values.Look(ref coldClothesEnabled, "coldClothesEnabled", true);
            Scribe_Values.Look(ref minimumEventTemperature, "minimumEventTemperature", -35f);
            Scribe_Values.Look(ref maximumEventTemperature, "maximumEventTemperature", 70f);
            Scribe_Values.Look(ref countEndingsWithoutNarrator, "countEndingsWithoutNarrator", true);
            Scribe_Values.Look(ref endingsWithoutNarrator, "endingsWithoutNarrator", true);
            Scribe_Values.Look(ref endingAidGoal, "endingAidGoal", 99);
            Scribe_Values.Look(ref endingBroadcastGoal, "endingBroadcastGoal", 3);
            Scribe_Values.Look(ref endingExpulsionLimit, "endingExpulsionLimit", 3);
            Scribe_Values.Look(ref endingAdultGoal, "endingAdultGoal", 100);
            Scribe_Values.Look(ref endingWaitDays, "endingWaitDays", 30);
            Scribe_Values.Look(ref endingE01, "endingE01", true);
            Scribe_Values.Look(ref endingE02, "endingE02", true);
            Scribe_Values.Look(ref endingE03, "endingE03", true);
            Scribe_Values.Look(ref endingE04, "endingE04", true);
            Scribe_Values.Look(ref endingE05, "endingE05", true);
            Scribe_Values.Look(ref plagueEnabled, "plagueEnabled", true);
            Scribe_Values.Look(ref plagueSpreadChancePerCarrier, "plagueSpreadChancePerCarrier", 0.005f);
            Scribe_Values.Look(ref plagueSpreadChanceCap, "plagueSpreadChanceCap", 0.30f);
            Scribe_Values.Look(ref plagueSpreadDayInterval, "plagueSpreadDayInterval", 3);
            Scribe_Values.Look(ref plagueSpreadHour, "plagueSpreadHour", 6);
            Scribe_Values.Look(ref plagueBloodPumpingSkipPercent, "plagueBloodPumpingSkipPercent", 120f);
            Scribe_Values.Look(ref plagueQuarantineBlocksJoin, "plagueQuarantineBlocksJoin", true);
            Scribe_Values.Look(ref plagueReturnEnabled, "plagueReturnEnabled", true);
            Scribe_Values.Look(ref plagueReturnDelayDays, "plagueReturnDelayDays", 15);
            Scribe_Values.Look(ref plagueReturnStayDays, "plagueReturnStayDays", 1);
            Scribe_Values.Look(ref beggingEnabled, "beggingEnabled", true);
            Scribe_Values.Look(ref stealingEnabled, "stealingEnabled", true);
            Scribe_Values.Look(ref fightingEnabled, "fightingEnabled", true);
            Scribe_Values.Look(ref gnawingEnabled, "gnawingEnabled", true);
            Scribe_Values.Look(ref batchTurnsHostile, "batchTurnsHostile", true);
            Scribe_Values.Look(ref batchLeavesTogether, "batchLeavesTogether", true);
            Scribe_Values.Look(ref suiYinThreatTempo, "suiYinThreatTempo", true);
            Scribe_Values.Look(ref weightWild, "weightWild", 1.4f);
            Scribe_Values.Look(ref weightBeggar, "weightBeggar", 1f);
            Scribe_Values.Look(ref weightThief, "weightThief", 0.7f);
            Scribe_Values.Look(ref weightTrade, "weightTrade", 0.5f);
            Scribe_Values.Look(ref weightSiege, "weightSiege", 0.35f);
            Scribe_Values.Look(ref weightAid, "weightAid", 0.25f);
            Scribe_Values.Look(ref weightSpecial, "weightSpecial", 0.2f);
            Scribe_Values.Look(ref weightIntel, "weightIntel", 0.12f);
            Scribe_Values.Look(ref weightSeason, "weightSeason", 1.1f);
            Scribe_Values.Look(ref weightPlague, "weightPlague", 0.5f);
            Scribe_Values.Look(ref endingIdentity, "endingIdentity", true);
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
                broadcastCooldownDays = HungerAndHavoc.Incidents.RHAH_BroadcastRules.ClampDays(broadcastCooldownDays);
                Normalize();
                positiveIncidentDays = HungerAndHavoc.Incidents.RHAH_IncidentSchedule.ClampDays(positiveIncidentDays);
                negativeIncidentDays = HungerAndHavoc.Incidents.RHAH_IncidentSchedule.ClampDays(negativeIncidentDays);
                ClampVisitorRules();
                ClampEndingGoals();
                refugeePredationChancePercent = HungerAndHavoc.Incidents.RHAH_PredationRules.ClampChance(refugeePredationChancePercent);
                ClampPlagueRules();
                ClampFamilyWeights();
            }
        }

        public float XenotypeWeight(string defName)
        {
            EnsureCollections();
            return RHAH_XenotypeWeightTable.StoredOrDefault(xenotypeWeights, defName, RHAH_GeneCatalog.SuggestedWeight(defName));
        }

        public void SetXenotypeWeight(string defName, float value)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            EnsureCollections();
            xenotypeWeights[defName] = RHAH_XenotypeWeightTable.Clamp(value);
        }

        public void ResetXenotypeWeights()
        {
            EnsureCollections();
            xenotypeWeights.Clear();
        }

        public bool IsXenotypeEnabled(string defName)
        {
            EnsureCollections();
            return RHAH_GeneCatalog.IsBuiltin(defName) || enabledXenotypeDefNames.Contains(defName);
        }

        public void SetXenotypeEnabled(string defName, bool enabled)
        {
            if (string.IsNullOrEmpty(defName) || RHAH_GeneCatalog.IsBuiltin(defName))
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
                MapComponent_RHAH_Map component = map != null
                    ? map.GetComponent<MapComponent_RHAH_Map>()
                    : null;
                if (component != null)
                {
                    component.InvalidateFoodSearch();
                }
            }
        }

        void Normalize()
        {
            xenotypeWeights = ClampWeights(xenotypeWeights, RHAH_XenotypeWeightTable.Clamp);
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

        void ClampVisitorRules()
        {
            maxEventPawns = Pawn.RHAH_VisitorRules.ClampCount(maxEventPawns);
            minGeneratedAge = Pawn.RHAH_VisitorRules.ClampAge(minGeneratedAge);
            maxGeneratedAge = Pawn.RHAH_VisitorRules.ClampAge(maxGeneratedAge);
            if (maxGeneratedAge < minGeneratedAge)
            {
                maxGeneratedAge = minGeneratedAge;
            }

            reliefFoodScoreBonus = Pawn.RHAH_VisitorRules.ClampBonus(reliefFoodScoreBonus);
            fedStayDays = ClampStayDays(fedStayDays, 0.5f);
            noFoodWaitDays = ClampStayDays(noFoodWaitDays, 0.5f);
            shelterDays = Pawn.RHAH_VisitorRules.ClampShelterDays(shelterDays);
            hireDays = Pawn.RHAH_VisitorRules.ClampHireDays(hireDays);
            if (float.IsNaN(minimumEventTemperature) || float.IsInfinity(minimumEventTemperature))
            {
                minimumEventTemperature = -35f;
            }

            if (float.IsNaN(maximumEventTemperature) || float.IsInfinity(maximumEventTemperature))
            {
                maximumEventTemperature = 70f;
            }

            if (minimumEventTemperature < -35f)
            {
                minimumEventTemperature = -35f;
            }

            if (maximumEventTemperature > 70f)
            {
                maximumEventTemperature = 70f;
            }

            if (minimumEventTemperature > maximumEventTemperature)
            {
                float swap = minimumEventTemperature;
                minimumEventTemperature = maximumEventTemperature;
                maximumEventTemperature = swap;
            }
        }


        static float ClampStayDays(float days, float fallback)
        {
            if (float.IsNaN(days) || float.IsInfinity(days))
            {
                return fallback;
            }

            if (days < 0f)
            {
                return 0f;
            }

            return days > 5f ? 5f : days;
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
            if (float.IsNaN(value) || float.IsInfinity(value) || value < HungerAndHavoc.Incidents.RHAH_IncidentTuning.MinDebugPoints)
            {
                return HungerAndHavoc.Incidents.RHAH_IncidentTuning.MinDebugPoints;
            }

            return value > HungerAndHavoc.Incidents.RHAH_IncidentTuning.MaxDebugPoints
                ? HungerAndHavoc.Incidents.RHAH_IncidentTuning.MaxDebugPoints
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

            return HungerAndHavoc.Incidents.RHAH_IncidentTuning.DefaultWeight;
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

            return Data.RHAH_ContentCatalog.DefaultTraitWeight(displayId);
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

        public bool AllowsRequest(HungerAndHavoc.Incidents.RHAH_ChoiceKind choice)
        {
            if (choice == HungerAndHavoc.Incidents.RHAH_ChoiceKind.Aid ||
                choice == HungerAndHavoc.Incidents.RHAH_ChoiceKind.ChildExchange)
            {
                return aidRequestsEnabled;
            }

            if (choice == HungerAndHavoc.Incidents.RHAH_ChoiceKind.Intel)
            {
                return intelTradesEnabled;
            }

            if (choice == HungerAndHavoc.Incidents.RHAH_ChoiceKind.None)
            {
                return false;
            }

            return visitorChoicesEnabled;
        }

        internal HungerAndHavoc.Narrative.RHAH_EndingGoals EndingGoals()
        {
            return new HungerAndHavoc.Narrative.RHAH_EndingGoals(
                endingAidGoal,
                endingBroadcastGoal,
                endingExpulsionLimit,
                endingAdultGoal,
                endingWaitDays,
                countEndingsWithoutNarrator,
                endingsWithoutNarrator,
                endingE01,
                endingE02,
                endingE03,
                endingE04,
                endingE05,
                endingIdentity);
        }

        internal HungerAndHavoc.Incidents.RHAH_WeightFactors WeightFactors()
        {
            return new HungerAndHavoc.Incidents.RHAH_WeightFactors(
                weightWild,
                weightBeggar,
                weightThief,
                weightTrade,
                weightSiege,
                weightAid,
                weightSpecial,
                weightIntel,
                weightSeason,
                weightPlague);
        }

        void ClampPlagueRules()
        {
            plagueSpreadChancePerCarrier = ClampUnit(plagueSpreadChancePerCarrier, 0.005f);
            plagueSpreadChanceCap = ClampUnit(plagueSpreadChanceCap, 0.30f);
            if (plagueSpreadDayInterval < 1)
            {
                plagueSpreadDayInterval = 1;
            }

            if (plagueSpreadDayInterval > 30)
            {
                plagueSpreadDayInterval = 30;
            }

            if (plagueSpreadHour < 0)
            {
                plagueSpreadHour = 0;
            }

            if (plagueSpreadHour > 23)
            {
                plagueSpreadHour = 23;
            }

            if (float.IsNaN(plagueBloodPumpingSkipPercent) || plagueBloodPumpingSkipPercent < 0f)
            {
                plagueBloodPumpingSkipPercent = 120f;
            }

            if (plagueBloodPumpingSkipPercent > 300f)
            {
                plagueBloodPumpingSkipPercent = 300f;
            }

            if (plagueReturnDelayDays < 0)
            {
                plagueReturnDelayDays = 0;
            }

            if (plagueReturnDelayDays > 60)
            {
                plagueReturnDelayDays = 60;
            }

            if (plagueReturnStayDays < 0)
            {
                plagueReturnStayDays = 0;
            }

            if (plagueReturnStayDays > 15)
            {
                plagueReturnStayDays = 15;
            }
        }

        void ClampFamilyWeights()
        {
            weightWild = ClampFactor(weightWild, 1.4f);
            weightBeggar = ClampFactor(weightBeggar, 1f);
            weightThief = ClampFactor(weightThief, 0.7f);
            weightTrade = ClampFactor(weightTrade, 0.5f);
            weightSiege = ClampFactor(weightSiege, 0.35f);
            weightAid = ClampFactor(weightAid, 0.25f);
            weightSpecial = ClampFactor(weightSpecial, 0.2f);
            weightIntel = ClampFactor(weightIntel, 0.12f);
            weightSeason = ClampFactor(weightSeason, 1.1f);
            weightPlague = ClampFactor(weightPlague, 0.5f);
        }

        static float ClampUnit(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return fallback;
            }

            return value > 1f ? 1f : value;
        }

        static float ClampFactor(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
            {
                return fallback;
            }

            return value > 5f ? 5f : value;
        }

        void ClampEndingGoals()
        {
            endingAidGoal = HungerAndHavoc.Narrative.RHAH_EndingRules.ClampAid(endingAidGoal);
            endingBroadcastGoal = HungerAndHavoc.Narrative.RHAH_EndingRules.ClampBroadcasts(endingBroadcastGoal);
            endingExpulsionLimit = HungerAndHavoc.Narrative.RHAH_EndingRules.ClampExpulsions(endingExpulsionLimit);
            endingAdultGoal = HungerAndHavoc.Narrative.RHAH_EndingRules.ClampAdults(endingAdultGoal);
            endingWaitDays = HungerAndHavoc.Narrative.RHAH_EndingRules.ClampWait(endingWaitDays);
        }
    }
}
