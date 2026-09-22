using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Caravan;
using HungerAndHavoc.Generation;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Trade
{
    internal static class TradeEventRouter
    {
        internal static bool TrySpawnTraderCaravan(HungerIncidentEntry entry, IncidentParms parms)
        {
            Map map = Core.HungerMapResolver.Resolve(parms?.target as Map);
            if (entry == null || map == null)
            {
                return false;
            }

            IntVec3 cell;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                return false;
            }

            int tick = Find.TickManager.TicksGame;
            HungerPawnCreationResult result = HungerPawnFactory.Create(new HungerPawnRequest
            {
                SourceIncidentDisplayId = entry.DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = HungerPawnRole.Trader,
                AttitudeAtArrival = HungerAttitude.Neutral,
                CarriesPlague = entry.Category == HungerIncidentCategory.Plague,
                Map = map,
                PawnKind = PawnKindDefOf.Colonist,
                Faction = Faction.OfPlayer,
                SpawnCell = cell
            });

            return result.Succeeded;
        }

        internal static bool TrySpawnCaravanAmbush(HungerIncidentEntry entry)
        {
            RimWorld.Planet.Caravan caravan = CaravanTargetResolver.ResolvePlayerCaravan();
            Faction faction = Find.FactionManager.RandomEnemyFaction();
            if (entry == null || caravan == null || faction == null ||
                !RimWorld.Planet.CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(caravan.Tile))
            {
                return false;
            }

            List<Verse.Pawn> attackers = new List<Verse.Pawn>();
            int tick = Find.TickManager.TicksGame;
            for (int i = 0; i < 3; i++)
            {
                HungerPawnCreationResult result = HungerPawnFactory.Create(new HungerPawnRequest
                {
                    SourceIncidentDisplayId = entry.DisplayId,
                    SpawnBatchId = tick + i + 1,
                    RelationshipGroupId = tick,
                    Role = HungerPawnRole.Thief,
                    AttitudeAtArrival = HungerAttitude.Hostile,
                    CarriesPlague = entry.Category == HungerIncidentCategory.Plague,
                    Map = null,
                    PawnKind = PawnKindDefOf.Colonist,
                    Faction = faction
                });
                if (!result.Succeeded)
                {
                    Cleanup(attackers);
                    return false;
                }

                attackers.Add(result.Pawns[0]);
            }

            Map map = null;
            try
            {
                map = RimWorld.Planet.CaravanIncidentUtility.SetupCaravanAttackMap(caravan, attackers, true);
            }
            catch
            {
                Cleanup(attackers);
                return false;
            }

            if (map == null)
            {
                Cleanup(attackers);
                return false;
            }

            return true;
        }

        static void Cleanup(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != null && !pawns[i].Destroyed)
                {
                    pawns[i].Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
