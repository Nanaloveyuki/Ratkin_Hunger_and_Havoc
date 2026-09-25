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
        List<int> pendingIncidentTargetIds = new List<int>();
        List<int> begCooldownPawnIds = new List<int>();
        List<int> begCooldownTicks = new List<int>();
        List<int> beggedPawnIds = new List<int>();
        List<int> beggedColonistIds = new List<int>();
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
        public IReadOnlyList<int> PendingIncidentTargetIds => pendingIncidentTargetIds;
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
            HungerAndHavoc.Storyteller.Suiyin.RHAH_EndingRuntime.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Storyteller.Suiyin.RHAH_EntrustCare.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Storyteller.Suiyin.RHAH_Quarantine.Tick(Current.Game?.GetComponent<HungerAndHavoc.Narrative.NarrativeState>(), Find.TickManager.TicksGame);
            HungerAndHavoc.Storyteller.Suiyin.RHAH_JournalRuntime.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Storyteller.Suiyin.RHAH_Envoy.Tick(Find.TickManager.TicksGame);
            HungerAndHavoc.Storyteller.Suiyin.RHAH_RecordSite.Tick(Find.TickManager.TicksGame);
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

            int index = NextPendingIndex();
            if (index < 0)
            {
                return false;
            }

            string displayId = pendingIncidentDisplayIds[index];
            float points = index < pendingIncidentPoints.Count
                ? pendingIncidentPoints[index]
                : PointsFor(displayId);
            int targetId = index < pendingIncidentTargetIds.Count
                ? pendingIncidentTargetIds[index]
                : RHAH_IncidentSchedule.UnspecifiedTargetId;
            HungerAndHavoc.Incidents.RHAH_IncidentEntry entry =
                HungerAndHavoc.Incidents.RHAH_IncidentCatalog.GetByDisplayId(displayId);
            if (entry == null)
            {
                Log.Warning("[RHAH] Dropped queued incident " + displayId + ". It is not in the catalog.");
                DropPending(index);
                return false;
            }

            RHAH_QueuedTarget queued = RHAH_QueuedTarget.Resolve(entry.Target, targetId);
            if (queued.Terminal)
            {
                Log.Warning("[RHAH] Dropped queued incident " + displayId + ". " + queued.Reason +
                    " points=" + points.ToString("0.##") + " target=" + targetId);
                DropPending(index);
                return false;
            }

            IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(entry.DefName);
            if (def == null || def.Worker == null)
            {
                Log.Warning("[RHAH] Dropped queued incident " + displayId + ". Def " + entry.DefName + " or its worker is missing.");
                DropPending(index);
                return false;
            }

            IncidentParms parms = new IncidentParms { target = queued.Target };
            parms.points = points;
            bool executed;
            try
            {
                executed = def.Worker.TryExecute(parms);
            }
            catch (Exception exception)
            {
                Log.Error("[RHAH] Queued " + displayId + " threw while spawning. def=" + entry.DefName +
                    " points=" + points.ToString("0.##") + " target=" + TargetText(entry, queued.Target) + "\n" + exception);
                DropPending(index);
                return false;
            }

            if (!executed)
            {
                Log.Warning("[RHAH] Dropped queued " + displayId + ". Worker did not spawn. def=" + entry.DefName +
                    " points=" + points.ToString("0.##") + " target=" + TargetText(entry, queued.Target));
                DropPending(index);
                return false;
            }

            Log.Message("[RHAH] Spawned queued " + displayId + ". def=" + entry.DefName +
                " points=" + points.ToString("0.##") + " target=" + TargetText(entry, queued.Target));
            DropPending(index);
            return true;
        }

        int NextPendingIndex()
        {
            int count = pendingIncidentDisplayIds.Count;
            bool[] terminal = new bool[count];
            bool[] ready = new bool[count];
            for (int i = 0; i < count; i++)
            {
                HungerAndHavoc.Incidents.RHAH_IncidentEntry entry =
                    HungerAndHavoc.Incidents.RHAH_IncidentCatalog.GetByDisplayId(pendingIncidentDisplayIds[i]);
                if (entry == null)
                {
                    terminal[i] = true;
                    continue;
                }

                int targetId = i < pendingIncidentTargetIds.Count
                    ? pendingIncidentTargetIds[i]
                    : RHAH_IncidentSchedule.UnspecifiedTargetId;
                RHAH_QueuedTarget queued = RHAH_QueuedTarget.Resolve(entry.Target, targetId);
                if (queued.Terminal)
                {
                    terminal[i] = true;
                    continue;
                }

                if (queued.Unavailable)
                {
                    continue;
                }

                IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(entry.DefName);
                ready[i] = def != null && def.Worker != null;
                terminal[i] = !ready[i];
            }

            return RHAH_IncidentSchedule.NextExecutable(terminal, ready);
        }

        static string TargetText(HungerAndHavoc.Incidents.RHAH_IncidentEntry entry, IIncidentTarget target)
        {
            if (entry.Target == HungerAndHavoc.Incidents.RHAH_IncidentTarget.Caravan)
            {
                RimWorld.Planet.Caravan caravan = target as RimWorld.Planet.Caravan;
                return caravan == null ? "caravan:none" : "caravan:" + caravan.ID;
            }

            Map map = target as Map;
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
            return QueueIncident(displayId, PointsFor(displayId), RHAH_IncidentSchedule.UnspecifiedTargetId);
        }

        public bool QueueIncident(string displayId, float points)
        {
            return QueueIncident(displayId, points, RHAH_IncidentSchedule.UnspecifiedTargetId);
        }

        public bool QueueIncident(string displayId, float points, int targetId)
        {
            if (string.IsNullOrEmpty(displayId) || pendingIncidentDisplayIds.Contains(displayId))
            {
                return false;
            }

            pendingIncidentDisplayIds.Add(displayId);
            pendingIncidentPoints.Add(RHAH_IncidentTuning.ClampPoints(points));
            pendingIncidentTargetIds.Add(targetId);
            return true;
        }

        internal static float PointsFor(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            float catalog = entry == null ? RHAH_IncidentTuning.MinDebugPoints : entry.DebugPoints;
            return RHAH_Mod.Settings == null
                ? catalog
                : RHAH_Mod.Settings.IncidentDebugPoints(displayId, catalog);
        }

        void DropPending(int index)
        {
            if (index < 0)
            {
                return;
            }

            if (index < pendingIncidentDisplayIds.Count)
            {
                pendingIncidentDisplayIds.RemoveAt(index);
            }

            if (index < pendingIncidentPoints.Count)
            {
                pendingIncidentPoints.RemoveAt(index);
            }

            if (index < pendingIncidentTargetIds.Count)
            {
                pendingIncidentTargetIds.RemoveAt(index);
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
            Scribe_Collections.Look(ref pendingIncidentTargetIds, "pendingIncidentTargetIds", LookMode.Value);
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
            Scribe_Collections.Look(ref begCooldownPawnIds, "begCooldownPawnIds", LookMode.Value);
            Scribe_Collections.Look(ref begCooldownTicks, "begCooldownTicks", LookMode.Value);
            Scribe_Collections.Look(ref beggedPawnIds, "beggedPawnIds", LookMode.Value);
            Scribe_Collections.Look(ref beggedColonistIds, "beggedColonistIds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                activeGenerationBatches = activeGenerationBatches ?? new List<string>();
                pendingIncidentDisplayIds = pendingIncidentDisplayIds ?? new List<string>();
                pendingIncidentPoints = pendingIncidentPoints ?? new List<float>();
                pendingIncidentTargetIds = pendingIncidentTargetIds ?? new List<int>();
                while (pendingIncidentPoints.Count < pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentPoints.Add(PointsFor(pendingIncidentDisplayIds[pendingIncidentPoints.Count]));
                }

                while (pendingIncidentTargetIds.Count < pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentTargetIds.Add(RHAH_IncidentSchedule.UnspecifiedTargetId);
                }

                while (pendingIncidentPoints.Count > pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentPoints.RemoveAt(pendingIncidentPoints.Count - 1);
                }

                while (pendingIncidentTargetIds.Count > pendingIncidentDisplayIds.Count)
                {
                    pendingIncidentTargetIds.RemoveAt(pendingIncidentTargetIds.Count - 1);
                }
                openChoices = openChoices ?? new List<RHAH_ChoiceRecord>();
                begCooldownPawnIds = begCooldownPawnIds ?? new List<int>();
                begCooldownTicks = begCooldownTicks ?? new List<int>();
                beggedPawnIds = beggedPawnIds ?? new List<int>();
                beggedColonistIds = beggedColonistIds ?? new List<int>();
                Align(begCooldownPawnIds, begCooldownTicks, -1);
                Align(beggedPawnIds, beggedColonistIds, 0);
                Pawn.RHAH_Begging.Load(begCooldownPawnIds, begCooldownTicks, beggedPawnIds, beggedColonistIds);
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

        internal void StoreBegging(Dictionary<int, int> cooldowns, Dictionary<int, HashSet<int>> begged)
        {
            begCooldownPawnIds.Clear();
            begCooldownTicks.Clear();
            beggedPawnIds.Clear();
            beggedColonistIds.Clear();
            if (cooldowns != null)
            {
                foreach (KeyValuePair<int, int> pair in cooldowns)
                {
                    begCooldownPawnIds.Add(pair.Key);
                    begCooldownTicks.Add(pair.Value);
                }
            }

            if (begged == null)
            {
                return;
            }

            foreach (KeyValuePair<int, HashSet<int>> pair in begged)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                foreach (int colonist in pair.Value)
                {
                    beggedPawnIds.Add(pair.Key);
                    beggedColonistIds.Add(colonist);
                }
            }
        }

        static void Align(List<int> left, List<int> right, int fill)
        {
            while (right.Count < left.Count)
            {
                right.Add(fill);
            }

            while (right.Count > left.Count)
            {
                right.RemoveAt(right.Count - 1);
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