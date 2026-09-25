using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Caravan;
using HungerAndHavoc.Generation;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HungerAndHavoc.Trade
{
    internal static class TradeEventRouter
    {
        internal static bool TrySpawnTraderCaravan(RHAH_IncidentEntry entry, IncidentParms parms)
        {
            Map map = Core.RHAH_MapResolver.Resolve(parms?.target as Map);
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
            RHAH_PawnCreationResult result = RHAH_PawnFactory.Create(new RHAH_PawnRequest
            {
                SourceIncidentDisplayId = entry.DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = RHAH_PawnRole.Trader,
                AttitudeAtArrival = RHAH_Attitude.Neutral,
                CarriesPlague = entry.Category == RHAH_IncidentCategory.Plague,
                Map = map,
                PawnKind = Core.RHAH_DefOf.RHAH_PawnKind_Ratkin,
                Faction = HungerAndHavoc.Pawn.RHAH_AttitudeFactions.Require(RHAH_Attitude.Neutral),
                SpawnCell = cell
            });

            if (!result.Succeeded || result.Pawns == null || result.Pawns.Count == 0)
            {
                return false;
            }

            int[] visitors = new int[result.Pawns.Count];
            for (int i = 0; i < result.Pawns.Count; i++)
            {
                visitors[i] = result.Pawns[i] == null ? 0 : result.Pawns[i].thingIDNumber;
            }

            Current.Game?.GetComponent<Narrative.NarrativeState>()?.NoteIncident(new Narrative.SuiyinIncidentFact(
                entry.DisplayId,
                map.uniqueID,
                tick,
                tick,
                result.Pawns.Count,
                entry.Category == RHAH_IncidentCategory.Plague,
                visitors));
            HungerAndHavoc.Pawn.Compat.RHAH_LeashBridge.TryLeashTravel(result.Pawns[0].GetLord());
            return true;
        }

        internal static bool TrySpawnCaravanAmbush(RHAH_IncidentEntry entry, float points, RimWorld.Planet.Caravan selected, bool allowFallback)
        {
            RimWorld.Planet.Caravan caravan = CaravanTargetResolver.Resolve(selected, allowFallback);
            RHAH_Attitude attitude = IncidentWorker_Sequel.ArrivalAttitude(entry);
            Faction faction = HungerAndHavoc.Pawn.RHAH_AttitudeFactions.Resolve(attitude);
            if (entry == null || caravan == null || faction == null ||
                !RimWorld.Planet.CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(caravan.Tile))
            {
                return false;
            }

            List<Verse.Pawn> attackers = new List<Verse.Pawn>();
            int tick = Find.TickManager.TicksGame;
            int count = RHAH_IncidentScale.Count(entry.DisplayId, points, EventCap(), false);
            for (int i = 0; i < count; i++)
            {
                RHAH_PawnCreationResult result = RHAH_PawnFactory.Create(new RHAH_PawnRequest
                {
                    SourceIncidentDisplayId = entry.DisplayId,
                    SpawnBatchId = tick + i + 1,
                    RelationshipGroupId = tick,
                    Role = RHAH_PawnRole.Thief,
                    AttitudeAtArrival = attitude,
                    CarriesPlague = entry.Category == RHAH_IncidentCategory.Plague,
                    Map = null,
                    PawnKind = Core.RHAH_DefOf.RHAH_PawnKind_Ratkin,
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
        static int EventCap()
        {
            Core.RHAH_Settings settings = Core.RHAH_Mod.Settings;
            return settings == null ? RHAH_IncidentScale.DefaultEventPawns : settings.maxEventPawns;
        }
    }
}
