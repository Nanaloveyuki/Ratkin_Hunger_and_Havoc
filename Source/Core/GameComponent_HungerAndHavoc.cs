using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    public sealed class GameComponent_HungerAndHavoc : GameComponent
    {
        List<string> activeGenerationBatches = new List<string>();
        List<string> pendingIncidentDisplayIds = new List<string>();

        public IReadOnlyList<string> ActiveGenerationBatches => activeGenerationBatches;
        public IReadOnlyList<string> PendingIncidentDisplayIds => pendingIncidentDisplayIds;

        public GameComponent_HungerAndHavoc(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (pendingIncidentDisplayIds.Count == 0 || Current.Game == null)
            {
                return;
            }

            string displayId = pendingIncidentDisplayIds[0];
            HungerAndHavoc.Incidents.HungerIncidentEntry entry =
                HungerAndHavoc.Incidents.HungerIncidentCatalog.GetByDisplayId(displayId);
            if (entry == null)
            {
                pendingIncidentDisplayIds.RemoveAt(0);
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
                pendingIncidentDisplayIds.RemoveAt(0);
                return;
            }

            if (def.Worker.TryExecute(new IncidentParms { target = map }))
            {
                pendingIncidentDisplayIds.RemoveAt(0);
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
            if (string.IsNullOrEmpty(displayId) || pendingIncidentDisplayIds.Contains(displayId))
            {
                return false;
            }

            pendingIncidentDisplayIds.Add(displayId);
            return true;
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref activeGenerationBatches, "activeGenerationBatches", LookMode.Value);
            Scribe_Collections.Look(ref pendingIncidentDisplayIds, "pendingIncidentDisplayIds", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                activeGenerationBatches = activeGenerationBatches ?? new List<string>();
                pendingIncidentDisplayIds = pendingIncidentDisplayIds ?? new List<string>();
            }
        }
    }

    public sealed class MapComponent_HungerAndHavoc : MapComponent
    {
        List<int> visitorPawnLoadIds = new List<int>();
        Dictionary<int, int> foodSearchTicks = new Dictionary<int, int>();

        public IReadOnlyList<int> VisitorPawnLoadIds => visitorPawnLoadIds;

        public MapComponent_HungerAndHavoc(Map map) : base(map)
        {
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
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref visitorPawnLoadIds, "visitorPawnLoadIds", LookMode.Value);
            Scribe_Collections.Look(ref foodSearchTicks, "foodSearchTicks", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                visitorPawnLoadIds = visitorPawnLoadIds ?? new List<int>();
                foodSearchTicks = foodSearchTicks ?? new Dictionary<int, int>();
            }
        }
    }
}