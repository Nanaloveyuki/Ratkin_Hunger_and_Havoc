using System.IO;
using System.Runtime.CompilerServices;
using HungerAndHavoc.Pawn;
using Verse.AI;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_JobGiverTests
    {
        static RHAH_JobGiverTests()
        {
            RimWorldAssemblies.EnsureResolved();
        }

        [Fact]
        public void TypeNamesMatchXmlContract()
        {
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Beg", typeof(JobGiver_RHAH_Beg).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Steal", typeof(JobGiver_RHAH_Steal).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Gnaw", typeof(JobGiver_RHAH_Gnaw).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Feed", typeof(JobGiver_RHAH_Feed).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobGiver_RHAH_Leave", typeof(JobGiver_RHAH_Leave).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobDriver_RHAH_Beg", typeof(JobDriver_RHAH_Beg).FullName);
            Assert.Equal("HungerAndHavoc.Pawn.JobDriver_RHAH_Gnaw", typeof(JobDriver_RHAH_Gnaw).FullName);
            Assert.True(typeof(ThinkNode_JobGiver).IsAssignableFrom(typeof(JobGiver_RHAH_Beg)));
            Assert.True(typeof(ThinkNode_JobGiver).IsAssignableFrom(typeof(JobGiver_RHAH_Steal)));
            Assert.True(typeof(ThinkNode_JobGiver).IsAssignableFrom(typeof(JobGiver_RHAH_Gnaw)));
            Assert.True(typeof(ThinkNode_JobGiver).IsAssignableFrom(typeof(JobGiver_RHAH_Feed)));
            Assert.True(typeof(ThinkNode_JobGiver).IsAssignableFrom(typeof(JobGiver_RHAH_Leave)));
            Assert.True(typeof(JobDriver).IsAssignableFrom(typeof(JobDriver_RHAH_Beg)));
            Assert.True(typeof(JobDriver).IsAssignableFrom(typeof(JobDriver_RHAH_Gnaw)));
        }

        [Fact]
        public void TryCreateRejectsNonVisitor()
        {
            Assert.Null(JobGiver_RHAH_Beg.TryCreate(null));
            Assert.Null(JobGiver_RHAH_Steal.TryCreate(null));
            Assert.Null(JobGiver_RHAH_Gnaw.TryCreate(null));
            Assert.Null(JobGiver_RHAH_Feed.TryCreate(null));
            Assert.Null(JobGiver_RHAH_Leave.TryCreate(null));
        }

        [Fact]
        public void GiversAskVisitorThenGateAndTryGiveJobCallsTryCreate()
        {
            AssertVisitorThenAllows("JobGiver_RHAH_Beg.cs", "RHAH_BehaviorGate.Beg");
            AssertVisitorThenAllows("JobGiver_RHAH_Steal.cs", "RHAH_BehaviorGate.Steal");
            AssertVisitorThenAllows("JobGiver_RHAH_Gnaw.cs", "RHAH_BehaviorGate.Gnaw");
            AssertVisitorThenAllows("JobGiver_RHAH_Feed.cs", "RHAH_BehaviorGate.FeedFromRelief");
            AssertVisitorThenAllows("JobGiver_RHAH_Leave.cs", "RHAH_BehaviorGate.ExitMap");
            string leave = ReadPawn("JobGiver_RHAH_Leave.cs");
            Assert.Contains("RHAH_BehaviorGate.LeaveAfterFed", leave);
        }

        [Fact]
        public void CustomJobsUseDefOfAndVanillaFallbacks()
        {
            string beg = ReadPawn("JobGiver_RHAH_Beg.cs");
            string gnaw = ReadPawn("JobGiver_RHAH_Gnaw.cs");
            string steal = ReadPawn("JobGiver_RHAH_Steal.cs");
            string feed = ReadPawn("JobGiver_RHAH_Feed.cs");
            string leave = ReadPawn("JobGiver_RHAH_Leave.cs");
            Assert.Contains("RHAH_DefOf.RHAH_Beg", beg);
            Assert.Contains("RHAH_DefOf.RHAH_Gnaw", gnaw);
            Assert.True(
                steal.Contains("JobDefOf.Steal") || steal.Contains("JobDefOf.TakeFromOtherInventory"),
                "Steal must use a vanilla JobDef");
            Assert.Contains("JobDefOf.Ingest", feed);
            Assert.Contains("JobDefOf.Goto", leave);
            Assert.Contains("exitMapOnArrival", leave);
        }

        [Fact]
        public void DriversImplementMakeNewToils()
        {
            string beg = ReadPawn("JobDriver_RHAH_Beg.cs");
            string gnaw = ReadPawn("JobDriver_RHAH_Gnaw.cs");
            Assert.Contains("protected override IEnumerable<Toil> MakeNewToils()", beg);
            Assert.Contains("protected override IEnumerable<Toil> MakeNewToils()", gnaw);
            Assert.Contains("Toils_Goto.GotoThing", beg);
            Assert.Contains("Toils_Goto.GotoThing", gnaw);
            Assert.Contains("WaitWith", beg);
            Assert.Contains("CurLevel", gnaw);
        }

        static void AssertVisitorThenAllows(string fileName, string gate)
        {
            string source = ReadPawn(fileName);
            Assert.Contains("protected override Job TryGiveJob(Verse.Pawn pawn)", source);
            Assert.Contains("return TryCreate(pawn);", source);
            Assert.Contains("internal static Job TryCreate(Verse.Pawn pawn)", source);
            Assert.DoesNotContain("using Pawn = Verse.Pawn", source);
            bool visitor = source.Contains("RHAH_Api.IsVisitor") ||
                           source.Contains("RHAH_VisitorGate");
            Assert.True(visitor, fileName + " must check IsVisitor or RHAH_VisitorGate");
            Assert.Contains("Allows", source);
            Assert.Contains(gate, source);
            int visitorIndex = IndexOfVisitorCheck(source);
            int allowsIndex = source.IndexOf("Allows");
            Assert.True(
                visitorIndex >= 0 && allowsIndex > visitorIndex,
                fileName + " must ask visitor then Allows");
        }

        static int IndexOfVisitorCheck(string source)
        {
            int api = source.IndexOf("RHAH_Api.IsVisitor");
            int gate = source.IndexOf("RHAH_VisitorGate");
            if (api < 0)
            {
                return gate;
            }

            if (gate < 0)
            {
                return api;
            }

            return api < gate ? api : gate;
        }

        static string ReadPawn(string fileName, [CallerFilePath] string testFile = null)
        {
            string testsDir = Path.GetDirectoryName(testFile);
            string path = Path.GetFullPath(Path.Combine(testsDir, "..", "Pawn", fileName));
            Assert.True(File.Exists(path), "missing " + path);
            return File.ReadAllText(path);
        }
    }
}
