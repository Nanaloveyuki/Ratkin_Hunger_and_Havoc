using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using RimWorld;
using Verse;

namespace HungerAndHavoc.EventMgr
{
    internal sealed class RHAH_EventFollowChain : IRHAH_EventChain
    {
        internal const int StageShort = 0;
        internal const int StageLong = 1;
        internal const int StageDone = 2;

        public string DisplayId => "RHAH_Follow";

        public bool CanStart(RHAH_EventChainSite site, int tick)
        {
            return false;
        }

        public bool OnStart(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context)
        {
            return true;
        }

        public bool OnTick(List<RHAH_EventChainRecord> records, RHAH_EventChainContext context)
        {
            if (context == null || context.Stage >= StageDone || !RHAH_EventFollowRules.ReadPayload(context.Payload, out string displayId, out int batchId, out int mapId))
            {
                return false;
            }

            if (context.Stage == StageShort)
            {
                RHAH_EventFollowRuntime.TryPredator(mapId, batchId, context.InstanceId);
                RHAH_EventChainRuntime.SetStage(records, context.InstanceId, StageDone, context.DeadlineTick, context.Payload);
                return true;
            }

            RHAH_EventFollowRuntime.TryLong(displayId, batchId, mapId, context);
            return true;
        }

        public void OnEnd(RHAH_EventChainContext context, RHAH_EventChainEnd end)
        {
        }
    }

    internal static class RHAH_EventFollowRuntime
    {
        static readonly HashSet<int> predatorRolled = new HashSet<int>();

        internal static void TryPredator(int mapId, int batchId, int instanceId)
        {
            if (mapId <= 0 || predatorRolled.Contains(instanceId) || Current.Game == null)
            {
                return;
            }

            Map map = FindMap(mapId);
            if (map == null)
            {
                return;
            }

            predatorRolled.Add(instanceId);
            int chance = Core.RHAH_Mod.Settings == null ? RHAH_EventFollowRules.ShortPredatorPercent : Core.RHAH_Mod.Settings.followPredatorPercent;
            if (!RHAH_EventFollowRules.PredatorSelected(chance, Rand.Value))
            {
                return;
            }

            List<Verse.Pawn> prey = Batch(map, batchId);
            if (prey.Count == 0)
            {
                return;
            }

            MapComponent_RHAH_Map component = map.GetComponent<MapComponent_RHAH_Map>();
            if (component == null)
            {
                return;
            }

            for (int i = 0; i < prey.Count; i++)
            {
                component.RememberPredationPrey(prey[i]);
            }

            component.PredationSelected = true;
            component.PredationPendingTick = Find.TickManager.TicksGame + HungerAndHavoc.Incidents.RHAH_PredationRules.ArrivalDelayTicks;
        }

        internal static void TryLong(string displayId, int batchId, int mapId, RHAH_EventChainContext context)
        {
            Map map = mapId > 0 ? FindMap(mapId) : null;
            if (map == null)
            {
                return;
            }

            List<Verse.Pawn> batch = Batch(map, batchId);
            if (batch.Count == 0 || !Ready(displayId, batch))
            {
                return;
            }

            Send(displayId, batch);
            GameComponent_RHAH_Game game = Current.Game.GetComponent<GameComponent_RHAH_Game>();
            game?.EndEventChain(context.InstanceId, context.Tick, RHAH_EventChainEnd.Completed);
        }

        internal static void ClearRolls()
        {
            predatorRolled.Clear();
        }

        static bool Ready(string displayId, List<Verse.Pawn> batch)
        {
            for (int i = 0; i < batch.Count; i++)
            {
                Verse.Pawn pawn = batch[i];
                CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(pawn);
                if (comp == null || pawn.Dead || pawn.ageTracker == null)
                {
                    continue;
                }

                if (RHAH_EventFollowRules.IsYoung(comp.State.role) && pawn.ageTracker.AgeBiologicalYearsFloat >= (Core.RHAH_Mod.Settings == null ? RHAH_EventFollowRules.AdultAge : Core.RHAH_Mod.Settings.followAdultAge))
                {
                    return displayId != "I-012" || comp.State.lifecycle == RHAH_Lifecycle.Released;
                }
            }

            return false;
        }

        static void Send(string displayId, List<Verse.Pawn> batch)
        {
            Verse.Pawn focus = batch[0];
            string key = "RHAH_Follow_" + displayId;
            if (!key.CanTranslate())
            {
                return;
            }

            Find.LetterStack.ReceiveLetter(key.Translate(), (key + "_Text").Translate(), LetterDefOf.NeutralEvent, new LookTargets(focus));
        }

        static List<Verse.Pawn> Batch(Map map, int batchId)
        {
            List<Verse.Pawn> result = new List<Verse.Pawn>();
            IReadOnlyList<Verse.Pawn> spawned = map.mapPawns?.AllPawnsSpawned;
            if (spawned == null)
            {
                return result;
            }

            for (int i = 0; i < spawned.Count; i++)
            {
                CompRHAH_Pawn comp = CompRHAH_Pawn.TryGet(spawned[i]);
                if (comp != null && comp.State.spawnBatchId == batchId)
                {
                    result.Add(spawned[i]);
                }
            }

            return result;
        }

        static Map FindMap(int mapId)
        {
            List<Map> maps = Find.Maps;
            if (maps == null)
            {
                return null;
            }

            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i] != null && maps[i].uniqueID == mapId)
                {
                    return maps[i];
                }
            }

            return null;
        }
    }
}
