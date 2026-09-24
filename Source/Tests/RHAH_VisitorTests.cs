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
            RHAH_Api.Bind(new RHAH_ApiHost());
            try
            {
                Assert.False(RHAH_Api.IsVisitor(null));
                Assert.False(RHAH_VisitorGate.IsVisitor(null));
                Assert.False(RHAH_VisitorGate.Allows(null, RHAH_BehaviorGate.Beg));
            }
            finally
            {
                RHAH_Api.Bind(null);
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
            Assert.True(release >= 0, "RHAH_ApiHost.ReleaseToColony missing");
            string body = source.Substring(release);
            int set = body.IndexOf("SetLifecycle(pawn, RHAH_Lifecycle.Released)");
            int notify = body.IndexOf("NotifyReleased");
            Assert.True(set >= 0, "ReleaseToColony must SetLifecycle Released");
            Assert.True(notify > set, "NotifyReleased must follow SetLifecycle Released");
        }

        [Fact]
        public void JobGiverAsksIsVisitorThenAllowsThenTryCreate()
        {
            string source = File.ReadAllText(PawnPath("JobGiver_RHAH_Visitor.cs"));
            int visitor = source.IndexOf("RHAH_Api.IsVisitor");
            int allows = source.IndexOf("RHAH_Api.Allows");
            Assert.True(visitor >= 0, "JobGiver_RHAH_Visitor must ask IsVisitor");
            Assert.True(allows > visitor, "JobGiver_RHAH_Visitor must ask Allows after IsVisitor");
            Assert.Contains("JobGiver_RHAH_Feed.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Beg.TryCreate(pawn)", source);
            Assert.Contains("JobGiver_RHAH_Steal.TryCreate(pawn)", source);
            Assert.DoesNotContain("JobGiver_RHAH_Gnaw.TryCreate(pawn)", source);
            Assert.Contains("AllowsModBehavior", source);
            Assert.Contains("JobGiver_RHAH_Leave.TryCreate(pawn)", source);
        }

        [Fact]
        public void ThinkNodeSatisfiedUsesIsVisitor()
        {
            string source = File.ReadAllText(PawnPath("ThinkNode_ConditionalRHAH_Visitor.cs"));
            Assert.Contains("RHAH_Api.IsVisitor(pawn)", source);
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
            Assert.Contains("RHAH_DefOf.RHAH_VisitorSeek", source);
            Assert.Contains("RHAH_DefOf.RHAH_VisitorLeave", source);
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
            int visitor = source.IndexOf("RHAH_Api.IsVisitor");
            int allows = source.IndexOf("RHAH_Api.Allows");
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
            Assert.DoesNotContain("RHAH_Feeding.TryComplete",
                File.ReadAllText(PawnPath("JobDriver_RHAH_Gnaw.cs")));
            Assert.Contains("RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving)",
                File.ReadAllText(PawnPath("JobGiver_RHAH_Leave.cs")));
            Assert.Contains("HungerAndHavoc.Pawn.JobGiver_RHAH_Leave", duty);
            Assert.DoesNotContain("JobGiver_ExitMapBest", duty);
            Assert.Contains("NoFoodWaitExpired",
                File.ReadAllText(PawnPath("JobGiver_RHAH_Leave.cs")));
            Assert.Contains("RHAH_BehaviorGate.Carry",
                File.ReadAllText(PawnPath("JobGiver_RHAH_Leave.cs")));
            Assert.Contains("Job carry = CarryDependent(pawn);",
                File.ReadAllText(PawnPath("JobGiver_RHAH_Leave.cs")));
        }

        [Fact]
        public void FeedingDeathAndExpiredStayCloseLifecycle()
        {
            string feeding = File.ReadAllText(PawnPath("RHAH_Feeding.cs"));
            Assert.Contains("RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Fed)", feeding);
            Assert.Contains("SetLeaveAfter", feeding);

            string patch = File.ReadAllText(PawnPath("RHAH_LifecyclePatch.cs"));
            Assert.Contains("Toils_Ingest.FinalizeIngest", patch);
            Assert.Contains("RHAH_Feeding.TryComplete(ingester)", patch);
            Assert.Contains("nameof(Verse.Pawn.Kill)", patch);
            Assert.Contains("RHAH_Lifecycle.Dead", patch);
            Assert.Contains("NotifyDead", patch);

            string menu = File.ReadAllText(PawnPath("RHAH_VisitorExpelMenu.cs"));
            Assert.Contains("RHAH_Feeding.TryComplete(clickedPawn)", menu);
            Assert.DoesNotContain("SetLifecycle(clickedPawn, RHAH_Lifecycle.Fed)", menu);

            string stay = File.ReadAllText(PawnPath("RHAH_VisitorStay.cs"));
            int clear = stay.IndexOf("RHAH_VisitorGroup.NotifyReleased(pawn)");
            int leaving = stay.IndexOf("RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Leaving)");
            Assert.True(clear >= 0 && leaving > clear, "expired stay must clear the lord before Leaving");
            Assert.Contains("RHAH_StayKind.Recruit", File.ReadAllText(IncidentPath("RHAH_ChoiceRuntime.cs")));
            Assert.DoesNotContain("MarkFed", File.ReadAllText(PawnPath("JobGiver_RHAH_Visitor.cs")));
        }


        static string HostPath([CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Identity", "RHAH_ApiHost.cs"));
        }

        static string PawnPath(string fileName, [CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Pawn", fileName));
        }

        static string IncidentPath(string fileName, [CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            return Path.GetFullPath(Path.Combine(testsDir, "..", "Incidents", fileName));
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
