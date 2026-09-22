using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace HungerAndHavoc.Caravan
{
    internal static class CaravanTargetResolver
    {
        internal static RimWorld.Planet.Caravan ResolvePlayerCaravan()
        {
            List<RimWorld.Planet.Caravan> caravans = new List<RimWorld.Planet.Caravan>();
            List<WorldObject> objects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is RimWorld.Planet.Caravan caravan && caravan.IsPlayerControlled && caravan.PawnsListForReading.Count > 0)
                {
                    caravans.Add(caravan);
                }
            }

            return caravans.Count == 0 ? null : caravans.RandomElement();
        }
    }
}
