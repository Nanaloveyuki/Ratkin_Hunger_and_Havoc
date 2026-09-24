using System;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_Gnaw : ThinkNode_JobGiver
    {
        const float SearchRadius = 40f;
        internal const float StarvationLevel = 0.05f;


        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return null;
        }

        // 只由饥饿觅食调度 不从寻食 duty 主动发
        internal static Job TryCreate(Verse.Pawn pawn, bool needFood)
        {
            if (!RHAH_Api.IsVisitor(pawn))
            {
                return null;
            }

            if (!RHAH_VisitorRules.AllowsModBehavior(RHAH_VisitorStay.Kind(pawn)))
            {
                return null;
            }

            if (!RHAH_Api.Allows(pawn, RHAH_BehaviorGate.Gnaw))
            {
                return null;
            }

            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot != null && snapshot.HasBeenFed)
            {
                return null;
            }

            bool busy = pawn.jobs != null && pawn.jobs.curJob != null;
            bool downed = pawn != null && pawn.Downed;
            Need_Food food = pawn.needs != null ? pawn.needs.food : null;
            float level = food == null ? 1f : food.CurLevelPercentage;
            if (!RHAH_VisitorRules.AllowsGnaw(true, true, true, needFood, level, StarvationLevel, busy, downed))
            {
                return null;
            }

            if (pawn.Map == null)
            {
                return null;
            }


            Thing target = FindGnawTarget(pawn);
            if (target == null)
            {
                return null;
            }

            return JobMaker.MakeJob(RHAH_DefOf.RHAH_Gnaw, target);
        }

        static Thing FindGnawTarget(Verse.Pawn pawn)
        {
            TraverseParms traverse = TraverseParms.For(pawn);
            Predicate<Thing> tree = thing => IsPlant(thing, pawn, true);
            Thing found = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Plant),
                PathEndMode.Touch,
                traverse,
                SearchRadius,
                tree);
            if (found != null)
            {
                return found;
            }

            Predicate<Thing> plant = thing => IsPlant(thing, pawn, false);
            found = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Plant),
                PathEndMode.Touch,
                traverse,
                SearchRadius,
                plant);
            if (found != null)
            {
                return found;
            }

            Predicate<Thing> wall = thing => IsWallLike(thing, pawn);
            return GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.Touch,
                traverse,
                SearchRadius,
                wall);
        }

        static bool IsPlant(Thing thing, Verse.Pawn pawn, bool treeOnly)
        {
            if (thing == null || thing.def.plant == null)
            {
                return false;
            }

            if (treeOnly && !thing.def.plant.IsTree)
            {
                return false;
            }

            return pawn.CanReserve(thing);
        }

        static bool IsWallLike(Thing thing, Verse.Pawn pawn)
        {
            if (thing == null || thing.def.category != ThingCategory.Building)
            {
                return false;
            }

            if (thing.def.Fillage != FillCategory.Full)
            {
                return false;
            }

            return pawn.CanReserve(thing);
        }
    }
}
