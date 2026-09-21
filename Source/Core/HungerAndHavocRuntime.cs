namespace HungerAndHavoc.Core
{
    internal static class HungerAndHavocRuntime
    {
        public const string PackageId = "nanaloveyuki.ratkin.hungerandhavoc";
        public const string HarmonyId = PackageId;
        public const string DefPrefix = "RHAH_";

        public static bool AllowsNewContent =>
            HungerAndHavocMod.Settings == null || HungerAndHavocMod.Settings.enableNewContent;
    }
}
