using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal sealed class IncidentWorker_Sequel : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            HungerIncidentEntry entry = Entry;
            if (entry == null || entry.Target != HungerIncidentTarget.Map ||
                !(parms?.target is Map map) || map.mapPawns == null ||
                !Core.HungerAndHavocRuntime.AllowsNewContent)
            {
                return false;
            }

            IntVec3 cell;
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms?.target as Map;
            HungerIncidentEntry entry = Entry;
            if (entry == null || entry.Target != HungerIncidentTarget.Map || !CanFireNowSub(parms))
            {
                return false;
            }

            IntVec3 cell;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                return false;
            }

            int tick = Find.TickManager.TicksGame;
            return HungerIncidentFacts.Submit(new HungerIncidentContext
            {
                DisplayId = entry.DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = RoleFor(entry),
                Attitude = entry.DefaultAttitudePool == HungerAttitudePool.Positive ? HungerAttitude.LeaningFriendly : HungerAttitude.Hostile,
                CarriesPlague = entry.Category == HungerIncidentCategory.Plague,
                Map = map,
                SpawnCell = cell,
                PawnCount = CountFor(entry)
            });
        }

        HungerIncidentEntry Entry => def == null ? null : HungerIncidentCatalog.GetByDefName(def.defName);

        static HungerPawnRole RoleFor(HungerIncidentEntry entry)
        {
            switch (entry.Family)
            {
                case HungerIncidentFamily.Thief: return HungerPawnRole.Thief;
                case HungerIncidentFamily.Wild: return HungerPawnRole.Wild;
                case HungerIncidentFamily.Siege: return HungerPawnRole.Siege;
                case HungerIncidentFamily.Trade: return HungerPawnRole.Trader;
                case HungerIncidentFamily.Aid: return HungerPawnRole.Refugee;
                case HungerIncidentFamily.Intel: return HungerPawnRole.Envoy;
                case HungerIncidentFamily.Special: return HungerPawnRole.Refugee;
                default: return HungerPawnRole.Beggar;
            }
        }

        static int CountFor(HungerIncidentEntry entry)
        {
            if (entry.Family == HungerIncidentFamily.Siege || entry.Family == HungerIncidentFamily.Thief)
            {
                return 3;
            }

            return entry.Family == HungerIncidentFamily.Beggar ? 2 : 1;
        }
    }
}
