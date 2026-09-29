using System;
using System.Xml.Linq;
using HungerAndHavoc.Core;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_SaveCleanupTests
    {
        [Fact]
        public void RemovesOwnedNodesAndKeepsForeignIdentity()
        {
            RHAH_SaveCleanup cleanup = new RHAH_SaveCleanup(Plan());
            XDocument document = Sample();

            cleanup.Clean(document);

            XElement game = document.Root.Element("game");
            Assert.Single(game.Element("components").Elements());
            Assert.Equal("ForeignComponent", game.Element("components").Element("li").Attribute("Class").Value);
            Assert.Equal("RatkinColonist", Pawn(document, "11").Element("kindDef").Value);
            Assert.Equal("11", Pawn(document, "11").Element("id").Value);
            Assert.Equal("22", Pawn(document, "22").Element("id").Value);
            Assert.Equal("Colonist", Pawn(document, "22").Element("kindDef").Value);
            Assert.Equal("Ancients", Only(document, "factionDef").Value);
            Assert.Equal("99", FactionLoadId(document));
            Assert.Equal("Baseliner", Only(document, "xenotype").Value);
            Assert.Equal("ForeignGene", Only(Pawn(document, "11").Element("genes").Element("endogenes"), "def").Value);
            Assert.Equal("Ratkin_Kid", Pawn(document, "11").Element("story").Element("childhood").Value);
            Assert.Equal("Ratkin_Farmer", Pawn(document, "11").Element("story").Element("adulthood").Value);
            Assert.Equal("Randy", Only(document, "storytellerDef").Value);
            Assert.False(HasValue(document, "def", "RHAH_HungerMark"));
            Assert.False(HasValue(document, "def", "RHAH_Trait_HardyLabor"));
            Assert.False(HasValue(document, "def", "RHAH_Gene_ThinRations"));
            Assert.False(HasClass(document, "HungerAndHavoc.Pawn.Area_RHAH_Relief"));
            Assert.False(HasClass(document, "HungerAndHavoc.Pawn.LordJob_RHAH_Visitor"));
            Assert.False(HasNamed(document, "curJob"));
            Assert.False(HasQueuedJob(document));
            Assert.False(HasValue(document, "target", "Thing_18"));
            Assert.True(HasValue(document, "target", "Thing_19"));
            Assert.Equal(1, CountChildren(document, "keys"));
            Assert.Equal(1, CountChildren(document, "values"));
            Assert.Equal("Thing_19", FirstChild(FirstNamed(document, "keys")).Value);
            Assert.True(cleanup.RemovedEntries > 0);
            Assert.True(cleanup.ReplacedDefs > 0);
        }

        [Fact]
        public void ConvertsAMappedCampAndDeletesAnEmptyOne()
        {
            RHAH_SaveCleanup cleanup = new RHAH_SaveCleanup(Plan());
            XDocument document = Sample();

            cleanup.Clean(document);

            XElement mapped = WorldObject(document, "30");
            Assert.Equal("RimWorld.Planet.Site", (string)mapped.Attribute("Class"));
            Assert.Equal("Site", (string)mapped.Element("def"));
            Assert.Equal("30", (string)mapped.Element("ID"));
            Assert.Equal("MapParent", (string)mapped.Element("mapParent"));
            Assert.Null(mapped.Element("parts"));
            Assert.Null(mapped.Element("residents"));
            Assert.Null(FindBareId(document, "31"));
            XElement record = WorldObject(document, "32");
            Assert.Equal("Site", (string)record.Element("def"));
            Assert.Equal("32", (string)record.Element("ID"));
            Assert.Null(record.Element("parts"));
            Assert.Null(FindBareId(document, "33"));
        }

        [Fact]
        public void SecondPassDoesNotChangeTheDocument()
        {
            RHAH_SaveCleanup first = new RHAH_SaveCleanup(Plan());
            XDocument document = Sample();
            first.Clean(document);
            string once = document.ToString();

            new RHAH_SaveCleanup(Plan()).Clean(document);

            Assert.Equal(once, document.ToString());
        }

        [Fact]
        public void UnknownOwnedScalarAborts()
        {
            XDocument document = XDocument.Parse(
                "<savegame><game><unknown>RHAH_Unknown</unknown></game></savegame>");

            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => new RHAH_SaveCleanup(Plan()).Clean(document));

            Assert.Contains("RHAH_Unknown", error.Message);
        }

        [Fact]
        public void MissingReplacementAbortsBeforeWritingAName()
        {
            RHAH_SaveCleanupPlan plan = Plan();
            plan.Replacements.Remove("RHAH_PawnKind_Ratkin");
            XDocument document = Sample();

            Assert.Throws<InvalidOperationException>(() => new RHAH_SaveCleanup(plan).Clean(document));
            Assert.Equal("RHAH_PawnKind_Ratkin", Pawn(document, "11").Element("kindDef").Value);
        }

        static RHAH_SaveCleanupPlan Plan()
        {
            RHAH_SaveCleanupPlan plan = new RHAH_SaveCleanupPlan
            {
                PackageId = "nanaloveyuki.ratkin.hungerandhavoc"
            };
            string[] defs =
            {
                "RHAH_PawnKind_Ratkin", "RHAH_Faction_Neutral", "RHAH_History_A001", "RHAH_History_Y001",
                "RHAH_Xenotype_Ratkin", "RHAH_Suiyin", "RHAH_HungerMark", "RHAH_Trait_HardyLabor",
                "RHAH_Gene_ThinRations", "RHAH_GuanyinTu", "RHAH_Beg", "RHAH_RefugeeMassacre",
                "RHAH_RefugeeCamp", "RHAH_RecordSite", "RHAH_Approach", "RHAH_ChoiceRequest"
            };
            for (int i = 0; i < defs.Length; i++)
            {
                plan.OwnedDefs.Add(defs[i]);
            }

            plan.ThingDefs.Add("RHAH_GuanyinTu");
            plan.OwnedClasses.Add("HungerAndHavoc.Core.GameComponent_RHAH_Game");
            plan.OwnedClasses.Add("HungerAndHavoc.Narrative.NarrativeState");
            plan.OwnedClasses.Add("HungerAndHavoc.Core.MapComponent_RHAH_Map");
            plan.OwnedClasses.Add("HungerAndHavoc.Pawn.LordJob_RHAH_Visitor");
            plan.OwnedClasses.Add("HungerAndHavoc.Pawn.JobDriver_RHAH_Beg");
            plan.OwnedClasses.Add("HungerAndHavoc.Pawn.Area_RHAH_Relief");
            plan.OwnedClasses.Add("HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp");
            plan.OwnedClasses.Add("HungerAndHavoc.Incidents.WorldObject_RHAH_Approach");
            plan.OwnedClasses.Add("HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request");
            plan.Replacements.Add("RHAH_PawnKind_Ratkin", "RatkinColonist");
            plan.Replacements.Add("RHAH_Faction_Neutral", "Ancients");
            plan.Replacements.Add("RHAH_History_Y001", "Ratkin_Kid");
            plan.Replacements.Add("RHAH_History_A001", "Ratkin_Farmer");
            plan.Replacements.Add("RHAH_Xenotype_Ratkin", "Baseliner");
            plan.Replacements.Add("RHAH_Suiyin", "Randy");
            plan.GeneDefs.Add("RHAH_Gene_ThinRations");
            plan.CampObjectDefs.Add("RHAH_RefugeeCamp");
            plan.CampObjectDefs.Add("RHAH_RecordSite");
            plan.CampObjectClasses.Add("HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp");
            return plan;
        }
        static XElement Pawn(XDocument document, string id)
        {
            foreach (XElement kind in document.Descendants("kindDef"))
            {
                if ((string)kind.Parent.Element("id") == id)
                {
                    return kind.Parent;
                }
            }

            throw new InvalidOperationException("missing pawn " + id);
        }

        static XElement WorldObject(XDocument document, string id)
        {
            foreach (XElement node in document.Descendants())
            {
                if (node.Element("ID") != null && (string)node.Element("ID") == id && node.Element("def") != null)
                {
                    return node;
                }
            }

            throw new InvalidOperationException("missing world object " + id);
        }

        static XElement Only(XContainer container, string name)
        {
            XElement found = null;
            foreach (XElement node in container.Descendants(name))
            {
                if (found != null)
                {
                    throw new InvalidOperationException("more than one " + name);
                }

                found = node;
            }

            if (found == null)
            {
                throw new InvalidOperationException("missing " + name);
            }

            return found;
        }

        static string FactionLoadId(XDocument document)
        {
            foreach (XElement node in document.Descendants("loadID"))
            {
                if (node.Parent.Name == "faction" || node.Parent.Name == "li" && node.Parent.Parent?.Name == "factions")
                {
                    return node.Value;
                }
            }

            throw new InvalidOperationException("missing faction");
        }

        static bool HasValue(XDocument document, string name, string value)
        {
            foreach (XElement node in document.Descendants(name))
            {
                if (node.Value == value)
                {
                    return true;
                }
            }

            return false;
        }

        static bool HasClass(XDocument document, string className)
        {
            foreach (XElement node in document.Descendants())
            {
                if ((string)node.Attribute("Class") == className)
                {
                    return true;
                }
            }

            return false;
        }
        static bool HasNamed(XDocument document, string name)
        {
            foreach (XElement node in document.Descendants(name))
            {
                return true;
            }

            return false;
        }

        static bool HasQueuedJob(XDocument document)
        {
            foreach (XElement node in document.Descendants("jobs"))
            {
                foreach (XElement child in node.Elements())
                {
                    if (child.Name == "li")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool HasChild(XDocument document, string name)
        {
            foreach (XElement node in document.Descendants(name))
            {
                if (node.Elements().GetEnumerator().MoveNext())
                {
                    return true;
                }
            }

            return false;
        }

        static int CountChildren(XDocument document, string name)
        {
            foreach (XElement node in document.Descendants(name))
            {
                int count = 0;
                foreach (XElement child in node.Elements())
                {
                    count++;
                }

                return count;
            }

            return 0;
        }

        static XElement FirstChild(XElement parent)
        {
            foreach (XElement child in parent.Elements())
            {
                return child;
            }

            return null;
        }
        static XElement FirstNamed(XDocument document, string name)
        {
            foreach (XElement node in document.Descendants(name))
            {
                return node;
            }

            return null;
        }

        static XElement FindBareId(XDocument document, string id)
        {
            foreach (XElement node in document.Descendants())
            {
                if ((string)node.Element("ID") == id)
                {
                    return node;
                }
            }

            return null;
        }
        static string Dump(XDocument document, string name)
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            foreach (XElement node in document.Descendants(name))
            {
                text.Append(node.ToString());
            }

            return text.ToString();
        }

        static XDocument Sample()
        {
            return XDocument.Parse(@"<savegame>
<meta>
  <modIds><li>nanaloveyuki.ratkin.hungerandhavoc</li><li>Foreign</li></modIds>
  <modNames><li>Hunger</li><li>Foreign</li></modNames>
  <modSteamIds><li></li><li></li></modSteamIds>
</meta>
<game>
  <components>
    <li Class=""HungerAndHavoc.Core.GameComponent_RHAH_Game""><newContentDisabled>True</newContentDisabled></li>
    <li Class=""HungerAndHavoc.Narrative.NarrativeState"" />
    <li Class=""ForeignComponent"" />
  </components>
  <storytellerDef>RHAH_Suiyin</storytellerDef>
  <worldObjects>
    <worldObject Class=""HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp"">
      <def>RHAH_RefugeeCamp</def><ID>30</ID><mapParent>MapParent</mapParent>
      <parts><li><def>RHAH_RefugeeCamp</def></li></parts>
      <residents><li>Thing_11</li></residents>
    </worldObject>
    <worldObject Class=""HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp"">
      <def>RHAH_RefugeeCamp</def><ID>31</ID>
    </worldObject>
    <worldObject Class=""RimWorld.Planet.Site"">
      <def>RHAH_RecordSite</def><ID>32</ID><mapParent>MapParent</mapParent>
      <parts><li><def>RHAH_RecordSite</def></li></parts>
    </worldObject>
    <worldObject Class=""RimWorld.Planet.Site"">
      <def>RHAH_RecordSite</def><ID>33</ID>
    </worldObject>
    <worldObject Class=""HungerAndHavoc.Incidents.WorldObject_RHAH_Approach"">
      <def>RHAH_Approach</def><ID>40</ID>
    </worldObject>
  </worldObjects>
  <maps>
    <li>
      <components><li Class=""HungerAndHavoc.Core.MapComponent_RHAH_Map"" /></components>
      <areas><li Class=""HungerAndHavoc.Pawn.Area_RHAH_Relief""><ID>7</ID></li><li Class=""Verse.Area_Home""><ID>1</ID></li></areas>
      <lords><li><loadID>4</loadID><lordJob Class=""HungerAndHavoc.Pawn.LordJob_RHAH_Visitor"" /></li></lords>
    </li>
  </maps>
  <worldPawns>
    <li>
      <kindDef>RHAH_PawnKind_Ratkin</kindDef><id>11</id>
      <faction>Faction_99</faction>
      <story><childhood>RHAH_History_Y001</childhood><adulthood>RHAH_History_A001</adulthood></story>
      <xenotype>RHAH_Xenotype_Ratkin</xenotype>
      <genes>
        <endogenes><li><def>ForeignGene</def><loadID>3</loadID></li><li><def>RHAH_Gene_ThinRations</def><loadID>8</loadID></li></endogenes>
      </genes>
      <hediffSet><hediffs><li><def>RHAH_HungerMark</def><loadID>5</loadID></li><li><def>Cut</def><loadID>6</loadID></li></hediffs></hediffSet>
      <storyTraits><allTraits><li><def>RHAH_Trait_HardyLabor</def></li><li><def>Kind</def></li></allTraits></storyTraits>
      <jobs><curJob><def>RHAH_Beg</def><loadID>9</loadID></curJob><curDriver Class=""HungerAndHavoc.Pawn.JobDriver_RHAH_Beg"" /><jobs><li><def>RHAH_Beg</def></li></jobs></jobs>
    </li>
    <li><kindDef>Colonist</kindDef><id>22</id><faction>Faction_Player</faction></li>
  </worldPawns>
  <factions><li><def>RHAH_Faction_Neutral</def><loadID>99</loadID><factionDef>RHAH_Faction_Neutral</factionDef></li></factions>
  <letters><li Class=""HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request""><def>RHAH_ChoiceRequest</def><ID>12</ID></li></letters>
  <quests><li><id>15</id><root>RHAH_RefugeeMassacre</root><parts><li /></parts></li><li><id>16</id><root>Hospitality</root></li></quests>
  <things><thing><def>RHAH_GuanyinTu</def><id>18</id></thing><thing><def>MealSimple</def><id>19</id></thing></things>
  <reservations><li><target>Thing_18</target></li><li><target>Thing_19</target></li></reservations>
  <dictionary><keys><li>Thing_18</li><li>Thing_19</li><li>Area_7_RHAH_Relief</li></keys><values><li>a</li><li>b</li><li>c</li></values></dictionary>
</game>
</savegame>");
        }
    }
}
