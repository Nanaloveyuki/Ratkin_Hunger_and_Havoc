using System;
using HungerAndHavoc.Api;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Feed : ThinkNode_JobGiver
    {
        const float SearchRadius = 30f;

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

            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.FeedFromRelief))
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed || JobDefOf.Ingest == null)
            {
                return null;
            }

            Need_Food need = pawn.needs != null ? pawn.needs.food : null;
            if (need == null)
            {
                return null;
            }

            Thing food = FindFood(pawn);
            if (food == null)
            {
                return null;
            }

            Pawn_InventoryTracker holder = food.ParentHolder as Pawn_InventoryTracker;
            Verse.Pawn carrier = holder != null ? holder.pawn : null;
            if (carrier != null && carrier != pawn && JobDefOf.TakeFromOtherInventory != null)
            {
                Job take = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, food, carrier);
                take.count = FoodUtility.WillIngestStackCountOf(
                    pawn,
                    food.def,
                    FoodUtility.NutritionForEater(pawn, food));
                return take;
            }

            Job ingest = JobMaker.MakeJob(JobDefOf.Ingest, food);
            ingest.count = FoodUtility.WillIngestStackCountOf(
                pawn,
                food.def,
                FoodUtility.NutritionForEater(pawn, food));
            return ingest;
        }

        static Thing FindFood(Verse.Pawn pawn)
        {
            Predicate<Thing> validator = thing =>
                thing != null &&
                thing.IngestibleNow &&
                pawn.RaceProps != null &&
                pawn.RaceProps.CanEverEat(thing) &&
                pawn.CanReserve(thing);
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree),
                PathEndMode.ClosestTouch,
                TraverseParms.For(pawn),
                SearchRadius,
                validator);
        }
    }
}
