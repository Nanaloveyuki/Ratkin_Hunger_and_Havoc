using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocSettings : ModSettings
    {
        public bool enableNewContent = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableNewContent, "enableNewContent", true);
        }
    }
}
