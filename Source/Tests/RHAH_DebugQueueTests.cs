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
    }
}
