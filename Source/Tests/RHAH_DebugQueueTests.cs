using System.IO;
using System.Runtime.CompilerServices;
using HungerAndHavoc.Core;
using HungerAndHavoc.Incidents;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_DebugQueueTests
    {
        [Fact]
        public void QueueingADebugIncidentDoesNotSpawnIt()
        {
            GameComponent_RHAH_Game game = new GameComponent_RHAH_Game(null);
            Assert.True(game.QueueIncident("I-001", 240f));
            Assert.Equal(new[] { "I-001" }, game.PendingIncidentDisplayIds);
        }

        [Fact]
        public void StaggeredQueueDoesNotSpawnBetweenBatchTicks()
        {
            RHAH_Settings previous = RHAH_Mod.Settings;
            RHAH_Settings settings = previous ?? new RHAH_Settings();
            bool stagger = settings.staggerGeneration;
            settings.staggerGeneration = true;
            RHAH_Mod.Settings = settings;
            try
            {
                GameComponent_RHAH_Game game = new GameComponent_RHAH_Game(null);
                Assert.True(game.QueueIncident("I-004", 80f));
                Assert.False(game.TrySpawnPending(1));
                Assert.Equal(new[] { "I-004" }, game.PendingIncidentDisplayIds);
            }
            finally
            {
                settings.staggerGeneration = stagger;
                RHAH_Mod.Settings = previous;
            }
        }

        [Fact]
        public void DisabledDebugIncidentCannotQueue()
        {
            RHAH_Settings settings = new RHAH_Settings();
            settings.SetIncidentEnabled("I-001", false);
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId("I-001");
            Assert.False(RHAH_Scheduler.CanQueueDebug(entry, settings, gameLoaded: true, targetReady: true));
        }

        [Fact]
        public void EnabledDebugIncidentCanQueueWithoutSpawning()
        {
            RHAH_Settings settings = new RHAH_Settings();
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId("I-001");
            Assert.True(RHAH_Scheduler.CanQueueDebug(entry, settings, gameLoaded: true, targetReady: true));
            Assert.False(RHAH_Scheduler.CanQueueDebug(entry, settings, gameLoaded: false, targetReady: true));
            Assert.False(RHAH_Scheduler.CanQueueDebug(entry, settings, gameLoaded: true, targetReady: false));
        }

        [Fact]
        public void ForcedPauseDoesNotBlockTheFrameDrain()
        {
            Assert.True(RHAH_Scheduler.ShouldDrainOnFrame(paused: true, forcePause: false, pending: 1));
            Assert.False(RHAH_Scheduler.ShouldDrainOnFrame(paused: true, forcePause: true, pending: 1));
            Assert.False(RHAH_Scheduler.ShouldDrainOnFrame(paused: false, forcePause: false, pending: 1));
            Assert.False(RHAH_Scheduler.ShouldDrainOnFrame(paused: true, forcePause: false, pending: 0));
        }

        [Fact]
        public void DebugTriggerLogsARejectionWithoutQueueing()
        {
            RHAH_Settings settings = new RHAH_Settings();
            settings.SetIncidentEnabled("I-001", false);
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId("I-001");
            Assert.False(RHAH_Scheduler.CanQueueDebug(entry, settings, gameLoaded: true, targetReady: true));
            Assert.Equal(
                "[RHAH] Debug trigger rejected I-001. content=True catalog=True game=True target=True enabled=False",
                RHAH_Scheduler.DebugRejectText("I-001", true, true, true, true, false));
        }

        [Fact]
        public void MultiPawnIncidentRegistersBatchAfterTheWholeBatch()
        {
            string source = File.ReadAllText(IncidentFactsPath());
            int create = source.IndexOf("RHAH_PawnFactory.Create");
            int deferred = source.IndexOf("registerBatch: false", create);
            int commit = source.IndexOf("RHAH_Runtime.RegisterBatch(context.Map, context.SpawnBatchId)", deferred);
            Assert.True(create >= 0, "RHAH_IncidentFacts must create pawns through RHAH_PawnFactory");
            Assert.True(deferred > create, "multi-pawn incident creation must defer batch registration");
            Assert.True(commit > deferred, "batch registration must happen after deferred creation");
        }

        [Fact]
        public void QueueFailurePolicyDropsTerminalWorkerFailures()
        {
            string source = File.ReadAllText(GameComponentPath());
            int catchBlock = source.IndexOf("catch (Exception exception)");
            int catchDrop = source.IndexOf("DropPending();", catchBlock);
            int falseBlock = source.IndexOf("if (!executed)");
            int falseDrop = source.IndexOf("DropPending();", falseBlock);
            Assert.True(catchBlock >= 0 && catchDrop > catchBlock,
                "worker exceptions must consume the failed queue item");
            Assert.True(falseBlock >= 0 && falseDrop > falseBlock,
                "worker false results must consume the failed queue item");
            Assert.Contains("No usable map", source);
            Assert.Contains("return false;", source.Substring(source.IndexOf("No usable map")));
        }

        static string GameComponentPath([CallerFilePath] string testFile = null)
        {
            return Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(testFile), "..", "Core", "GameComponent_RHAH_Game.cs"));
        }

        static string IncidentFactsPath([CallerFilePath] string testFile = null)
        {
            return Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(testFile), "..", "Incidents", "RHAH_IncidentFacts.cs"));
        }
    }
}
