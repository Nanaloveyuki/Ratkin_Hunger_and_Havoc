using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class HungerAndHavocRuntime
    {
        public const string PackageId = "nanaloveyuki.ratkin.hungerandhavoc";
        public const string HarmonyId = PackageId;
        public const string DefPrefix = "RHAH_";

        public static bool AllowsNewContent =>
            HungerAndHavocMod.Settings == null || HungerAndHavocMod.Settings.enableNewContent;

        internal static bool IsBatchActive(Map map, int batchId)
        {
            GameComponent_HungerAndHavoc component = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            return component != null && ContainsBatch(component.ActiveGenerationBatches, BatchKey(map, batchId));
        }

        internal static void RegisterBatch(Map map, int batchId)
        {
            GameComponent_HungerAndHavoc component = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (component != null && map != null)
            {
                component.RegisterBatch(BatchKey(map, batchId));
            }
        }

        static string BatchKey(Map map, int batchId)
        {
            return map == null ? string.Empty : map.uniqueID + ":" + batchId;
        }

        static bool ContainsBatch(IReadOnlyList<string> batches, string key)
        {
            for (int i = 0; i < batches.Count; i++)
            {
                if (batches[i] == key)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
