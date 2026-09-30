using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Caravan;
using HungerAndHavoc.Generation;
using HungerAndHavoc.Incidents;
using Verse.AI.Group;
using RimWorld;
using Verse;

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
            RHAH_Attitude attitude = HungerAndHavoc.Incidents.RHAH_IncidentArrival.For(entry);
            int count = HungerAndHavoc.Incidents.RHAH_IncidentScale.Count(entry.DisplayId, parms == null ? 0f : parms.points, EventCap(), false);
            RHAH_IncidentContext context = new RHAH_IncidentContext
            {
                DisplayId = entry.DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = RHAH_PawnRole.Trader,
                Attitude = attitude,
                CarriesPlague = entry.Category == RHAH_IncidentCategory.Plague,
                Map = map,
                SpawnCell = cell,
                PawnCount = count,
                Points = parms == null ? 0f : parms.points
            };
            return HungerAndHavoc.Incidents.RHAH_IncidentFacts.Submit(context);
        }

        internal static bool TrySpawnCaravanAmbush(RHAH_IncidentEntry entry, float points, RimWorld.Planet.Caravan selected, bool allowFallback)
        {
            RimWorld.Planet.Caravan caravan = CaravanTargetResolver.Resolve(selected, allowFallback);
            RHAH_Attitude attitude = HungerAndHavoc.Incidents.RHAH_IncidentArrival.For(entry);
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
            if (!TryCreateAssaultLord(faction, map, attackers))
            {
                Cleanup(attackers);
                return false;
            }

            EventMgr.RHAH_EventChainClock.NoteStarted(entry.DisplayId, 0, caravan.ID, tick, tick + 1);
            return true;
        }

        internal static bool TryCreateAssaultLord(Faction faction, Map map, List<Verse.Pawn> attackers)
        {
            if (faction == null || map == null || attackers == null || attackers.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < attackers.Count; i++)
            {
                if (attackers[i] == null || attackers[i].Faction != faction || attackers[i].GetLord() != null)
                {
                    return false;
                }
            }

            Lord lord = LordMaker.MakeNewLord(
                faction,
                new LordJob_AssaultColony(faction, true, false, false, false, true, false, false),
                map,
                attackers);
            if (lord == null || lord.LordJob is not LordJob_AssaultColony || !OwnsAll(lord, attackers))
            {
                Disband(lord);
                return false;
            }

            return true;
        }

        static bool OwnsAll(Lord lord, List<Verse.Pawn> attackers)
        {
            if (lord == null || lord.ownedPawns.Count != attackers.Count)
            {
                return false;
            }

            for (int i = 0; i < attackers.Count; i++)
            {
                if (attackers[i].GetLord() != lord || !lord.ownedPawns.Contains(attackers[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static void Disband(Lord lord)
        {
            if (lord == null)
            {
                return;
            }

            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Verse.Pawn pawn = lord.ownedPawns[i];
                if (pawn != null && pawn.lord == lord)
                {
                    pawn.lord = null;
                    if (pawn.mindState != null)
                    {
                        pawn.mindState.duty = null;
                    }
                }
            }

            lord.ownedPawns.Clear();

            if (lord.lordManager != null)
            {
                lord.lordManager.lords.Remove(lord);
                lord.lordManager = null;
            }
        }

        static void Cleanup(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (pawn == null || pawn.Destroyed)
                {
                    continue;
                }

                Lord lord = pawn.GetLord();
                if (lord != null)
                {
                    lord.Notify_PawnLost(pawn, PawnLostCondition.Vanished, null);
                }

                pawn.Destroy(DestroyMode.Vanish);
            }
        }
        static int EventCap()
        {
            Core.RHAH_Settings settings = Core.RHAH_Mod.Settings;
            return settings == null ? RHAH_IncidentScale.DefaultEventPawns : settings.maxEventPawns;
        }
    }
}
