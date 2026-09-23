using System.Collections.Generic;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Identity
{
    internal static class RHAH_PlagueRuntime
    {
        static readonly Dictionary<int, int> missingSinceTicks = new Dictionary<int, int>();
        static readonly List<int> recoveredLoadIds = new List<int>();
        static readonly List<int> pendingReturnLoadIds = new List<int>();
        static readonly List<PlagueWatchEntry> watchEntries = new List<PlagueWatchEntry>();


        internal static bool IsQuarantined(Verse.Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            Map map = pawn.MapHeld ?? pawn.Map;
            MapComponent_RHAH_Map component = map?.GetComponent<MapComponent_RHAH_Map>();
            return component != null && component.IsQuarantined(pawn.thingIDNumber);
        }

        internal static void TickMap(MapComponent_RHAH_Map component, Map map)
        {
            if (component == null || map == null || !map.IsPlayerHome || Find.TickManager == null)
            {
                return;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            bool plagueOn = settings == null || settings.plagueEnabled;
            if (!plagueOn)
            {
                return;
            }

            int spreadHour = settings == null ? RHAH_Plague.SpreadHour : settings.plagueSpreadHour;
            if (Find.TickManager.TicksGame % 60 != 0 || GenLocalDate.HourInteger(map) != spreadHour)
            {
                return;
            }

            int absoluteDay = GenLocalDate.Year(map) * 60 + GenLocalDate.DayOfYear(map);
            SettleQuarantine(component, map, Find.TickManager.TicksGame);
            if (settings == null || settings.plagueReturnEnabled)
            {
                TryScheduleReturn(component, map);
                AdvanceReturn(component, map);
            }

            int interval = settings == null ? RHAH_Plague.SpreadDayInterval : settings.plagueSpreadDayInterval;
            if (!RHAH_Plague.IsSpreadDay(absoluteDay, component.PlagueLastSpreadDay, interval))
            {
                return;
            }

            component.PlagueLastSpreadDay = absoluteDay;
            Spread(map, settings);
        }

        static void Spread(Map map, RHAH_Settings settings)
        {
            HediffDef def = RHAH_Plague.Def;
            if (def == null || map.mapPawns == null)
            {
                return;
            }

            IReadOnlyList<Verse.Pawn> spawned = map.mapPawns.AllPawnsSpawned;
            int carriers = 0;
            for (int i = 0; i < spawned.Count; i++)
            {
                Verse.Pawn pawn = spawned[i];
                if (RHAH_Plague.IsCarrier(pawn, RHAH_Race.IsRatkin(pawn?.def)))
                {
                    carriers++;
                }
            }

            float perCarrier = settings == null ? RHAH_Plague.SpreadChancePerCarrier : settings.plagueSpreadChancePerCarrier;
            float cap = settings == null ? RHAH_Plague.SpreadChanceCap : settings.plagueSpreadChanceCap;
            float chance = RHAH_Plague.SpreadChance(carriers, perCarrier, cap);
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
                float skip = settings == null ? RHAH_Plague.BloodPumpingSkipPercent : settings.plagueBloodPumpingSkipPercent;
                if (!RHAH_Plague.CanReceive(pawn, pumping, skip) || !Rand.Chance(chance))
                {
                    continue;
                }

                if (RHAH_Plague.TryInfect(pawn, null))
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

        internal static void SettleQuarantine(MapComponent_RHAH_Map component, Map map, int tick)
        {
            List<int> ids = component.PlagueQuarantineLoadIds;
            if (!RHAH_Plague.QuarantineOpen(ids))
            {
                return;
            }

            watchEntries.Clear();
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                int loadId = ids[i];
                Verse.Pawn pawn = FindPawn(loadId);
                if (pawn == null)
                {
                    if (tick - MissingSince(loadId, tick) < GenDate.TicksPerDay)
                    {
                        continue;
                    }

                    watchEntries.Add(new PlagueWatchEntry { LoadId = loadId, Missing = true });

                    ids.RemoveAt(i);
                    missingSinceTicks.Remove(loadId);
                    continue;
                }

                missingSinceTicks.Remove(loadId);
                bool dead = pawn.Dead || pawn.Destroyed;
                bool here = !dead && pawn.MapHeld == map;
                bool sick = RHAH_Plague.HasActive(pawn);
                if (here && sick)
                {
                    continue;
                }

                watchEntries.Add(new PlagueWatchEntry
                {
                    LoadId = loadId,
                    Dead = dead,
                    LeftMap = !dead && !here,
                    StillSick = sick
                });
                ids.RemoveAt(i);
            }

            PlagueWatch watch = new PlagueWatch { Entries = watchEntries };
            PlagueTally tally = RHAH_Plague.ResolveQuarantine(watch);
            component.PlagueRecovered += tally.Recovered;
            component.PlagueDied += tally.Died;
            RememberRecovered();
            if (tally.Recovered > 0 || tally.Died > 0)
            {
                Current.Game?.GetComponent<Narrative.NarrativeState>()?.NotePlague(
                    new Narrative.SuiyinPlagueFact(map.uniqueID, tally.Recovered, tally.Died));
            }

            RememberReturnCandidate();
            watchEntries.Clear();
        }

        static int MissingSince(int loadId, int tick)
        {
            int since;
            if (!missingSinceTicks.TryGetValue(loadId, out since))
            {
                missingSinceTicks[loadId] = tick;
                return tick;
            }

            return since;
        }

        static void RememberRecovered()
        {
            for (int i = 0; i < watchEntries.Count; i++)
            {
                PlagueWatchEntry entry = watchEntries[i];
                if (entry == null || !entry.Counted || entry.Dead || entry.StillSick || entry.LoadId <= 0)
                {
                    continue;
                }

                if (entry.LeftMap)
                {
                    if (!recoveredLoadIds.Contains(entry.LoadId))
                    {
                        recoveredLoadIds.Add(entry.LoadId);
                    }
                }
                else if (!pendingReturnLoadIds.Contains(entry.LoadId))
                {
                    pendingReturnLoadIds.Add(entry.LoadId);
                }
            }
        }

        static void RememberReturnCandidate()
        {
            for (int i = pendingReturnLoadIds.Count - 1; i >= 0; i--)
            {
                int loadId = pendingReturnLoadIds[i];
                Verse.Pawn pawn = FindPawn(loadId);
                if (pawn == null || pawn.Dead || pawn.Destroyed || RHAH_Plague.HasActive(pawn))
                {
                    pendingReturnLoadIds.RemoveAt(i);
                    continue;
                }

                if (pawn.MapHeld != null)
                {
                    continue;
                }

                pendingReturnLoadIds.RemoveAt(i);
                if (!recoveredLoadIds.Contains(loadId))
                {
                    recoveredLoadIds.Add(loadId);
                }
            }

            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null || game.PlagueReturnLoadId != 0 || Find.TickManager == null)
            {
                return;
            }

            int chosen = RHAH_Plague.ChooseReturn(game.PlagueReturnLoadId, recoveredLoadIds);
            if (chosen <= 0)
            {
                return;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            int delay = settings == null ? RHAH_Plague.ReturnDelayDays : settings.plagueReturnDelayDays;
            game.PlagueReturnLoadId = chosen;
            game.PlagueReturnDueTick = Find.TickManager.TicksGame + GenDate.TicksPerDay * delay;
            game.PlagueReturnPhase = 1;
        }

        static void TryScheduleReturn(MapComponent_RHAH_Map component, Map map)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null || game.PlagueReturnPhase != 1 || Find.TickManager.TicksGame < game.PlagueReturnDueTick)
            {
                return;
            }

            if (game.PlagueReturnMapId != 0 && game.PlagueReturnMapId != map.uniqueID)
            {
                return;
            }

            Verse.Pawn pawn = FindPawn(game.PlagueReturnLoadId);
            if (pawn == null || pawn.Dead || pawn.Destroyed || RHAH_Plague.HasActive(pawn) || pawn.Spawned)
            {
                CancelReturn(game);
                return;
            }

            if (!CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 cell))
            {
                return;
            }

            GenSpawn.Spawn(pawn, cell, map);
            RHAH_Settings settings = RHAH_Mod.Settings;
            int stay = settings == null ? RHAH_Plague.ReturnStayDays : settings.plagueReturnStayDays;
            game.PlagueReturnMapId = map.uniqueID;
            game.PlagueReturnPhase = 2;
            game.PlagueReturnLeaveTick = Find.TickManager.TicksGame + GenDate.TicksPerDay * stay;
            component.RegisterVisitor(pawn.thingIDNumber);
        }

        static void AdvanceReturn(MapComponent_RHAH_Map component, Map map)
        {
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null || game.PlagueReturnPhase != 2 || game.PlagueReturnMapId != map.uniqueID)
            {
                return;
            }

            Verse.Pawn pawn = FindPawn(game.PlagueReturnLoadId);
            if (pawn == null || pawn.Dead || RHAH_Plague.HasActive(pawn) || Find.TickManager.TicksGame >= game.PlagueReturnLeaveTick)
            {
                if (pawn != null && pawn.Spawned && pawn.Map == map)
                {
                    pawn.ExitMap(false, Rot4.Invalid);
                }

                component.RemoveVisitor(game.PlagueReturnLoadId);
                game.PlagueReturnPhase = 3;
            }
        }

        static void CancelReturn(GameComponent_RHAH_Game game)
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
