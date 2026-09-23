using System;
using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class RHAH_MapResolver
    {
        internal static Map Resolve(Map selectedMap = null, int? selectedMapId = null)
        {
            if (selectedMap != null && IsUsable(selectedMap))
            {
                return selectedMap;
            }

            if (selectedMapId.HasValue)
            {
                List<Map> mapsById = Find.Maps;
                for (int i = 0; i < mapsById.Count; i++)
                {
                    if (mapsById[i].uniqueID == selectedMapId.Value && IsUsable(mapsById[i]))
                    {
                        return mapsById[i];
                    }
                }

                return null;
            }

            List<Map> maps = new List<Map>();
            List<Map> allMaps = Find.Maps;
            for (int i = 0; i < allMaps.Count; i++)
            {
                if (IsUsable(allMaps[i]))
                {
                    maps.Add(allMaps[i]);
                }
            }

            return maps.Count == 0 ? null : maps.RandomElement();
        }

        static bool IsUsable(Map map)
        {
            return map != null && map.mapPawns != null && !map.Parent.Destroyed;
        }
    }
}
