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
                Verse.Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, faction);
                if (pawn == null || HungerAndHavocApi.TryMarkOrigin(pawn, new HungerPawnSeed(
                    entry.DisplayId, tick, tick, HungerPawnRole.Thief, HungerLifecycle.Arriving,
                    entry.Category == HungerIncidentCategory.Plague, HungerAttitude.Hostile, -1, 0, null)) == null)
                {
                    Cleanup(attackers);
                    if (pawn != null && !pawn.Destroyed)
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                    return false;
                }

                attackers.Add(pawn);
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
