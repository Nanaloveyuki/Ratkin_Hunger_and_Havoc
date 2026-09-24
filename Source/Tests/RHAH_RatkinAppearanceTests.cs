using System.Collections.Generic;
using HungerAndHavoc.Generation;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_RatkinAppearanceTests
    {
        [Fact]
        public void ReadsTheHarShapeAndRejectsHumanClothes()
        {
            RaceThing race = Race(new PartGenerator
            {
                headTypes = new List<HeadThing>
                {
                    new HeadThing { defName = "RK_HeadType_Head1" }
                }
            },
            new Dictionary<System.Type, StyleSetting>
            {
                { typeof(HairDef), new StyleSetting { hasStyle = true, styleTagsOverride = new List<string> { "RK_Style" } } },
                { typeof(BeardDef), new StyleSetting { hasStyle = false } },
                { typeof(TattooDef), new StyleSetting { hasStyle = false } }
            },
            new List<RaceThing> { new RaceThing { defName = "RK_ApronSkirt" } },
            new List<RaceThing> { new RaceThing { defName = "Apparel_ShieldBelt" } });

            RHAH_RatkinStyle style = RHAH_RatkinAppearance.Read(race);

            Assert.True(style.Complete);
            Assert.Contains("RK_HeadType_Head1", style.HeadNames);
            Assert.Contains("RK_Style", style.HairTags);
            Assert.True(style.BeardsDisabled);
            Assert.True(style.TattoosDisabled);
            Assert.True(RHAH_RatkinAppearance.ApparelAllowed("RK_ApronSkirt", style.ApparelNames, style.WhiteApparelNames));
            Assert.True(RHAH_RatkinAppearance.ApparelAllowed("Apparel_ShieldBelt", style.ApparelNames, style.WhiteApparelNames));
            Assert.False(RHAH_RatkinAppearance.ApparelAllowed("Apparel_TribalA", style.ApparelNames, style.WhiteApparelNames));
        }

        [Fact]
        public void MissingHarFieldsAreNotACompleteRatkinStyle()
        {
            RHAH_RatkinStyle empty = RHAH_RatkinAppearance.Read(new RaceThing { defName = "Ratkin" });
            RHAH_RatkinStyle partial = new RHAH_RatkinStyle(
                new List<string> { "RK_HeadType_Head1" },
                new List<string>(),
                false,
                true,
                new HashSet<string>(),
                new HashSet<string>());

            Assert.False(empty.Complete);
            Assert.False(partial.Complete);
            Assert.False(RHAH_RatkinAppearance.UsesRatkinAppearance("Human"));
            Assert.True(RHAH_RatkinAppearance.UsesRatkinAppearance("Ratkin"));
        }

        sealed class RaceThing
        {
            public string defName;
            public RaceSettings alienRace;
        }

        sealed class RaceSettings
        {
            public GeneralSettings generalSettings;
            public Dictionary<System.Type, StyleSetting> styleSettings;
            public RaceRestriction raceRestriction;
        }

        sealed class GeneralSettings
        {
            public PartGenerator alienPartGenerator;
        }

        sealed class PartGenerator
        {
            public List<HeadThing> headTypes;
        }

        sealed class HeadThing
        {
            public string defName;
        }

        sealed class StyleSetting
        {
            public bool hasStyle;
            public List<string> styleTagsOverride;
        }

        sealed class RaceRestriction
        {
            public List<RaceThing> apparelList;
            public List<RaceThing> whiteApparelList;
        }

        sealed class HairDef
        {
        }

        sealed class BeardDef
        {
        }

        sealed class TattooDef
        {
        }

        static RaceThing Race(
            PartGenerator generator,
            Dictionary<System.Type, StyleSetting> styles,
            List<RaceThing> apparel,
            List<RaceThing> white)
        {
            return new RaceThing
            {
                defName = "Ratkin",
                alienRace = new RaceSettings
                {
                    generalSettings = new GeneralSettings { alienPartGenerator = generator },
                    styleSettings = styles,
                    raceRestriction = new RaceRestriction
                    {
                        apparelList = apparel,
                        whiteApparelList = white
                    }
                }
            };
        }
    }
}
