using System.Text;
using HungerAndHavoc.Core;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class WorldObject_RHAH_Approach : WorldObject
    {
        string displayId = "";
        float points;
        int mapId;
        int nextTileId = -1;
        int costLeft;
        bool crossing;
        bool arrivalPending;
        bool arriving;

        public override string Label => "RHAH_Approach_Label".Translate();

        public override Vector3 DrawPos
        {
            get
            {
                if (!Spawned || Find.WorldGrid == null || !Tile.Valid)
                {
                    return base.DrawPos;
                }

                Vector3 start = Find.WorldGrid.GetTileCenter(Tile);
                if (nextTileId < 0 || costLeft <= 0)
                {
                    return start;
                }

                PlanetTile next = new PlanetTile(nextTileId, Tile.Layer);
                if (!next.Valid)
                {
                    return start;
                }

                int total = RHAH_ApproachRules.MoveTicks(1f, 1f);
                float traveled = total <= 0 ? 1f : 1f - Mathf.Clamp01(costLeft / (float)total);
                return Vector3.Slerp(start, Find.WorldGrid.GetTileCenter(next), traveled);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref displayId, "displayId", "");
            Scribe_Values.Look(ref points, "points", 0f);
            Scribe_Values.Look(ref mapId, "mapId", 0);
            Scribe_Values.Look(ref nextTileId, "nextTileId", -1);
            Scribe_Values.Look(ref costLeft, "costLeft", 0);
            Scribe_Values.Look(ref crossing, "crossing", false);
            Scribe_Values.Look(ref arrivalPending, "arrivalPending", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && displayId == null)
            {
                displayId = "";
            }
        }

        public override string GetInspectString()
        {
            StringBuilder text = new StringBuilder();
            text.Append("RHAH_Approach_Inspect".Translate(IncidentLabel()));
            if (mapId > 0)
            {
                text.AppendLine();
                text.Append("RHAH_Approach_Target".Translate(MapLabel()));
            }

            return text.ToString();
        }

        public void Configure(string incidentId, float incidentPoints, int targetMapId, bool crossSea)
        {
            displayId = incidentId ?? "";
            points = incidentPoints;
            mapId = targetMapId;
            crossing = crossSea;
        }

        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (arriving || !Spawned || delta <= 0)
            {
                return;
            }

            int left = delta;
            while (left > 0 && !arriving)
            {
                if (costLeft > 0)
                {
                    int spent = costLeft < left ? costLeft : left;
                    costLeft = RHAH_ApproachRules.Spend(costLeft, spent);
                    left -= spent;
                    if (costLeft > 0)
                    {
                        return;
                    }
                }

                if (arrivalPending)
                {
                    Arrive();
                    return;
                }

                if (nextTileId >= 0)
                {
                    PlanetTile entered = new PlanetTile(nextTileId, Tile.Layer);
                    nextTileId = -1;
                    if (entered.Valid)
                    {
                        Tile = entered;
                    }
                }

                if (OnDestination())
                {
                    Arrive();
                    return;
                }

                if (!Step())
                {
                    Arrive();
                    return;
                }
            }
        }

        bool Step()
        {
            Map map = Home();
            if (map?.Tile == null || !map.Tile.Valid || !Tile.Valid || map.Tile.Layer != Tile.Layer || Find.World == null)
            {
                return false;
            }

            WorldPath path = map.Tile.Layer.Pather.FindPath(Tile, map.Tile, null);
            if (!RHAH_ApproachRules.TryConsumeNext(path, out PlanetTile next))
            {
                if (RHAH_ApproachRules.ReleasePath(path == null, path == WorldPath.NotFound))
                {
                    path.ReleaseToPool();
                }

                return false;
            }

            path.ReleaseToPool();
            if (!next.Valid || !RHAH_ApproachRules.CanWalk(Find.World.Impassable(next), crossing))
            {
                return false;
            }

            nextTileId = next.tileId;
            float difficulty = WorldPathGrid.CalculatedMovementDifficultyAt(next, false, null, null);
            float road = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(Tile, next, null);
            costLeft = RHAH_ApproachRules.MoveTicks(difficulty, road);
            return true;
        }

        bool OnDestination()
        {
            Map map = Home();
            return map != null && map.Tile.Valid && Tile == map.Tile;
        }

        void Arrive()
        {
            if (arriving)
            {
                return;
            }

            arrivalPending = true;
            arriving = true;
            try
            {
                GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
                RHAH_ArrivalResult result = game == null
                    ? RHAH_ArrivalResult.Waiting
                    : game.SpawnArrived(displayId, points, mapId);
                if (result == RHAH_ArrivalResult.Waiting)
                {
                    costLeft = GenDate.TicksPerHour;
                    return;
                }

                Destroy();
            }
            finally
            {
                arriving = false;
            }
        }

        Map Home()
        {
            if (Find.Maps == null)
            {
                return null;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                Map map = Find.Maps[i];
                if (map != null && map.IsPlayerHome && map.uniqueID == mapId)
                {
                    return map;
                }
            }

            return null;
        }

        string IncidentLabel()
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            return entry == null ? displayId : entry.LabelKey.Translate();
        }

        string MapLabel()
        {
            Map map = Home();
            return map == null ? mapId.ToString() : map.Parent.LabelCap;
        }
    }
}
