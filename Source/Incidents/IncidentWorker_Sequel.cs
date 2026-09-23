using HungerAndHavoc.Api;
using HungerAndHavoc.Trade;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal sealed class IncidentWorker_Sequel : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            RHAH_IncidentEntry entry = Entry;
            if (entry == null || !Core.RHAH_Runtime.AllowsNewContent)
            {
                return false;
            }

            if (entry.DisplayId == "I-038")
            {
                return Core.RHAH_MapResolver.Resolve(parms?.target as Map) != null;
            }

            if (entry.Target == RHAH_IncidentTarget.Caravan)
            {
                return Caravan.CaravanTargetResolver.ResolvePlayerCaravan() != null;
            }

            Map map = Core.RHAH_MapResolver.Resolve(parms?.target as Map);
            return map != null && FindAnySpawnCell(map);
        }
        static bool FindAnySpawnCell(Map map)
        {
            IntVec3 cell;
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            RHAH_IncidentEntry entry = Entry;
            if (entry == null || !CanFireNowSub(parms))
            {
                return false;
            }

            if (entry.DisplayId == "I-051")
            {
                return RHAH_RefugeeCampQuest.TryOffer(parms);
            }
            if (entry.DisplayId == "I-038")
            {
                return TradeEventRouter.TrySpawnTraderCaravan(entry, parms);
            }

            if (entry.Target == RHAH_IncidentTarget.Caravan)
            {
                return TradeEventRouter.TrySpawnCaravanAmbush(entry, parms == null ? 0f : parms.points);
            }

            Map map = Core.RHAH_MapResolver.Resolve(parms?.target as Map);
            if (map == null)
            {
                return false;
            }

            IntVec3 cell;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                return false;
            }

            int tick = Find.TickManager.TicksGame;
            return RHAH_IncidentFacts.Submit(new RHAH_IncidentContext
            {
                DisplayId = entry.DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = RoleFor(entry),
                Attitude = entry.DefaultAttitudePool == RHAH_AttitudePool.Positive ? RHAH_Attitude.LeaningFriendly : RHAH_Attitude.Hostile,
                CarriesPlague = entry.Category == RHAH_IncidentCategory.Plague,
                Map = map,
                SpawnCell = cell,
                PawnCount = RHAH_IncidentScale.Count(entry.DisplayId, parms == null ? 0f : parms.points),
                Points = parms == null ? 0f : parms.points
            });
        }

        RHAH_IncidentEntry Entry => def == null ? null : RHAH_IncidentCatalog.GetByDefName(def.defName);

        static RHAH_PawnRole RoleFor(RHAH_IncidentEntry entry)
        {
            switch (entry.Family)
            {
                case RHAH_IncidentFamily.Thief: return RHAH_PawnRole.Thief;
                case RHAH_IncidentFamily.Wild: return RHAH_PawnRole.Wild;
                case RHAH_IncidentFamily.Siege: return RHAH_PawnRole.Siege;
                case RHAH_IncidentFamily.Trade: return RHAH_PawnRole.Trader;
                case RHAH_IncidentFamily.Aid: return RHAH_PawnRole.Refugee;
                case RHAH_IncidentFamily.Intel: return RHAH_PawnRole.Envoy;
                case RHAH_IncidentFamily.Special: return RHAH_PawnRole.Refugee;
                default: return RHAH_PawnRole.Beggar;
            }
        }

        static int CountFor(RHAH_IncidentEntry entry)
        {
            if (entry.Family == RHAH_IncidentFamily.Siege || entry.Family == RHAH_IncidentFamily.Thief)
            {
                return 3;
            }

            return entry.Family == RHAH_IncidentFamily.Beggar ? 2 : 1;
        }
    }
}
