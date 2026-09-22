using System.IO;
using System.Runtime.CompilerServices;
using HungerAndHavoc.Api;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Pawn;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_VisitorTests
    {
        static RHAH_VisitorTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void HostBind_NonVisitorGateFalse()
        {
            HungerAndHavocApi.Bind(new HungerApiHost());
            try
            {
                Assert.False(HungerAndHavocApi.IsVisitor(null));
                Assert.False(RHAH_VisitorGate.IsVisitor(null));
                Assert.False(RHAH_VisitorGate.Allows(null, HungerBehaviorGate.Beg));
            }
            finally
            {
                HungerAndHavocApi.Bind(null);
            }
        }

        [Fact]
        public void TypeNamesMatchPawnContract()
        {
            Assert.Equal("HungerAndHavoc.Pawn.LordJob_RHAH_Visitor", typeof(LordJob_RHAH_Visitor).FullName);
            Assert.Equal(
                "HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor",
                typeof(ThinkNode_ConditionalRHAH_Visitor).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Visitor", typeof(JobGiver_RHAH_Visitor).FullName);
            Assert.True(typeof(LordJob_RHAH_Visitor).IsPublic);
            Assert.True(typeof(ThinkNode_ConditionalRHAH_Visitor).IsPublic);
            Assert.True(typeof(JobGiver_RHAH_Visitor).IsPublic);
            Assert.False(typeof(RHAH_VisitorGate).IsPublic);
            Assert.False(typeof(RHAH_VisitorGroup).IsPublic);
        }

        [Fact]
        public void ReleaseToColonyCallsNotifyReleasedAfterSetLifecycle()
        {
            string source = File.ReadAllText(HostPath());
            int release = source.IndexOf("public bool ReleaseToColony");
            Assert.True(release >= 0, "HungerApiHost.ReleaseToColony missing");
            string body = source.Substring(release);
            int set = body.IndexOf("SetLifecycle(pawn, HungerLifecycle.Released)");
            int notify = body.IndexOf("NotifyReleased");
            Assert.True(set >= 0, "ReleaseToColony must SetLifecycle Released");
            Assert.True(notify > set, "NotifyReleased must follow SetLifecycle Released");
        }

        [Fact]
        public void JobGiverAsksIsVisitorThenAllowsThenTryCreate()
        {
            string source = File.ReadAllText(PawnPath("JobGiver_RHAH_Visitor.cs"));
            int visitor = source.IndexOf("HungerAndHavocApi.IsVisitor");
            int allows = source.IndexOf("HungerAndHavocApi.Allows");
            Assert.True(visitor >= 0, "JobGiver_RHAH_Visitor must ask IsVisitor");
            Assert.True(allows > visitor, "JobGiver_RHAH_Visitor must ask Allows after IsVisitor");
            Assert.Contains("JobGiver_RHAH_Feed.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Beg.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Steal.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Gnaw.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Leave.TryCreate(pawn)", source);
        }

        [Fact]
        public void ThinkNodeSatisfiedUsesIsVisitor()
        {
            string source = File.ReadAllText(PawnPath("ThinkNode_ConditionalRHAH_Visitor.cs"));
            Assert.Contains("HungerAndHavocApi.IsVisitor(pawn)", source);
        }

        [Fact]
        public void NotifyReleasedRemovesLordAndDuty()
        {
            string source = File.ReadAllText(PawnPath("RHAH_VisitorGroup.cs"));
            int notify = source.IndexOf("internal static void NotifyReleased");
            Assert.True(notify >= 0, "NotifyReleased missing");
            string body = source.Substring(notify);
            Assert.Contains("GetLord", body);
            Assert.Contains("RemovePawn", body);
            Assert.Contains("mindState.duty = null", body);
        }

        [Fact]
        public void LordJobExposesFactionWaitSpotAndHostileExit()
        {
            string source = File.ReadAllText(PawnPath("LordJob_RHAH_Visitor.cs"));
            Assert.Contains("LostImportantReferenceDuringLoading", source);
            Assert.Contains("\"faction\"", source);
            Assert.Contains("\"waitSpot\"", source);
            Assert.Contains("Trigger_Memo(\"RHAH_Leave\")", source);
            Assert.Contains("LordToil_RHAH_VisitorLeave", source);
            Assert.DoesNotContain("LordToil_ExitMapAndDefendSelf", source);
            Assert.DoesNotContain("Trigger_BecamePlayerEnemy", source);
            Assert.DoesNotContain("Trigger_PawnKilled", source);
            Assert.Contains("HungerAndHavocDefOf.RHAH_VisitorSeek", source);
            Assert.Contains("HungerAndHavocDefOf.RHAH_VisitorLeave", source);
            Assert.DoesNotContain("LordJob_BegForItems", source);
            Assert.DoesNotContain("CheckIdeology", source);
        }

        [Fact]
        public void ThinkTreeInsertsHumanlikePostDutyWithoutHumanlikeXpath()
        {
            string xml = File.ReadAllText(ThinkTreePath());
            Assert.Contains("<insertTag>Humanlike_PostDuty</insertTag>", xml);
            Assert.Contains("HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor", xml);
            Assert.Contains("HungerAndHavoc.Pawn.JobGiver_RHAH_Visitor", xml);

            string root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(ThinkTreePath()), "..", ".."));
            foreach (string file in Directory.GetFiles(root, "*.xml", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                Assert.DoesNotContain("Humanlike.xml", text);
            }
        }

        [Fact]
        public void VisitorGateWrapsIsVisitorThenAllowsWithoutLinq()
        {
            string source = File.ReadAllText(PawnPath("RHAH_VisitorGate.cs"));
            int visitor = source.IndexOf("HungerAndHavocApi.IsVisitor");
            int allows = source.IndexOf("HungerAndHavocApi.Allows");
            Assert.True(visitor >= 0 && allows > visitor);
            Assert.DoesNotContain("System.Linq", source);
        }
        [Fact]
        public void VisitorSeekDutyUsesUnifiedJobGiverAndLifecycleHooksExist()
        {
            string duty = File.ReadAllText(Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(ThinkTreePath()), "..", "DutyDefs", "RHAH_Duties.xml")));
            Assert.Contains("HungerAndHavoc.Pawn.JobGiver_RHAH_Visitor", duty);
            Assert.DoesNotContain("JobGiver_RHAH_Feed", duty);
            Assert.DoesNotContain("JobGiver_RHAH_Beg", duty);
            Assert.Contains("RHAH_Feeding.TryComplete(pawn)",
                File.ReadAllText(PawnPath("JobDriver_RHAH_Gnaw.cs")));
            Assert.Contains("HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Leaving)",
                File.ReadAllText(PawnPath("JobGiver_RHAH_Leave.cs")));
        }

        static string HostPath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Identity", "HungerApiHost.cs"));
        }

        static string PawnPath(string fileName, [CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Pawn", fileName));
        }

        static string ThinkTreePath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(
                testsDir,
                "..",
                "..",
                "1.6",
                "Defs",
                "ThinkTreeDefs",
                "RHAH_ThinkTrees.xml"));
        }
    }
}
