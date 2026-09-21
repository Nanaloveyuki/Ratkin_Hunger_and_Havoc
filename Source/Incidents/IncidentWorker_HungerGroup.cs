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
        protected virtual int PawnCount => 1;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return parms?.target is Map map && map.mapPawns != null &&
                HungerAndHavocRuntimeAllows() && FindAnySpawnCell(map);
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

            int tick = Find.TickManager.TicksGame;
            return SubmitFacts(new HungerIncidentContext
            {
                DisplayId = DisplayId,
                SpawnBatchId = tick,
                RelationshipGroupId = tick,
                Role = Role,
                Attitude = Attitude,
                CarriesPlague = CarriesPlague,
                Map = map,
                SpawnCell = cell,
                PawnCount = PawnCount
            });
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

    internal abstract class IncidentWorker_OriginalHungerGroup : IncidentWorker_HungerGroup
    {
        protected override bool SubmitFacts(HungerIncidentContext context) => HungerIncidentFacts.Submit(context);
    }

    internal sealed class IncidentWorker_LargeRefugeeWave : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-001";
        protected override HungerPawnRole Role => HungerPawnRole.Refugee;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
        protected override int PawnCount => 3;
    }

    internal sealed class IncidentWorker_AbandonedRatkinChildren : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-002";
        protected override HungerPawnRole Role => HungerPawnRole.BeggarChild;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
        protected override int PawnCount => 2;
    }

    internal sealed class IncidentWorker_ShatteredMother : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-003";
        protected override HungerPawnRole Role => HungerPawnRole.Mother;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
    }

    internal sealed class IncidentWorker_BeggarFamily : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-004";
        protected override HungerPawnRole Role => HungerPawnRole.Refugee;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
        protected override int PawnCount => 3;
    }

    internal sealed class IncidentWorker_BeggarGroup : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-005";
        protected override HungerPawnRole Role => HungerPawnRole.Refugee;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
        protected override int PawnCount => 4;
    }

    internal sealed class IncidentWorker_ThiefRatkinGroup : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-006";
        protected override HungerPawnRole Role => HungerPawnRole.Thief;
        protected override HungerAttitude Attitude => HungerAttitude.Hostile;
        protected override int PawnCount => 3;
    }

    internal sealed class IncidentWorker_ThiefRatkinChildGroup : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-007";
        protected override HungerPawnRole Role => HungerPawnRole.ThiefChild;
        protected override HungerAttitude Attitude => HungerAttitude.Hostile;
        protected override int PawnCount => 2;
    }

    internal sealed class IncidentWorker_WildRatkinWandersIn : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-008";
        protected override HungerPawnRole Role => HungerPawnRole.Wild;
        protected override HungerAttitude Attitude => HungerAttitude.Neutral;
    }

    internal sealed class IncidentWorker_WildRatkinChildWandersIn : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-009";
        protected override HungerPawnRole Role => HungerPawnRole.WildChild;
        protected override HungerAttitude Attitude => HungerAttitude.Neutral;
    }

    internal sealed class IncidentWorker_WildRatkinGroupWandersIn : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-010";
        protected override HungerPawnRole Role => HungerPawnRole.Wild;
        protected override HungerAttitude Attitude => HungerAttitude.Neutral;
        protected override int PawnCount => 3;
    }

    internal sealed class IncidentWorker_FamineRefugees : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-011";
        protected override HungerPawnRole Role => HungerPawnRole.Refugee;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
        protected override int PawnCount => 4;
    }

    internal sealed class IncidentWorker_RatkinTraderCaravan : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-012";
        protected override HungerPawnRole Role => HungerPawnRole.Trader;
        protected override HungerAttitude Attitude => HungerAttitude.Neutral;
        protected override int PawnCount => 3;
    }

    internal sealed class IncidentWorker_ChildExchange : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-013";
        protected override HungerPawnRole Role => HungerPawnRole.RatkinYoung;
        protected override HungerAttitude Attitude => HungerAttitude.LeaningFriendly;
    }

    internal sealed class IncidentWorker_BeggarSiege : IncidentWorker_OriginalHungerGroup
    {
        protected override string DisplayId => "I-014";
        protected override HungerPawnRole Role => HungerPawnRole.Siege;
        protected override HungerAttitude Attitude => HungerAttitude.Hostile;
        protected override int PawnCount => 5;
    }
}
