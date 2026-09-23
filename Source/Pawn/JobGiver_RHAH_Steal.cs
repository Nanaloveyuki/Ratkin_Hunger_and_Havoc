using HungerAndHavoc.Api;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Steal : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return TryCreate(pawn);
        }

        // 供无 Lord 的总 JobGiver 调度
        internal static Job TryCreate(Verse.Pawn pawn)
        {
            if (!RHAH_Api.IsVisitor(pawn))
            {
                return null;
            }

            if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Steal))
            {
                return null;
            }

            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot != null && snapshot.HasBeenFed)
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed || JobDefOf.Steal == null)
            {
                return null;
            }

            if (GenAI.InDangerousCombat(pawn))
            {
                return null;
            }

            IntVec3 exit;
            if (!RCellFinder.TryFindBestExitSpot(pawn, out exit))
            {
                return null;
            }

            Thing item;
            if (!StealAIUtility.TryFindBestItemToSteal(pawn.Position, pawn.Map, 12f, out item, pawn, null) ||
                item == null)
            {
                return TryTakeFoodFromInventory(pawn);
            }

            Job job = JobMaker.MakeJob(JobDefOf.Steal, item, exit);
            float volume = item.def.VolumePerUnit;
            int carry = volume > 0f
                ? (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / volume)
                : item.stackCount;
            job.count = Mathf.Max(1, Mathf.Min(item.stackCount, carry));
            return job;
        }

        static Job TryTakeFoodFromInventory(Verse.Pawn pawn)
        {
            if (JobDefOf.TakeFromOtherInventory == null)
            {
                return null;
            }

            foreach (Verse.Pawn colonist in pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (colonist == null || colonist == pawn || colonist.inventory == null)
                {
                    continue;
                }

                if (!pawn.CanReach(colonist, PathEndMode.Touch, Danger.Deadly))
                {
                    continue;
                }

                ThingOwner inner = colonist.inventory.innerContainer;
                if (inner == null)
                {
                    continue;
                }

                for (int i = 0; i < inner.Count; i++)
                {
                    Thing thing = inner[i];
                    if (thing == null || !thing.IngestibleNow)
                    {
                        continue;
                    }

                    if (pawn.RaceProps != null && !pawn.RaceProps.CanEverEat(thing))
                    {
                        continue;
                    }

                    Job job = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, colonist);
                    job.count = FoodUtility.WillIngestStackCountOf(
                        pawn,
                        thing.def,
                        FoodUtility.NutritionForEater(pawn, thing));
                    return job;
                }
            }

            return null;
        }
    }
}
