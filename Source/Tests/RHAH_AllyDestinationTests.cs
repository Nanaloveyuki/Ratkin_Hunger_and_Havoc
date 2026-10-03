using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using HungerAndHavoc.Incidents;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public sealed class RHAH_AllyDestinationTests
    {
        [Fact]
        public void PlayerOnlyWorldHasNoAllyDestinationOrRelationErrors()
        {
            Game previousGame = Current.Game;
            int previousErrors = Log.Messages.Where(message => message.type == LogMessageType.Error).Sum(message => message.repeats);
            try
            {
                Current.Game = (Game)FormatterServices.GetUninitializedObject(typeof(Game));
                World world = (World)FormatterServices.GetUninitializedObject(typeof(World));
                world.factionManager = new FactionManager();
                world.worldObjects = new WorldObjectsHolder();
                Current.Game.World = world;
                Faction player = new Faction
                {
                    def = new FactionDef { isPlayer = true, humanlikeFaction = true },
                    loadID = 70
                };
                world.factionManager.AllFactionsListForReading.Add(player);
                typeof(FactionManager).GetField("ofPlayer", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(world.factionManager, player);

                Assert.Null(RHAH_ChoiceRuntime.AllyDestination());
                Assert.Equal(previousErrors, Log.Messages.Where(message => message.type == LogMessageType.Error).Sum(message => message.repeats));
            }
            finally
            {
                Current.Game = previousGame;
            }
        }
    }
}
