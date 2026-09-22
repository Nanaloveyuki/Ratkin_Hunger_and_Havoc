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
            HungerIncidentEntry entry = Entry;
            if (entry == null || !Core.HungerAndHavocRuntime.AllowsNewContent)
            {
                return false;
            }

            if (entry.DisplayId == "I-038")
            {
                return Core.HungerMapResolver.Resolve(parms?.target as Map) != null;
            }

            if (entry.Target == HungerIncidentTarget.Caravan)
            {
                return Caravan.CaravanTargetResolver.ResolvePlayerCaravan() != null;
            }

            Map map = Core.HungerMapResolver.Resolve(parms?.target as Map);
            return map != null && FindAnySpawnCell(map);
        }
        static bool FindAnySpawnCell(Map map)
        {
            IntVec3 cell;
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            HungerIncidentEntry entry = Entry;
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

            if (entry.Target == HungerIncidentTarget.Caravan)
            {
                return TradeEventRouter.TrySpawnCaravanAmbush(entry);
            }

            Map map = Core.HungerMapResolver.Resolve(parms?.target as Map);
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
