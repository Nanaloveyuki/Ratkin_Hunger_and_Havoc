using UnityEngine;
using Verse;

namespace HungerAndHavoc.Core
{
    public class HungerAndHavocMod : Mod
    {
        public static HungerAndHavocSettings Settings;

        public HungerAndHavocMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<HungerAndHavocSettings>();
#if RHAH_IRISMENUS
            Pawn.Compat.RHAH_IrisMenusCompat.TryRegister(this);
#endif
        }

        public override string SettingsCategory()
        {
            return "RHAH_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled("RHAH_Settings_EnableNewContent".Translate(), ref Settings.enableNewContent,
                "RHAH_Settings_EnableNewContent_Tooltip".Translate());
            listing.End();
            Settings.Write();
        }
    }
}
