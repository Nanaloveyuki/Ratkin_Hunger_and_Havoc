using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Incidents;
using HungerAndHavoc.Pawn;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_VisitorCustodyTests : IDisposable
    {
        readonly Game previousGame;
        readonly ProgramState previousProgramState;
        readonly IRHAH_ApiHost previousHost;
        readonly List<IRHAH_PawnBehavior> previousBehaviors;
        readonly Map map;
        readonly Faction player;
        readonly Faction visitors;
        readonly GameComponent_RHAH_Game choices;
        int nextPawnId = 1;

        public RHAH_VisitorCustodyTests()
        {
            RimWorldAssemblies.EnsureResolved();
            previousGame = Current.Game;
            previousProgramState = Current.ProgramState;
            previousHost = (IRHAH_ApiHost)Field(typeof(RHAH_Api), "host").GetValue(null);
            previousBehaviors = new List<IRHAH_PawnBehavior>(
                (List<IRHAH_PawnBehavior>)Field(typeof(RHAH_PawnBehaviors), "handlers").GetValue(null));
            RHAH_PawnBehaviors.ResetForTests();
            RHAH_Api.Bind(new RHAH_ApiHost());

            Current.Game = Uninitialized<Game>();
            Current.Game.components = new List<GameComponent>();
            Current.Game.uniqueIDsManager = new UniqueIDsManager();
            Current.Game.tickManager = new TickManager();
            Current.ProgramState = ProgramState.Entry;
            Field(typeof(Game), "maps").SetValue(Current.Game, new List<Map>());
            World world = Uninitialized<World>();
            world.factionManager = new FactionManager();
            world.worldPawns = new WorldPawns();
            world.worldObjects = new WorldObjectsHolder();
            Current.Game.World = world;
            player = NewFaction(true);
            visitors = NewFaction(false);
            Field(typeof(FactionManager), "ofPlayer").SetValue(world.factionManager, player);
            map = NewMap(41);
            choices = new GameComponent_RHAH_Game(Current.Game);
            Current.Game.components.Add(choices);
        }

        public void Dispose()
        {
            Current.Game = previousGame;
            Current.ProgramState = previousProgramState;
            RHAH_Api.Bind(previousHost);
            RHAH_PawnBehaviors.ResetForTests();
            for (int i = 0; i < previousBehaviors.Count; i++)
            {
                RHAH_PawnBehaviors.Register(previousBehaviors[i]);
            }
        }

        [Theory]
        [InlineData("released")]
        [InlineData("player")]
        [InlineData("otherMap")]
        [InlineData("imprisonDenied")]
        public void CaptureEligibilityRejectsStaleOrForbiddenVisitors(string change)
        {
            Verse.Pawn pawn = Visitor(map);
            Assert.True(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));

            if (change == "released")
            {
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Released);
            }
            else if (change == "player")
            {
                pawn.SetFactionDirect(player);
            }
            else if (change == "otherMap")
            {
                Field(typeof(Thing), "mapIndexOrState").SetValue(pawn, (sbyte)Find.Maps.IndexOf(NewMap(42)));
            }
            else
            {
                RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Imprison, false);
            }

            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, true));
            Assert.False(RHAH_VisitorBatch.HasEligible(new List<Verse.Pawn> { pawn }, map.uniqueID, false));
        }

        [Fact]
        public void HireAndJoinDenialDoesNotDenyImprisonment()
        {
            Verse.Pawn pawn = Visitor(map);
            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Hire, false);
            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.JoinColony, false);

            Assert.True(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            Assert.True(RHAH_VisitorBatch.HasEligible(new List<Verse.Pawn> { null, pawn }, map.uniqueID, false));

            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Imprison, false);
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
        }

        [Fact]
        public void QuarantineAndMentalStateOverrideOtherwiseValidCustodyEligibility()
        {
            Verse.Pawn pawn = Visitor(map);
            Assert.True(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            MapComponent_RHAH_Map component = new MapComponent_RHAH_Map(map);
            map.components.Add(component);
            component.PlagueQuarantineLoadIds.Add(pawn.thingIDNumber);
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            component.PlagueQuarantineLoadIds.Clear();

            Field(typeof(MentalStateHandler), "curStateInt").SetValue(
                pawn.mindState.mentalStateHandler, Uninitialized<MentalState_Berserk>());
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
        }

        [Fact]
        public void DownedVisitorRemainsEligibleButExistingCustodyNeverDoes()
        {
            Verse.Pawn pawn = Visitor(map);
            Field(typeof(Pawn_HealthTracker), "healthState").SetValue(pawn.health, PawnHealthState.Down);
            Assert.True(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));

            pawn.guest.guestStatusInt = GuestStatus.Prisoner;
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            pawn.guest.guestStatusInt = GuestStatus.Slave;
            Assert.False(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
        }

        [Theory]
        [InlineData(true, false, true, true)]
        [InlineData(false, false, true, false)]
        [InlineData(true, true, true, false)]
        [InlineData(true, false, false, false)]
        public void OnlyEnclosedPrisonRoomsWithRegionsAreSecure(
            bool prison, bool edge, bool hasRegion, bool expected)
        {
            Room room = NewRoom(prison, edge, hasRegion ? 1 : 0);
            Assert.Equal(expected, RHAH_VisitorBatch.IsSecurePrison(room));
            Assert.False(RHAH_VisitorBatch.IsSecurePrison(null));
        }

        [Fact]
        public void FreePassageDoorOnAnyRoomRegionRejectsEvenAnEnclosedCorridorExit()
        {
            Room prison = NewRoom(true, false, 2);
            Room corridor = NewRoom(false, false, 1);
            Building_Door door = Uninitialized<Building_Door>();
            Region doorway = NewRegion(3);
            doorway.type = RegionType.Portal;
            doorway.door = door;
            Link(prison.Regions[1], doorway);
            Link(doorway, corridor.Regions[0]);
            Assert.False(corridor.TouchesMapEdge);
            Assert.False(door.FreePassage);
            Assert.True(RHAH_VisitorBatch.IsSecurePrison(prison));

            Field(typeof(Building_Door), "openInt").SetValue(door, true);
            Field(typeof(Building_Door), "holdOpenInt").SetValue(door, true);
            Assert.True(door.FreePassage);
            Assert.False(RHAH_VisitorBatch.IsSecurePrison(prison));
        }

        [Fact]
        public void CaptureGatePropagatesReleaseVetoWithoutChangingVisitorState()
        {
            Verse.Pawn pawn = Visitor(map);
            ReleasePolicy policy = new ReleasePolicy();
            RHAH_PawnBehaviors.Register(policy);
            Lord lord = Uninitialized<Lord>();
            PawnDuty duty = Uninitialized<PawnDuty>();
            Job job = Uninitialized<Job>();
            pawn.lord = lord;
            pawn.mindState.duty = duty;
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.jobs.curJob = job;
            IRHAH_Pawn before = RHAH_Api.Get(pawn);
            Pawn_GuestTracker guest = pawn.guest;

            Assert.False(RHAH_ColonyGates.AllowsCapture(pawn, player));

            Assert.Equal(1, policy.ReleaseCalls);
            Assert.Equal(RHAH_ReleaseReason.Imprisoned, policy.LastReason);
            Assert.True(RHAH_Api.IsVisitor(pawn));
            Assert.Equal(before.Lifecycle, RHAH_Api.Get(pawn).Lifecycle);
            Assert.Equal(before.SpawnBatchId, RHAH_Api.Get(pawn).SpawnBatchId);
            Assert.Same(visitors, pawn.Faction);
            Assert.Same(guest, pawn.guest);
            Assert.Equal(GuestStatus.Guest, guest.GuestStatus);
            Assert.False(pawn.IsPrisoner);
            Assert.False(pawn.IsSlave);
            Assert.Same(lord, pawn.GetLord());
            Assert.Same(duty, pawn.mindState.duty);
            Assert.Same(job, pawn.CurJob);
        }

        [Theory]
        [InlineData(RHAH_ChoiceAction.Capture)]
        [InlineData(RHAH_ChoiceAction.Enslave)]
        public void EmptyCustodyBatchKeepsChoiceOpen(RHAH_ChoiceAction action)
        {
            RHAH_ChoiceRecord record = Open();

            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 9, true, false));

            Assert.True(record.Open);
            Assert.Equal(RHAH_ChoiceAction.None, record.Settled);
        }

        [Fact]
        public void CaptureOutsidePrisonDoesNotMoveVisitorOrCloseChoice()
        {
            Verse.Pawn pawn = Visitor(map);
            Room room = pawn.GetRoom();
            Assert.True(RHAH_VisitorBatch.CanConvert(pawn, map.uniqueID, false));
            Field(typeof(Room), "isPrisonCell").SetValue(room, false);
            IntVec3 position = pawn.Position;
            RHAH_ChoiceRecord record = Open(pawn);

            Assert.Equal(0, RHAH_VisitorBatch.Capture(new List<Verse.Pawn> { pawn }, map.uniqueID));
            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, RHAH_ChoiceAction.Capture, 9, true, false));

            Assert.True(record.Open);
            Assert.True(pawn.Spawned);
            Assert.Same(map, pawn.Map);
            Assert.Equal(position, pawn.Position);
            Assert.True(RHAH_Api.IsVisitor(pawn));
            Assert.Equal(GuestStatus.Guest, pawn.guest.GuestStatus);
        }

        [Theory]
        [InlineData(RHAH_ChoiceAction.Capture)]
        [InlineData(RHAH_ChoiceAction.Enslave)]
        public void EntirelyIneligibleCustodyBatchKeepsChoiceAndPawnsUnchanged(RHAH_ChoiceAction action)
        {
            Verse.Pawn blocked = Visitor(map);
            RHAH_Api.SetGate(blocked, RHAH_BehaviorGate.Imprison, false);
            Verse.Pawn released = Visitor(map);
            RHAH_Api.SetLifecycle(released, RHAH_Lifecycle.Released);
            Verse.Pawn otherMap = Visitor(NewMap(42));
            Verse.Pawn colonist = Visitor(map);
            colonist.SetFactionDirect(player);
            RHAH_ChoiceRecord record = Open(blocked, released, otherMap, colonist);

            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 9, true, false));

            Assert.True(record.Open);
            Assert.Equal(RHAH_ChoiceAction.None, record.Settled);
            Assert.True(RHAH_Api.IsVisitor(blocked));
            Assert.True(RHAH_Api.IsVisitor(otherMap));
            Assert.True(RHAH_Api.Get(released).IsReleased);
            Assert.Same(player, colonist.Faction);
            Assert.Equal(GuestStatus.Guest, blocked.guest.GuestStatus);
            Assert.Equal(GuestStatus.Guest, otherMap.guest.GuestStatus);
            Assert.Same(Find.Maps[1], otherMap.Map);
        }

        [Theory]
        [InlineData(RHAH_ChoiceAction.Capture)]
        [InlineData(RHAH_ChoiceAction.Enslave)]
        public void DisabledChoiceDoesNotConsultOrConvertItsEligibleVisitor(RHAH_ChoiceAction action)
        {
            Verse.Pawn pawn = Visitor(map);
            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Imprison, null);
            ReleasePolicy policy = new ReleasePolicy();
            RHAH_PawnBehaviors.Register(policy);
            RHAH_ChoiceRecord record = Open(pawn);

            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 9, false, false));

            Assert.True(record.Open);
            Assert.Equal(0, policy.GateCalls);
            Assert.Equal(0, policy.ReleaseCalls);
            Assert.True(RHAH_Api.IsVisitor(pawn));
            Assert.Equal(GuestStatus.Guest, pawn.guest.GuestStatus);
        }

        [Theory]
        [InlineData(RHAH_ChoiceAction.Capture)]
        [InlineData(RHAH_ChoiceAction.Enslave)]
        public void ExpiryWinsOverCustodyAndRepeatedClicksNeverRetryConversion(RHAH_ChoiceAction action)
        {
            Verse.Pawn pawn = Visitor(map);
            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Imprison, null);
            ReleasePolicy policy = new ReleasePolicy();
            RHAH_PawnBehaviors.Register(policy);
            RHAH_ChoiceRecord record = Open(pawn);

            Assert.Equal(RHAH_ChoiceAction.Timeout,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 10, true, false));
            Assert.False(record.Open);
            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 11, true, false));

            Assert.Equal(RHAH_ChoiceAction.Timeout, record.Settled);
            Assert.Equal(0, policy.GateCalls);
            Assert.Equal(0, policy.ReleaseCalls);
            Assert.True(RHAH_Api.IsVisitor(pawn));
            Assert.Equal(GuestStatus.Guest, pawn.guest.GuestStatus);
        }

        [Theory]
        [InlineData(RHAH_ChoiceAction.Capture)]
        [InlineData(RHAH_ChoiceAction.Enslave)]
        public void AlreadySettledCustodyDoesNotConvertNewlyEligibleVisitors(RHAH_ChoiceAction action)
        {
            Verse.Pawn pawn = Visitor(map);
            RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Imprison, null);
            ReleasePolicy policy = new ReleasePolicy();
            RHAH_PawnBehaviors.Register(policy);
            RHAH_ChoiceRecord record = Open(pawn);
            record.Settled = action;

            Assert.Equal(RHAH_ChoiceAction.None,
                RHAH_ChoiceRuntime.TrySettle(choices, record.Id, action, 9, true, false));

            Assert.Equal(action, record.Settled);
            Assert.Equal(0, policy.GateCalls);
            Assert.Equal(0, policy.ReleaseCalls);
            Assert.True(RHAH_Api.IsVisitor(pawn));
            Assert.Equal(GuestStatus.Guest, pawn.guest.GuestStatus);
        }

        RHAH_ChoiceRecord Open(params Verse.Pawn[] pawns)
        {
            RHAH_ChoiceRecord record = new RHAH_ChoiceRecord
            {
                DisplayId = "I-004",
                Choice = RHAH_ChoiceKind.Visitors,
                BatchId = 7,
                MapId = map.uniqueID,
                ExpireTick = 10
            };
            for (int i = 0; i < pawns.Length; i++)
            {
                record.PawnLoadIds.Add(pawns[i].thingIDNumber);
            }
            return RHAH_ChoiceRuntime.Open(choices, record);
        }

        Verse.Pawn Visitor(Map target)
        {
            Verse.Pawn pawn = Uninitialized<Verse.Pawn>();
            pawn.thingIDNumber = nextPawnId++;
            pawn.def = Uninitialized<ThingDef>();
            pawn.def.defName = "CustodyVisitor";
            pawn.def.category = ThingCategory.Pawn;
            pawn.SetFactionDirect(visitors);
            Field(typeof(Thing), "mapIndexOrState").SetValue(pawn, (sbyte)Find.Maps.IndexOf(target));
            Field(typeof(Thing), "positionInt").SetValue(pawn, new IntVec3(1, 0, 1));
            pawn.health = Uninitialized<Pawn_HealthTracker>();
            Field(typeof(Pawn_HealthTracker), "healthState").SetValue(pawn.health, PawnHealthState.Mobile);
            pawn.health.hediffSet = new HediffSet(pawn);
            pawn.mindState = Uninitialized<Pawn_MindState>();
            pawn.mindState.mentalStateHandler = new MentalStateHandler(pawn);
            pawn.guest = Uninitialized<Pawn_GuestTracker>();
            Field(typeof(Pawn_GuestTracker), "pawn").SetValue(pawn.guest, pawn);
            Hediff_RHAH_Mark mark = new Hediff_RHAH_Mark { pawn = pawn };
            CompRHAH_Pawn origin = new CompRHAH_Pawn { parent = mark };
            mark.comps = new List<HediffComp> { origin };
            pawn.health.hediffSet.hediffs.Add(mark);
            RHAH_Api.TryMarkOrigin(pawn, new RHAH_PawnSeed(
                sourceIncidentDisplayId: "I-004",
                spawnBatchId: 7,
                role: RHAH_PawnRole.Refugee,
                gateOverrides: new[]
                {
                    new KeyValuePair<RHAH_BehaviorGate, bool>(RHAH_BehaviorGate.Imprison, true)
                }));
            // 记录查找走原版世界角色集合 不触发地图容器的 Unity 主线程检查
            ((HashSet<Verse.Pawn>)Field(typeof(WorldPawns), "pawnsAlive").GetValue(Find.WorldPawns)).Add(pawn);
            return pawn;
        }

        Map NewMap(int id)
        {
            Map result = Uninitialized<Map>();
            result.uniqueID = id;
            result.info = new MapInfo { Size = new IntVec3(3, 1, 3) };
            result.components = new List<MapComponent>();
            Find.Maps.Add(result);
            result.cellIndices = new CellIndices(result);
            result.regionGrid = new RegionGrid(result);
            result.regionDirtyer = new RegionDirtyer(result);
            result.regionAndRoomUpdater = new RegionAndRoomUpdater(result) { Enabled = false };
            Field(typeof(RegionAndRoomUpdater), "initialized").SetValue(result.regionAndRoomUpdater, true);
            Room room = NewRoom(true, false, 1);
            result.regionGrid.SetRegionAt(new IntVec3(1, 0, 1), room.Regions[0]);
            return result;
        }

        static Room NewRoom(bool prison, bool edge, int regionCount)
        {
            Room room = Uninitialized<Room>();
            District district = Uninitialized<District>();
            List<Region> regions = new List<Region>();
            for (int i = 0; i < regionCount; i++)
            {
                Region region = NewRegion(i + 1);
                Field(typeof(Region), "districtInt").SetValue(region, district);
                regions.Add(region);
            }
            Field(typeof(District), "regions").SetValue(district, regions);
            Field(typeof(District), "roomInt").SetValue(district, room);
            Field(typeof(District), "numRegionsTouchingMapEdge").SetValue(district, edge ? 1 : 0);
            Field(typeof(Room), "districts").SetValue(room, new List<District> { district });
            Field(typeof(Room), "tmpRegions").SetValue(room, new List<Region>());
            Field(typeof(Room), "isPrisonCell").SetValue(room, prison);
            return room;
        }

        static Region NewRegion(int id)
        {
            Region region = Uninitialized<Region>();
            region.id = id;
            region.type = RegionType.Normal;
            region.valid = true;
            region.links = new List<RegionLink>();
            return region;
        }

        static void Link(Region first, Region second)
        {
            RegionLink link = new RegionLink { RegionA = first, RegionB = second };
            first.links.Add(link);
            second.links.Add(link);
        }

        static Faction NewFaction(bool isPlayer)
        {
            Faction faction = Uninitialized<Faction>();
            faction.def = Uninitialized<FactionDef>();
            faction.def.isPlayer = isPlayer;
            faction.def.humanlikeFaction = true;
            return faction;
        }

        static T Uninitialized<T>() where T : class
        {
            return (T)FormatterServices.GetUninitializedObject(typeof(T));
        }

        static FieldInfo Field(Type type, string name)
        {
            return type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException(type.FullName + "." + name);
        }

        sealed class ReleasePolicy : IRHAH_PawnBehavior
        {
            internal int GateCalls;
            internal int ReleaseCalls;
            internal RHAH_ReleaseReason LastReason;

            public bool? Allows(Verse.Pawn pawn, IRHAH_Pawn snapshot, RHAH_BehaviorGate gate)
            {
                GateCalls++;
                return gate == RHAH_BehaviorGate.Imprison ? (bool?)true : null;
            }

            public bool? ShouldReleaseToColony(Verse.Pawn pawn, IRHAH_Pawn snapshot, RHAH_ReleaseReason reason)
            {
                ReleaseCalls++;
                LastReason = reason;
                return false;
            }
        }
    }
}
