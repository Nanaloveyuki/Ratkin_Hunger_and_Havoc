using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocSettings : ModSettings
    {
        public bool enableNewContent = true;
        public bool optimizeGeneration = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableNewContent, "enableNewContent", true);
            Scribe_Values.Look(ref optimizeGeneration, "optimizeGeneration", true);
        }
    }
}
