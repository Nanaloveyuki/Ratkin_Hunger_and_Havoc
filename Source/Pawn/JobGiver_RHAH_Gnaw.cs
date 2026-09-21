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

            if (!HungerAndHavocApi.Allows(pawn, HungerBehaviorGate.Gnaw))
            {
                return null;
            }

            if (pawn.Map == null || pawn.Downed)
            {
                return null;
            }

            Thing target = FindGnawTarget(pawn);
            if (target == null)
            {
                return null;
            }

            return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_Gnaw, target);
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
