using HungerAndHavoc.Incidents;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    public sealed class GameComponent_HungerAndHavoc : GameComponent
    {
        List<string> activeGenerationBatches = new List<string>();
        List<string> pendingIncidentDisplayIds = new List<string>();
        List<float> pendingIncidentPoints = new List<float>();
        int plagueReturnLoadId;
        int plagueReturnMapId;
        int plagueReturnPhase;
        int plagueReturnDueTick = -1;
        int plagueReturnLeaveTick = -1;
        List<HungerChoiceRecord> openChoices = new List<HungerChoiceRecord>();
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
        public IReadOnlyList<HungerChoiceRecord> OpenChoices => openChoices;
        public int BroadcastCooldownUntilTick { get => broadcastCooldownUntilTick; set => broadcastCooldownUntilTick = value; }
        internal int NextChoiceId { get => nextChoiceId; set => nextChoiceId = value; }


        public GameComponent_HungerAndHavoc(Game game)
        {
        }

        public override void GameComponentTick()
        {
            TickPlague();
            HungerChoiceRuntime.Tick(this, Find.TickManager.TicksGame, HungerAndHavocMod.Settings == null || HungerAndHavocMod.Settings.visitorChoicesEnabled);
            if (HungerAndHavocMod.Settings != null && HungerAndHavocMod.Settings.staggerGeneration && (Find.TickManager.TicksGame & 63) != 0)
            {
                return;
            }
            if (pendingIncidentDisplayIds.Count == 0 || Current.Game == null)
            {
                return;
            }

            string displayId = pendingIncidentDisplayIds[0];
            float points = pendingIncidentPoints.Count > 0
                ? pendingIncidentPoints[0]
                : PointsFor(displayId);
            HungerAndHavoc.Incidents.HungerIncidentEntry entry =
                HungerAndHavoc.Incidents.HungerIncidentCatalog.GetByDisplayId(displayId);
            if (entry == null)
            {
                DropPending();
                return;
            }

            Map map = entry.Target == HungerAndHavoc.Incidents.HungerIncidentTarget.Map ? HungerMapResolver.Resolve() : null;
            if (entry.Target == HungerAndHavoc.Incidents.HungerIncidentTarget.Map && map == null)
            {
                return;
            }
            IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(entry.DefName);
            if (def == null)
            {
                DropPending();
                return;
            }

            IncidentParms parms = new IncidentParms { target = map };
            parms.points = points;
            if (def.Worker.TryExecute(parms))
            {
                DropPending();
            }
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
            pendingIncidentPoints.Add(HungerIncidentTuning.ClampPoints(points));
            return true;
        }

        static float PointsFor(string displayId)
        {
            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(displayId);
            float catalog = entry == null ? HungerIncidentTuning.MinDebugPoints : entry.DebugPoints;
            return HungerAndHavocMod.Settings == null
                ? catalog
                : HungerAndHavocMod.Settings.IncidentDebugPoints(displayId, catalog);
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

            if (Find.TickManager.TicksGame % Identity.HungerPlague.CheckIntervalTicks != 0)
            {
                return;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                Map map = Find.Maps[i];
                map?.GetComponent<MapComponent_HungerAndHavoc>()?.TickPlague();
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
                openChoices = openChoices ?? new List<HungerChoiceRecord>();
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
    }

    public sealed class MapComponent_HungerAndHavoc : MapComponent
    {
        List<int> visitorPawnLoadIds = new List<int>();
        Dictionary<int, int> foodSearchTicks = new Dictionary<int, int>();
        Dictionary<int, int> wallGnawCounts = new Dictionary<int, int>();
        List<int> plagueQuarantineLoadIds = new List<int>();
        int plagueRecovered;
        int plagueDied;
        int plagueLastSpreadDay = -1;

        public IReadOnlyList<int> VisitorPawnLoadIds => visitorPawnLoadIds;
        public List<int> PlagueQuarantineLoadIds => plagueQuarantineLoadIds;
        public int PlagueRecovered { get => plagueRecovered; set => plagueRecovered = value; }
        public int PlagueDied { get => plagueDied; set => plagueDied = value; }
        public int PlagueLastSpreadDay { get => plagueLastSpreadDay; set => plagueLastSpreadDay = value; }
        public MapComponent_HungerAndHavoc(Map map) : base(map)
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
            return Identity.HungerPlague.IsQuarantined(plagueQuarantineLoadIds, pawnLoadId);
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
            Identity.HungerPlagueRuntime.TickMap(this, map);
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
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                visitorPawnLoadIds = visitorPawnLoadIds ?? new List<int>();
                foodSearchTicks = foodSearchTicks ?? new Dictionary<int, int>();
                plagueQuarantineLoadIds = plagueQuarantineLoadIds ?? new List<int>();
                wallGnawCounts = wallGnawCounts ?? new Dictionary<int, int>();
            }
        }
    }
}