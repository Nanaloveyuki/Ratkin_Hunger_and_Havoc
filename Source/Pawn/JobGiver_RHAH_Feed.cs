using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Feed : ThinkNode_JobGiver
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

            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.FeedFromRelief))
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed || JobDefOf.Ingest == null)
            {
                return null;
            }

            IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
            if (snapshot != null && snapshot.HasBeenFed)
            {
                return null;
            }

            Need_Food need = pawn.needs != null ? pawn.needs.food : null;
            if (need == null)
            {
                return null;
            }

            Thing carried = FoodInInventory(pawn);
            if (carried != null && RHAH_ReliefFood.Reject(pawn, carried, false) == RHAH_FoodReject.None)
            {
                return RHAH_ReliefFood.MakeJob(pawn, carried);
            }

            MapComponent_HungerAndHavoc mapState = pawn.Map.GetComponent<MapComponent_HungerAndHavoc>();
            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            if (mapState != null && !mapState.FoodSearchReady(pawn.thingIDNumber, now))
            {
                return null;
            }

            bool insideOnly = RHAH_ReliefFood.ReliefRulesApply && !RHAH_ReliefFood.MayEatOutside(pawn);
            if (insideOnly && RHAH_ReliefArea.IsEmpty(pawn.Map))
            {
                RememberMiss(mapState, pawn, now);
                return null;
            }

            Thing food = FindFood(pawn, true);
            if (food == null && !insideOnly)
            {
                food = FindFood(pawn, false);
            }

            if (food == null)
            {
                RememberMiss(mapState, pawn, now);
                return null;
            }

            return RHAH_ReliefFood.MakeJob(pawn, food);
        }

        static void RememberMiss(MapComponent_HungerAndHavoc mapState, Verse.Pawn pawn, int now)
        {
            if (mapState == null || pawn == null)
            {
                return;
            }

            int wait = RHAH_ReliefFood.RetryBaseTicks + (pawn.thingIDNumber % 60);
            mapState.SetFoodSearchTick(pawn.thingIDNumber, now + wait);
        }

        static Thing FoodInInventory(Verse.Pawn pawn)
        {
            if (pawn.inventory == null || pawn.inventory.innerContainer == null)
            {
                return null;
            }

            ThingOwner<Thing> container = pawn.inventory.innerContainer;
            for (int i = 0; i < container.Count; i++)
            {
                Thing thing = container[i];
                if (thing != null && thing.IngestibleNow && RHAH_ReliefFood.FoodAllowed(thing.def))
                {
                    return thing;
                }
            }

            return null;
        }

        static Thing FindFood(Verse.Pawn pawn, bool insideZone)
        {
            if (insideZone && RHAH_ReliefArea.IsEmpty(pawn.Map))
            {
                return null;
            }

            Thing best = null;
            float bestDist = float.MaxValue;
            List<Thing> foods = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
            Consider(pawn, foods, insideZone, ref best, ref bestDist);
            List<Thing> plants = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HarvestablePlant);
            Consider(pawn, plants, insideZone, ref best, ref bestDist);
            return best;
        }

        static void Consider(
            Verse.Pawn pawn,
            List<Thing> things,
            bool insideZone,
            ref Thing best,
            ref float bestDist)
        {
            if (things == null)
            {
                return;
            }

            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (RHAH_ReliefFood.Reject(pawn, thing, insideZone) != RHAH_FoodReject.None)
                {
                    continue;
                }

                float dist = thing.PositionHeld.DistanceToSquared(pawn.Position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = thing;
                }
            }
        }
    }
}
