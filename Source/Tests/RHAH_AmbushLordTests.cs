using System.Collections.Generic;
using HungerAndHavoc.Trade;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Xunit;

namespace HungerAndHavoc.Tests
{
    using Pawn = Verse.Pawn;
    using Verse.AI;
    public class RHAH_AmbushLordTests : System.IDisposable
    {
        readonly Game previousGame = Current.Game;
        readonly object previousPrefs = typeof(Prefs).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);

        public void Dispose()
        {
            Current.Game = previousGame;
            typeof(Prefs).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, previousPrefs);
        }

        [Fact]
        public void WrongFactionAttackerGetsNoLord()
        {
            RHAH_AmbushLordFixture.Bind();
            Faction faction = RHAH_AmbushLordFixture.Faction();
            Faction other = RHAH_AmbushLordFixture.Faction();
            Map map = RHAH_AmbushLordFixture.Map();
            Pawn owned = RHAH_AmbushLordFixture.Attacker(faction);
            Pawn foreign = RHAH_AmbushLordFixture.Attacker(other);
            List<Pawn> attackers = new List<Pawn> { owned, foreign };

            Assert.False(TradeEventRouter.TryCreateAssaultLord(faction, map, attackers));

            Assert.Null(owned.GetLord());
            Assert.Null(foreign.GetLord());
            Assert.Empty(map.lordManager.lords);
        }

        [Fact]
        public void EmptyOrMissingMapDoesNotCreateALord()
        {
            RHAH_AmbushLordFixture.Bind();
            Faction faction = RHAH_AmbushLordFixture.Faction();
            Pawn attacker = RHAH_AmbushLordFixture.Attacker(faction);

            Assert.False(TradeEventRouter.TryCreateAssaultLord(faction, RHAH_AmbushLordFixture.Map(), new List<Pawn>()));
            Assert.False(TradeEventRouter.TryCreateAssaultLord(faction, null, new List<Pawn> { attacker }));
            Assert.False(TradeEventRouter.TryCreateAssaultLord(null, RHAH_AmbushLordFixture.Map(), new List<Pawn> { attacker }));
            Assert.Null(attacker.GetLord());
        }
    }

    static class RHAH_AmbushLordFixture
    {
        public static void Bind()
        {
            Current.Game = (Game)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Game));
            Current.Game.uniqueIDsManager = new UniqueIDsManager();
            Current.Game.tickManager = new TickManager();

            PrefsData data = new PrefsData();
            data.adaptiveTrainingEnabled = false;
            data.devMode = false;
            typeof(Prefs).GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, data);
        }

        public static Faction Faction()
        {
            Faction faction = new Faction();
            faction.def = new FactionDef();
            faction.def.humanlikeFaction = true;
            faction.def.autoFlee = false;
            faction.def.isPlayer = false;
            faction.Name = "Ambush";
            return faction;
        }

        public static Map Map()
        {
            Map map = new Map();
            map.info = new MapInfo();
            map.info.isPocketMap = true;
            map.pocketTileInfo = new Tile();
            map.pocketTileInfo.PrimaryBiome = new BiomeDef();
            map.pocketTileInfo.PrimaryBiome.canExitMap = true;
            map.events = new MapEvents(map);
            map.lordManager = new LordManager(map);
            return map;
        }

        public static Pawn Attacker(Faction faction)
        {
            Pawn pawn = new Pawn();
            pawn.mindState = new Pawn_MindState();
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.def = (ThingDef)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ThingDef));
            pawn.def.category = ThingCategory.Pawn;
            pawn.SetFactionDirect(faction);
            return pawn;
        }

    }
}
