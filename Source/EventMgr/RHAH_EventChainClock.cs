using System.Collections.Generic;
using HungerAndHavoc.Core;
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

        internal static void NoteStarted(string displayId, int mapId, int caravanId, int tick)
        {
            if (RHAH_EventChains.Find(displayId) == null)
            {
                return;
            }

            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (game == null)
            {
                return;
            }

            game.StartEventChain(displayId, RHAH_EventChainRuntime.SiteId(mapId, caravanId), tick, -1, "");
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
                    if (map != null && map.IsPlayerHome)
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
