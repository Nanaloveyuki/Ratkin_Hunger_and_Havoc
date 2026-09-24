using System;
using HungerAndHavoc.Incidents;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    public sealed class GameComponent_RHAH_Game : GameComponent
    {
        List<string> activeGenerationBatches = new List<string>();
        List<string> pendingIncidentDisplayIds = new List<string>();
        List<float> pendingIncidentPoints = new List<float>();
        int plagueReturnLoadId;
        int plagueReturnMapId;
        int plagueReturnPhase;
        int plagueReturnDueTick = -1;
        int plagueReturnLeaveTick = -1;
        List<RHAH_ChoiceRecord> openChoices = new List<RHAH_ChoiceRecord>();
        int nextChoiceId = 1;
        int broadcastCooldownUntilTick = -1;
        int generationCursor;

        public IReadOnlyList<string> ActiveGenerationBatches => activeGenerationBatches;
        public IReadOnlyList<string> PendingIncidentDisplayIds => pendingIncidentDisplayIds;
        public int PlagueReturnLoadId { get => plagueReturnLoadId; set => plagueReturnLoadId = value; }
        public int PlagueReturnMapId { get => plagueReturnMapId; set => plagueReturnMapId = value; }
        public int PlagueReturnPhase { get => plagueReturnPhase; set => plagueReturnPhase = value; }
        public int PlagueReturnDueTick { get => plagueReturnDueTick; set => plagueReturnDueTick = value; }
        public int PlagueReturnLeaveTick { get => plagueReturnLeaveTick; set => plagueReturnLeaveTick = value; }
        public IReadOnlyList<RHAH_ChoiceRecord> OpenChoices => openChoices;
        public int BroadcastCooldownUntilTick { get => broadcastCooldownUntilTick; set => broadcastCooldownUntilTick = value; }
        internal int NextChoiceId { get => nextChoiceId; set => nextChoiceId = value; }


        public GameComponent_RHAH_Game(Game game)
        {
        }

        public override void GameComponentTick()
        {
            TickPlague();
            RHAH_ChoiceRuntime.Tick(this, Find.TickManager.TicksGame, RHAH_Mod.Settings == null || RHAH_Mod.Settings.visitorChoicesEnabled);
            TrySpawnPending(Find.TickManager.TicksGame);
            TickStays(Find.TickManager.TicksGame);
            Pawn.RHAH_AttitudeFactions.LockGoodwill();
            HungerAndHavoc.Narrative.RHAH_EndingRuntime.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Narrative.RHAH_EntrustCare.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Narrative.RHAH_JournalRuntime.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Narrative.RHAH_Envoy.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Narrative.RHAH_RecordSite.Tick(Find.TickManager.TicksGame);
        }

        public override void GameComponentUpdate()
        {
            if (!RHAH_Scheduler.ShouldDrainOnFrame(
                Find.TickManager != null && Find.TickManager.Paused,
                Find.WindowStack != null && Find.WindowStack.WindowsForcePause,
                pendingIncidentDisplayIds.Count))
            {
                return;
            }

            TrySpawnPending(0);
        }


        internal bool TrySpawnPending(int tick)
        {
            if (RHAH_Mod.Settings != null && RHAH_Mod.Settings.staggerGeneration && (tick & 63) != 0)
            {
                return false;
            }

            if (pendingIncidentDisplayIds.Count == 0 || Current.Game == null)
            {
                return false;
            }

            string displayId = pendingIncidentDisplayIds[0];
            float points = pendingIncidentPoints.Count > 0
                ? pendingIncidentPoints[0]
                : PointsFor(displayId);
            HungerAndHavoc.Incidents.RHAH_IncidentEntry entry =
                HungerAndHavoc.Incidents.RHAH_IncidentCatalog.GetByDisplayId(displayId);
            if (entry == null)
            {
                Log.Warning("[RHAH] Dropped queued incident " + displayId + ". It is not in the catalog.");
                DropPending();
                return false;
            }

            Map map = entry.Target == HungerAndHavoc.Incidents.RHAH_IncidentTarget.Map ? RHAH_MapResolver.Resolve() : null;
            if (entry.Target == HungerAndHavoc.Incidents.RHAH_IncidentTarget.Map && map == null)
            {
                Log.Warning("[RHAH] Queued " + displayId + " is still waiting. No usable map. points=" + points.ToString("0.##"));
                return false;
            }

            IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(entry.DefName);
            if (def == null || def.Worker == null)
            {
                Log.Warning("[RHAH] Dropped queued incident " + displayId + ". Def " + entry.DefName + " or its worker is missing.");
                DropPending();
                return false;
            }

            IncidentParms parms = new IncidentParms { target = map };
            parms.points = points;
            bool executed;
            try
            {
                executed = def.Worker.TryExecute(parms);
            }
            catch (Exception exception)
            {
                Log.Error("[RHAH] Queued " + displayId + " threw while spawning. def=" + entry.DefName +
                    " points=" + points.ToString("0.##") + " target=" + TargetText(entry, map) + "\n" + exception);
                DropPending();
                return false;
            }

            if (!executed)
            {
                Log.Warning("[RHAH] Dropped queued " + displayId + ". Worker did not spawn. def=" + entry.DefName +
                    " points=" + points.ToString("0.##") + " target=" + TargetText(entry, map));
                DropPending();
                return false;
            }

            Log.Message("[RHAH] Spawned queued " + displayId + ". def=" + entry.DefName +
                " points=" + points.ToString("0.##") + " target=" + TargetText(entry, map));
            DropPending();
            return true;
        }

        static string TargetText(HungerAndHavoc.Incidents.RHAH_IncidentEntry entry, Map map)
        {
            if (entry.Target == HungerAndHavoc.Incidents.RHAH_IncidentTarget.Caravan)
            {
                return "caravan";
            }

            return map == null ? "map:none" : "map:" + map.uniqueID;
        }

        public void RegisterBatch(string batchKey)
        {
            if (!string.IsNullOrEmpty(batchKey) && !activeGenerationBatches.Contains(batchKey))
            {
                activeGenerationBatches.Add(batchKey);
            }
        }

        public bool QueueIncident(string displayId)
        {
            return QueueIncident(displayId, PointsFor(displayId));
        }

        public bool QueueIncident(string displayId, float points)
        {
            if (string.IsNullOrEmpty(displayId) || pendingIncidentDisplayIds.Contains(displayId))
            {
                return false;
            }

            pendingIncidentDisplayIds.Add(displayId);
            pendingIncidentPoints.Add(RHAH_IncidentTuning.ClampPoints(points));
            return true;
        }

        static float PointsFor(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            float catalog = entry == null ? RHAH_IncidentTuning.MinDebugPoints : entry.DebugPoints;
            return RHAH_Mod.Settings == null
                ? catalog
                : RHAH_Mod.Settings.IncidentDebugPoints(displayId, catalog);
        }

        void DropPending()
        {
            if (pendingIncidentDisplayIds.Count > 0)
            {
                pendingIncidentDisplayIds.RemoveAt(0);
            }

            if (pendingIncidentPoints.Count > 0)
            {
                pendingIncidentPoints.RemoveAt(0);
            }
        }
        void TickPlague()
        {
            if (Find.TickManager == null || Find.Maps == null)
            {
                return;
            }

            if (Find.TickManager.TicksGame % Identity.RHAH_Plague.CheckIntervalTicks != 0)
            {
                return;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                Map map = Find.Maps[i];
                map?.GetComponent<MapComponent_RHAH_Map>()?.TickPlague();
            }
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref activeGenerationBatches, "activeGenerationBatches", LookMode.Value);
            Scribe_Collections.Look(ref pendingIncidentDisplayIds, "pendingIncidentDisplayIds", LookMode.Value);
            Scribe_Collections.Look(ref pendingIncidentPoints, "pendingIncidentPoints", LookMode.Value);
            Scribe_Values.Look(ref plagueReturnLoadId, "plagueReturnLoadId", 0);
            Scribe_Values.Look(ref plagueReturnMapId, "plagueReturnMapId", 0);
            Scribe_Values.Look(ref plagueReturnPhase, "plagueReturnPhase", 0);
            Scribe_Values.Look(ref plagueReturnDueTick, "plagueReturnDueTick", -1);
            Scribe_Values.Look(ref plagueReturnLeaveTick, "plagueReturnLeaveTick", -1);
            Scribe_Collections.Look(ref openChoices, "openChoices", LookMode.Deep);
            Scribe_Values.Look(ref nextChoiceId, "nextChoiceId", 1);
            Scribe_Values.Look(ref broadcastCooldownUntilTick, "broadcastCooldownUntilTick", -1);
            Scribe_Values.Look(ref generationCursor, "generationCursor", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                activeGenerationBatches = activeGenerationBatches ?? new List<string>();
                pendingIncidentDisplayIds = pendingIncidentDisplayIds ?? new List<string>();
                pendingIncidentPoints = pendingIncidentPoints ?? new List<float>();
                while (pendingIncidentPoints.Count < pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentPoints.Add(PointsFor(pendingIncidentDisplayIds[pendingIncidentPoints.Count]));
                }

                while (pendingIncidentPoints.Count > pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentPoints.RemoveAt(pendingIncidentPoints.Count - 1);
                }
                openChoices = openChoices ?? new List<RHAH_ChoiceRecord>();
                for (int i = openChoices.Count - 1; i >= 0; i--)
                {
                    if (openChoices[i] == null)
                    {
                        openChoices.RemoveAt(i);
                        continue;
                    }

                    openChoices[i].PawnLoadIds = openChoices[i].PawnLoadIds ?? new List<int>();
                }
            }
        }
        static void TickStays(int tick)
        {
            if ((tick & 250) != 0 || Find.Maps == null)
            {
                return;
            }

            for (int mapIndex = 0; mapIndex < Find.Maps.Count; mapIndex++)
            {
                Map map = Find.Maps[mapIndex];
                if (map?.mapPawns?.AllPawnsSpawned == null)
                {
                    continue;
                }

                System.Collections.Generic.IReadOnlyList<Verse.Pawn> pawns = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    HungerAndHavoc.Pawn.RHAH_VisitorStay.Tick(pawns[i], tick);
                }
            }
        }
    }

    public sealed class MapComponent_RHAH_Map : MapComponent
    {
        List<int> visitorPawnLoadIds = new List<int>();
        Dictionary<int, int> foodSearchTicks = new Dictionary<int, int>();
        Dictionary<int, int> wallGnawCounts = new Dictionary<int, int>();
        List<int> plagueQuarantineLoadIds = new List<int>();
        int plagueRecovered;
        int plagueDied;
        int plagueLastSpreadDay = -1;
        List<Verse.Pawn> predationPrey = new List<Verse.Pawn>();
        List<HungerAndHavoc.Incidents.RHAH_PredatorRecord> predators = new List<HungerAndHavoc.Incidents.RHAH_PredatorRecord>();
        bool predationRolled;
        bool predationSelected;
        int predationPendingTick = -1;
        int predationNextTick = -1;
        int holeThingId;

        public IReadOnlyList<int> VisitorPawnLoadIds => visitorPawnLoadIds;
        public List<int> PlagueQuarantineLoadIds => plagueQuarantineLoadIds;
        public int PlagueRecovered { get => plagueRecovered; set => plagueRecovered = value; }
        public int PlagueDied { get => plagueDied; set => plagueDied = value; }
        public int PlagueLastSpreadDay { get => plagueLastSpreadDay; set => plagueLastSpreadDay = value; }
        public bool PredationRolled { get => predationRolled; set => predationRolled = value; }
        public bool PredationSelected { get => predationSelected; set => predationSelected = value; }
        public int PredationPendingTick { get => predationPendingTick; set => predationPendingTick = value; }
        public int PredationNextTick { get => predationNextTick; set => predationNextTick = value; }
        public int HoleThingId { get => holeThingId; set => holeThingId = value; }
        public List<Verse.Pawn> PredationPrey => predationPrey;
        public List<HungerAndHavoc.Incidents.RHAH_PredatorRecord> Predators => predators;
        public MapComponent_RHAH_Map(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            Pawn.RHAH_ReliefArea.Ensure(map);
        }

        public void RegisterVisitor(int pawnLoadId)
        {
            if (pawnLoadId > 0 && !visitorPawnLoadIds.Contains(pawnLoadId))
            {
                visitorPawnLoadIds.Add(pawnLoadId);
            }
        }

        public void RemoveVisitor(int pawnLoadId)
        {
            visitorPawnLoadIds.Remove(pawnLoadId);
            foodSearchTicks.Remove(pawnLoadId);
            wallGnawCounts.Remove(pawnLoadId);
        }

        public bool FoodSearchReady(int pawnLoadId, int tick)
        {
            int next;
            if (!foodSearchTicks.TryGetValue(pawnLoadId, out next))
            {
                return true;
            }

            return tick >= next;
        }

        public void SetFoodSearchTick(int pawnLoadId, int tick)
        {
            if (pawnLoadId <= 0)
            {
                return;
            }

            foodSearchTicks[pawnLoadId] = tick;
        }

        public void InvalidateFoodSearch()
        {
            foodSearchTicks.Clear();
        }

        public int NextWallGnaw(int pawnLoadId)
        {
            return HungerAndHavoc.Pawn.RHAH_GnawHealth.NextWallCount(wallGnawCounts, pawnLoadId);
        }

        public void ForgetWallGnaw(int pawnLoadId)
        {
            HungerAndHavoc.Pawn.RHAH_GnawHealth.Forget(wallGnawCounts, pawnLoadId);
        }
        public bool IsQuarantined(int pawnLoadId)
        {
            return Identity.RHAH_Plague.IsQuarantined(plagueQuarantineLoadIds, pawnLoadId);
        }

        public void Quarantine(int pawnLoadId)
        {
            if (pawnLoadId > 0 && !IsQuarantined(pawnLoadId))
            {
                plagueQuarantineLoadIds.Add(pawnLoadId);
            }
        }

        public void TickPlague()
        {
            Identity.RHAH_PlagueRuntime.TickMap(this, map);
        }
        public void RememberPredationPrey(Verse.Pawn pawn)
        {
            if (pawn != null && !predationPrey.Contains(pawn))
            {
                predationPrey.Add(pawn);
            }
        }

        public bool IsPredationPrey(Verse.Pawn pawn)
        {
            return pawn != null && predationPrey.Contains(pawn);
        }

        public void TrackPredator(Verse.Pawn pawn, bool outside, int tick)
        {
            if (pawn == null)
            {
                return;
            }

            for (int i = 0; i < predators.Count; i++)
            {
                if (predators[i]?.pawn == pawn)
                {
                    predators[i].outside = outside;
                    predators[i].nextSearchTick = tick;
                    return;
                }
            }

            predators.Add(new HungerAndHavoc.Incidents.RHAH_PredatorRecord
            {
                pawn = pawn,
                outside = outside,
                nextSearchTick = tick
            });
        }

        public override void MapComponentTick()
        {
            HungerAndHavoc.Incidents.RHAH_CampPredation.Tick(this, map);
            HungerAndHavoc.Incidents.RHAH_GrainHole.Tick(map);
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref visitorPawnLoadIds, "visitorPawnLoadIds", LookMode.Value);
            Scribe_Collections.Look(ref foodSearchTicks, "foodSearchTicks", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref plagueQuarantineLoadIds, "plagueQuarantineLoadIds", LookMode.Value);
            Scribe_Collections.Look(ref wallGnawCounts, "wallGnawCounts", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref plagueRecovered, "plagueRecovered", 0);
            Scribe_Values.Look(ref plagueDied, "plagueDied", 0);
            Scribe_Values.Look(ref plagueLastSpreadDay, "plagueLastSpreadDay", -1);
            Scribe_Collections.Look(ref predationPrey, "predationPrey", LookMode.Reference);
            Scribe_Collections.Look(ref predators, "predators", LookMode.Deep);
            Scribe_Values.Look(ref predationRolled, "predationRolled", false);
            Scribe_Values.Look(ref predationSelected, "predationSelected", false);
            Scribe_Values.Look(ref predationPendingTick, "predationPendingTick", -1);
            Scribe_Values.Look(ref predationNextTick, "predationNextTick", -1);
            Scribe_Values.Look(ref holeThingId, "holeThingId", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                visitorPawnLoadIds = visitorPawnLoadIds ?? new List<int>();
                foodSearchTicks = foodSearchTicks ?? new Dictionary<int, int>();
                plagueQuarantineLoadIds = plagueQuarantineLoadIds ?? new List<int>();
                wallGnawCounts = wallGnawCounts ?? new Dictionary<int, int>();
                predationPrey = predationPrey ?? new List<Verse.Pawn>();
                predators = predators ?? new List<HungerAndHavoc.Incidents.RHAH_PredatorRecord>();
            }
        }
    }
}