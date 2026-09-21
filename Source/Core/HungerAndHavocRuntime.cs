using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class HungerAndHavocRuntime
    {
        public const string PackageId = "nanaloveyuki.ratkin.hungerandhavoc";
        public const string HarmonyId = PackageId;
        public const string DefPrefix = "RHAH_";

        static readonly HashSet<string> activeBatches = new HashSet<string>();

        public static bool AllowsNewContent =>
            HungerAndHavocMod.Settings == null || HungerAndHavocMod.Settings.enableNewContent;

        internal static bool IsBatchActive(Map map, int batchId)
        {
            return map != null && activeBatches.Contains(BatchKey(map, batchId));
        }

        internal static void RegisterBatch(Map map, int batchId)
        {
            if (map != null)
            {
                activeBatches.Add(BatchKey(map, batchId));
            }
        }

        static string BatchKey(Map map, int batchId)
        {
            return map.uniqueID + ":" + batchId;
        }
    }
}
