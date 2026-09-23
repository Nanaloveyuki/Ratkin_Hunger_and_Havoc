using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class CompRHAH_PawnSaveContractTests
    {
        static readonly string[] SaveKeys =
        {
            "sourceIncidentDisplayId",
            "spawnBatchId",
            "relationshipGroupId",
            "role",
            "lifecycle",
            "hasBeenFed",
            "leaveAfterGameTick",
            "carriesPlague",
            "attitudeAtArrival",
            "attitude",
            "parentPawnLoadId",
            "childPawnLoadIds",
            "droppedChildLoadIds",
            "gateOverrides",
            "extraData"
        };

        [Fact]
        public void CompExposeDataUsesRebuiltDisplayIdKey()
        {
            string source = File.ReadAllText(CompPath());
            Assert.Contains("CompExposeData", source);
            Assert.Contains("\"sourceIncidentDisplayId\"", source);
            Assert.DoesNotContain("\"sourceIncidentId\"", source);
            foreach (string key in SaveKeys)
            {
                Assert.Contains("\"" + key + "\"", source);
            }

            Assert.Contains("LoadSaveMode.PostLoadInit", source);
            Assert.Contains("spawnBatchId", source);
            Assert.Contains("0", DefaultLook(source, "spawnBatchId"));
            Assert.Contains("-1", DefaultLook(source, "leaveAfterGameTick"));
        }

        static string DefaultLook(string source, string key)
        {
            string token = "\"" + key + "\"";
            int index = source.IndexOf(token);
            Assert.True(index >= 0, key);
            int end = source.IndexOf(')', index);
            Assert.True(end > index, key);
            return source.Substring(index, end - index);
        }

        static string CompPath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Identity", "CompRHAH_Pawn.cs"));
        }
    }
}
