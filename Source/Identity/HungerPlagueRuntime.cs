using System.Collections.Generic;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Identity
{
    internal static class HungerPlagueRuntime
    {
        internal static bool IsQuarantined(Verse.Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            Map map = pawn.MapHeld ?? pawn.Map;
            MapComponent_HungerAndHavoc component = map?.GetComponent<MapComponent_HungerAndHavoc>();
            return component != null && component.IsQuarantined(pawn.thingIDNumber);
        }

        internal static void TickMap(MapComponent_HungerAndHavoc component, Map map)
        {
            if (component == null || map == null || !map.IsPlayerHome || Find.TickManager == null)
            {
                return;
            }

            if (Find.TickManager.TicksGame % 60 != 0 || GenLocalDate.HourInteger(map) != HungerPlague.SpreadHour)
            {
                return;
            }

            int absoluteDay = GenLocalDate.Year(map) * 60 + GenLocalDate.DayOfYear(map);
            SettleQuarantine(component, map);
            TryScheduleReturn(component, map);
            AdvanceReturn(component, map);
            if (!HungerPlague.IsSpreadDay(absoluteDay, component.PlagueLastSpreadDay))
            {
                return;
            }

            component.PlagueLastSpreadDay = absoluteDay;
            Spread(map);
        }

        static void Spread(Map map)
        {
            HediffDef def = HungerPlague.Def;
            if (def == null || map.mapPawns == null)
            {
                return;
            }

            IReadOnlyList<Verse.Pawn> spawned = map.mapPawns.AllPawnsSpawned;
            int carriers = 0;
            for (int i = 0; i < spawned.Count; i++)
            {
                Verse.Pawn pawn = spawned[i];
                if (HungerPlague.IsCarrier(pawn, HungerRace.IsRatkin(pawn?.def)))
                {
                    carriers++;
                }
            }

            float chance = HungerPlague.SpreadChance(carriers);
            if (chance <= 0f)
            {
                return;
            }

            List<Verse.Pawn> infected = new List<Verse.Pawn>();
            List<Verse.Pawn> colonists = map.mapPawns.FreeColonistsSpawned;
            for (int i = 0; i < colonists.Count; i++)
            {
                Verse.Pawn pawn = colonists[i];
                float pumping = pawn?.health?.capacities == null
                    ? 0f
                    : pawn.health.capacities.GetLevel(PawnCapacityDefOf.BloodPumping) * 100f;
                if (!HungerPlague.CanReceive(pawn, pumping) || !Rand.Chance(chance))
                {
                    continue;
                }

                if (HungerPlague.TryInfect(pawn, null))
                {
                    infected.Add(pawn);
                }
            }

            if (infected.Count > 0)
            {
                Messages.Message(
                    "RHAH_PlagueSpreading".Translate(),
                    infected,
                    MessageTypeDefOf.NegativeHealthEvent,
                    historical: true);
            }
        }

        internal static void SettleQuarantine(MapComponent_HungerAndHavoc component, Map map)
        {
            List<int> ids = component.PlagueQuarantineLoadIds;
            if (!HungerPlague.QuarantineOpen(ids))
            {
                return;
            }

            PlagueWatch watch = new PlagueWatch();
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                int loadId = ids[i];
                Verse.Pawn pawn = FindPawn(loadId);
                bool dead = pawn == null || pawn.Dead || pawn.Destroyed;
                bool left = pawn != null && pawn.MapHeld != map;
                bool sick = HungerPlague.HasActive(pawn);
                if (!dead && !left && sick)
                {
                    continue;
                }

                watch.Entries.Add(new PlagueWatchEntry
                {
                    LoadId = loadId,
                    Dead = dead,
                    LeftMap = left && !dead,
                    StillSick = sick,
                    Counted = false
                });
                ids.RemoveAt(i);
            }

            PlagueTally tally = HungerPlague.ResolveQuarantine(watch);
            component.PlagueRecovered += tally.Recovered;
            component.PlagueDied += tally.Died;
            if (tally.Recovered > 0 || tally.Died > 0)
            {
                Current.Game?.GetComponent<Narrative.NarrativeState>()?.NotePlague(
                    new Narrative.SuiyinPlagueFact(map.uniqueID, tally.Recovered, tally.Died));
            }
            RememberReturnCandidate(watch);
        }

        static void RememberReturnCandidate(PlagueWatch watch)
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (game == null || game.PlagueReturnLoadId != 0)
            {
                return;
            }

            int chosen = HungerPlague.ChooseReturn(game.PlagueReturnLoadId, watch.Entries);
            if (chosen > 0)
            {
                game.PlagueReturnLoadId = chosen;
                game.PlagueReturnDueTick = Find.TickManager.TicksGame + GenDate.TicksPerDay * HungerPlague.ReturnDelayDays;
                game.PlagueReturnPhase = 1;
            }
        }

        static void TryScheduleReturn(MapComponent_HungerAndHavoc component, Map map)
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (game == null || game.PlagueReturnPhase != 1 || Find.TickManager.TicksGame < game.PlagueReturnDueTick)
            {
                return;
            }

            if (game.PlagueReturnMapId != 0 && game.PlagueReturnMapId != map.uniqueID)
            {
                return;
            }

            Verse.Pawn pawn = FindPawn(game.PlagueReturnLoadId);
            if (pawn == null || pawn.Dead || pawn.Destroyed || HungerPlague.HasActive(pawn) || pawn.Spawned)
            {
                CancelReturn(game);
                return;
            }

            if (!CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell))
            {
                return;
            }

            GenSpawn.Spawn(pawn, cell, map);
            game.PlagueReturnMapId = map.uniqueID;
            game.PlagueReturnPhase = 2;
            game.PlagueReturnLeaveTick = Find.TickManager.TicksGame + GenDate.TicksPerDay * HungerPlague.ReturnStayDays;
            component.RegisterVisitor(pawn.thingIDNumber);
        }

        static void AdvanceReturn(MapComponent_HungerAndHavoc component, Map map)
        {
            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            if (game == null || game.PlagueReturnPhase != 2 || game.PlagueReturnMapId != map.uniqueID)
            {
                return;
            }

            Verse.Pawn pawn = FindPawn(game.PlagueReturnLoadId);
            if (pawn == null || pawn.Dead || HungerPlague.HasActive(pawn) || Find.TickManager.TicksGame >= game.PlagueReturnLeaveTick)
            {
                if (pawn != null && pawn.Spawned && pawn.Map == map)
                {
                    pawn.ExitMap(false, Rot4.Invalid);
                }

                component.RemoveVisitor(game.PlagueReturnLoadId);
                game.PlagueReturnPhase = 3;
            }
        }

        static void CancelReturn(GameComponent_HungerAndHavoc game)
        {
            game.PlagueReturnPhase = 3;
            game.PlagueReturnDueTick = -1;
            game.PlagueReturnLeaveTick = -1;
        }

        static Verse.Pawn FindPawn(int loadId)
        {
            if (loadId <= 0 || Find.WorldPawns == null)
            {
                return null;
            }

            List<Verse.Pawn> pawns = PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != null && pawns[i].thingIDNumber == loadId)
                {
                    return pawns[i];
                }
            }

            return null;
        }
    }
}
