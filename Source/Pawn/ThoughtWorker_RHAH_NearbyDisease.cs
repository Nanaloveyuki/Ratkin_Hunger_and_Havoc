using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    public class ThoughtWorker_RHAH_NearbyDisease : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Verse.Pawn pawn)
        {
            if (pawn?.Map?.mapPawns == null || pawn.Dead || pawn.Suspended)
            {
                return ThoughtState.Inactive;
            }

            foreach (Verse.Pawn other in pawn.Map.mapPawns.AllPawnsSpawned)
            {
                if (other == null || other == pawn || other.Dead)
                {
                    continue;
                }

                if (other.health?.hediffSet?.AnyHediffMakesSickThought == true)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
            }

            return ThoughtState.Inactive;
        }
    }
}
