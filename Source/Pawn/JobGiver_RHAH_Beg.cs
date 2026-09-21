using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Beg : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return TryCreate(pawn);
        }

        // 供无 Lord 的总 JobGiver 调度
        internal static Job TryCreate(Verse.Pawn pawn)
        {
            if (!HungerAndHavocApi.IsVisitor(pawn))
            {
                return null;
            }

            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Beg))
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed)
            {
                return null;
            }

            Verse.Pawn colonist = FindClosestColonist(pawn);
            if (colonist == null)
            {
                return null;
            }

            return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_Beg, colonist);
        }

        static Verse.Pawn FindClosestColonist(Verse.Pawn pawn)
        {
            Verse.Pawn best = null;
            float bestDist = float.MaxValue;
            foreach (Verse.Pawn colonist in pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (colonist == null || colonist == pawn || colonist.Dead || colonist.Downed)
                {
                    continue;
                }

                if (!pawn.CanReach(colonist, PathEndMode.Touch, Danger.Deadly))
                {
                    continue;
                }

                float dist = colonist.Position.DistanceToSquared(pawn.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = colonist;
                }
            }

            return best;
        }
    }
}
