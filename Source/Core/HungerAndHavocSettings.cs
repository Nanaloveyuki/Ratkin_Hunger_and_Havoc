using System.Collections.Generic;
using HungerAndHavoc.Generation;
using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocSettings : ModSettings
    {
        public bool enableNewContent = true;
        public bool optimizeGeneration = true;
        Dictionary<string, float> xenotypeWeights = new Dictionary<string, float>();
        List<string> enabledXenotypeDefNames = new List<string>();
        List<string> enabledGeneDefNames = new List<string>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableNewContent, "enableNewContent", true);
            Scribe_Values.Look(ref optimizeGeneration, "optimizeGeneration", true);
            Scribe_Collections.Look(ref xenotypeWeights, "xenotypeWeights", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref enabledXenotypeDefNames, "enabledXenotypeDefNames", LookMode.Value);
            Scribe_Collections.Look(ref enabledGeneDefNames, "enabledGeneDefNames", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                xenotypeWeights = xenotypeWeights ?? new Dictionary<string, float>();
                enabledXenotypeDefNames = enabledXenotypeDefNames ?? new List<string>();
                enabledGeneDefNames = enabledGeneDefNames ?? new List<string>();
                Normalize();
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
        }
    }
}
