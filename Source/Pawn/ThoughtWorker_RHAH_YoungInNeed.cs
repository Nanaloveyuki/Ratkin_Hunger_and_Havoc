using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HungerAndHavoc.Pawn
{
    public class ThoughtWorker_RHAH_YoungInNeed : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Verse.Pawn pawn)
        {
            if (pawn?.Map?.mapPawns == null || pawn.Dead || pawn.Suspended)
            {
                return ThoughtState.Inactive;
            }

            bool hungry = false;
            Lord lord = pawn.GetLord();
            foreach (Verse.Pawn other in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (other == null || other == pawn || other.Dead || other.DevelopmentalStage.Adult())
                {
                    continue;
                }

                if (!Related(pawn, other, lord))
                {
                    continue;
                }

                if (other.Downed || other.health?.hediffSet?.AnyHediffMakesSickThought == true)
                {
                    return ThoughtState.ActiveAtStage(1);
                }

                if (other.needs?.food != null && other.needs.food.CurCategory >= HungerCategory.Hungry)
                {
                    hungry = true;
                }
            }

            return hungry ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
        }

        static bool Related(Verse.Pawn pawn, Verse.Pawn other, Lord lord)
        {
            if (pawn.Faction != null && pawn.Faction == other.Faction)
            {
                return true;
            }

            if (lord != null && other.GetLord() == lord)
            {
                return true;
            }

            return pawn.Faction == Faction.OfPlayer &&
                (other.IsColonist || other.IsPrisonerOfColony || other.IsSlaveOfColony);
        }
    }
}
