using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Generation
{
    internal static class RHAH_XenotypeWeightTable
    {
        internal const float MinWeight = 0f;
        internal const float MaxWeight = 100f;
        internal const float DefaultBuiltinWeight = 100f;
        internal const float DefaultExternalWeight = 0f;

        internal static float Clamp(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return MinWeight;
            }

            if (value < MinWeight)
            {
                return MinWeight;
            }

            if (value > MaxWeight)
            {
                return MaxWeight;
            }

            return value;
        }

        internal static float StoredOrDefault(IReadOnlyDictionary<string, float> weights, string defName, float fallback)
        {
            if (string.IsNullOrEmpty(defName) || weights == null || !weights.TryGetValue(defName, out float stored))
            {
                return Clamp(fallback);
            }

            return Clamp(stored);
        }

        internal static string Choose(IReadOnlyList<string> defNames, IReadOnlyList<float> weights, float roll)
        {
            if (defNames == null || weights == null || defNames.Count == 0 || weights.Count != defNames.Count)
            {
                return null;
            }

            float total = 0f;
            for (int i = 0; i < weights.Count; i++)
            {
                float weight = Clamp(weights[i]);
                if (!string.IsNullOrEmpty(defNames[i]) && weight > 0f)
                {
                    total += weight;
                }
            }

            if (total <= 0f)
            {
                return null;
            }

            float cursor = roll;
            if (cursor < 0f)
            {
                cursor = 0f;
            }

            if (cursor >= total)
            {
                cursor = total - float.Epsilon;
            }

            string last = null;
            for (int i = 0; i < defNames.Count; i++)
            {
                float weight = Clamp(weights[i]);
                if (string.IsNullOrEmpty(defNames[i]) || weight <= 0f)
                {
                    continue;
                }

                last = defNames[i];
                cursor -= weight;
                if (cursor < 0f)
                {
                    return defNames[i];
                }
            }

            return last;
        }

        internal static string Resolve(bool explicitChoice, string explicitDefName, bool optimizationEnabled, string weightedDefName, string defaultDefName)
        {
            if (explicitChoice)
            {
                return string.IsNullOrEmpty(explicitDefName) ? null : explicitDefName;
            }

            if (!optimizationEnabled)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(weightedDefName))
            {
                return weightedDefName;
            }

            return string.IsNullOrEmpty(defaultDefName) ? null : defaultDefName;
        }
    }
}
