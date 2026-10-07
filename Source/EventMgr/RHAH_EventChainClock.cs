using System.Collections.Generic;
using HungerAndHavoc.Core;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.EventMgr
{
    internal static class RHAH_EventChainClock
    {
        internal const int CheckInterval = 1000;

        internal static bool Due(int tick, int interval)
        {
            return tick >= 0 && interval > 0 && tick % interval == 0;
        }

        internal static void NoteStarted(string displayId, int mapId, int caravanId, int tick, int batchId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                return;
            }

            int site = RHAH_EventChainRuntime.SiteId(mapId, caravanId);
            bool longChain = RHAH_Mod.Settings != null && RHAH_Mod.Settings.IncidentUsesLongChain(displayId);
            int deadline = RHAH_EventFollowRules.OpensLong(displayId, longChain)
                ? RHAH_EventFollowRules.LongDeadline(displayId, tick)
                : -1;
            string payload = RHAH_EventFollowRules.Payload(displayId, batchId, site);
            int id = game.StartEventChain(displayId, site, tick, deadline, payload);
            if (id > 0)
            {
                game.SetEventChainStage(id, longChain ? 1 : 0, deadline, payload);
            }
        }

        internal static void Check(int interval)
        {
            if (Find.TickManager == null || !Due(Find.TickManager.TicksGame, interval) || Current.Game == null)
            {
                return;
            }

            GameComponent_RHAH_Game game = Current.Game.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                return;
            }

            int tick = Find.TickManager.TicksGame;
            List<Map> maps = Find.Maps;
            if (maps != null)
            {
                for (int i = 0; i < maps.Count; i++)
                {
                    Map map = maps[i];
                    if (map != null && map.IsPlayerHome &&
                        RHAH_ApproachRules.AllowsHome(RHAH_Approach.SpaceHome(map), RHAH_Approach.SpaceApproachEnabled()))
                    {
                        game.CheckEventChains(new RHAH_EventChainSite(map.uniqueID, 0, map.Tile.tileId), tick);
                    }
                }
            }

            List<RimWorld.Planet.WorldObject> objects = Find.WorldObjects?.AllWorldObjects;
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                RimWorld.Planet.Caravan caravan = objects[i] as RimWorld.Planet.Caravan;
                if (caravan != null && caravan.IsPlayerControlled && caravan.PawnsListForReading.Count > 0)
                {
                    game.CheckEventChains(new RHAH_EventChainSite(0, caravan.ID, caravan.Tile.tileId), tick);
                }
            }
        }
    }
}
