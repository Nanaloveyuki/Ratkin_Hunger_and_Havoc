using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_ChoiceLetterTests
    {
        [Fact]
        public void ChoiceLettersInstantiateTheirOwnClasses()
        {
            XmlDocument letters = Load("1.6/Defs/LetterDefs/RHAH_Letters.xml");
            Assert.Equal(
                "HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request",
                LetterClass(letters, "RHAH_ChoiceRequest"));
            Assert.Equal(
                "HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Visitors",
                LetterClass(letters, "RHAH_ChoiceVisitors"));
            Assert.Null(letters.DocumentElement.SelectSingleNode("LetterDef[@ParentName]"));

            XmlDocument kinds = Load("1.6/Defs/PawnKindDefs/RHAH_PawnKinds.xml");
            XmlNode kind = kinds.SelectSingleNode("//PawnKindDef[defName='RHAH_PawnKind_Ratkin']");
            Assert.NotNull(kind);
            Assert.Equal("0~0", kind.SelectSingleNode("initialWillRange").InnerText);
            Assert.Equal("0~0", kind.SelectSingleNode("initialResistanceRange").InnerText);

            string facts = File.ReadAllText(Source("Incidents/RHAH_IncidentFacts.cs"));
            int send = facts.IndexOf("static void SendLetter");
            Assert.True(send >= 0, "SendLetter missing");
            string body = facts.Substring(send);
            Assert.Contains("RHAH_DefOf.RHAH_ChoiceVisitors", body);
            Assert.Contains("RHAH_DefOf.RHAH_ChoiceRequest", body);
            Assert.DoesNotContain("LetterDefOf.NeutralEvent", body);
            Assert.Contains(
                "public static LetterDef RHAH_ChoiceRequest",
                File.ReadAllText(Source("Core/RHAH_DefOf.cs")));
            Assert.Contains(
                "public static LetterDef RHAH_ChoiceVisitors",
                File.ReadAllText(Source("Core/RHAH_DefOf.cs")));

            Assert.Contains("ChoiceKey(record)", body);
            Assert.Contains("RHAH_Choice_Visitors", body);
            Assert.Contains("RHAH_Choice_Baby", body);
            Assert.Contains("RHAH_Choice_SimpleMeal", body);
            Assert.DoesNotContain("ThingDefName(record.Kind)", body);
        }

        static string LetterClass(XmlDocument document, string defName)
        {
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                XmlNode name = node.SelectSingleNode("defName");
                if (name != null && name.InnerText == defName)
                {
                    XmlNode letterClass = node.SelectSingleNode("letterClass");
                    return letterClass == null ? null : letterClass.InnerText;
                }
            }

            return null;
        }

        static XmlDocument Load(string relative)
        {
            XmlDocument document = new XmlDocument();
            document.Load(Path.Combine(Root(), relative));
            return document;
        }

        static string Source(string relative, [CallerFilePath] string testFile = null)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testFile), "..", relative));
        }

        static string Root([CallerFilePath] string testFile = null)
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testFile), "..", ".."));
        }
    }
}
