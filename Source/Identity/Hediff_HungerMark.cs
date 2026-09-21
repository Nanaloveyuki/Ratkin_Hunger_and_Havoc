using Verse;

namespace HungerAndHavoc.Identity
{
    public class Hediff_HungerMark : HediffWithComps
    {
        public CompHungerPawn Hunger
        {
            get
            {
                if (comps == null)
                {
                    return null;
                }

                for (int i = 0; i < comps.Count; i++)
                {
                    if (comps[i] is CompHungerPawn hunger)
                    {
                        return hunger;
                    }
                }

                return null;
            }
        }

        public override bool ShouldRemove => false;

        public override string LabelBase
        {
            get
            {
                CompHungerPawn comp = Hunger;
                if (comp == null)
                {
                    return base.LabelBase;
                }

                string key = comp.RoleLabelKey;
                TaggedString translated = key.Translate();
                if (translated.RawText == key)
                {
                    return base.LabelBase;
                }

                return translated;
            }
        }
    }
}
