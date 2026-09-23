using Verse;

namespace HungerAndHavoc.Identity
{
    public class Hediff_RHAH_Mark : HediffWithComps
    {
        public CompRHAH_Pawn Hunger
        {
            get
            {
                if (comps == null)
                {
                    return null;
                }

                for (int i = 0; i < comps.Count; i++)
                {
                    if (comps[i] is CompRHAH_Pawn hunger)
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
                CompRHAH_Pawn comp = Hunger;
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
