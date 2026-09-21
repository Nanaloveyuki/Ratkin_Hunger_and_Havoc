using HungerAndHavoc.Api;
using HungerAndHavoc.Generation;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal abstract class IncidentWorker_HungerGroup : IncidentWorker
    {
        protected abstract string DisplayId { get; }
        protected abstract HungerPawnRole Role { get; }
        protected abstract HungerAttitude Attitude { get; }
        protected virtual bool CarriesPlague => false;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return parms?.target is Map map &&
                map.mapPawns != null &&
                HungerAndHavocRuntimeAllows() &&
                FindAnySpawnCell(map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms?.target as Map;
            if (map == null || !CanFireNowSub(parms))
            {
                return false;
            }

            IntVec3 cell;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null))
            {
                return false;
            }

            HungerIncidentContext context = new HungerIncidentContext
            {
                DisplayId = DisplayId,
                SpawnBatchId = Find.TickManager.TicksGame,
                RelationshipGroupId = Find.TickManager.TicksGame,
                Role = Role,
                Attitude = Attitude,
                CarriesPlague = CarriesPlague,
                Map = map,
                SpawnCell = cell,
                PawnCount = 1
            };

            return SubmitFacts(context);
        }

        protected abstract bool SubmitFacts(HungerIncidentContext context);

        static bool FindAnySpawnCell(Map map)
        {
            IntVec3 cell;
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal, false, null);
        }

        static bool HungerAndHavocRuntimeAllows()
        {
            return Core.HungerAndHavocRuntime.AllowsNewContent;
        }
    }

    internal sealed class IncidentWorker_LargeRefugeeWave : IncidentWorker_HungerGroup
    {
        protected override string DisplayId => "I-001";
        protected override HungerPawnRole Role => HungerPawnRole.Refugee;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;

        protected override bool SubmitFacts(HungerIncidentContext context)
        {
            return HungerIncidentFacts.Submit(context);
        }
    }

    internal sealed class IncidentWorker_ThiefRatkinGroup : IncidentWorker_HungerGroup
    {
        protected override string DisplayId => "I-006";
        protected override HungerPawnRole Role => HungerPawnRole.Thief;
        protected override HungerAttitude Attitude => HungerAttitude.Hostile;

        protected override bool SubmitFacts(HungerIncidentContext context)
        {
            return HungerIncidentFacts.Submit(context);
        }
    }
}
