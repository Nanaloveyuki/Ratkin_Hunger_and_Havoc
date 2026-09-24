using System;
using Verse.AI.Group;
using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_FoodHandoff
    {
        internal static void Begin(List<Verse.Pawn> pawns)
        {
            if (pawns == null || pawns.Count == 0)
            {
                return;
            }

            Verse.Pawn receiver = Receiver(pawns);
            if (receiver == null)
            {
                return;
            }

            Lord lord = receiver.GetLord();
            LordJob_RHAH_Visitor job = lord == null ? null : lord.LordJob as LordJob_RHAH_Visitor;
            int count = RHAH_RequestRules.FoodRequestCount(pawns.Count);
            if (job == null || count <= 0)
            {
                return;
            }

            job.BeginFoodWait(receiver, count);
            if (lord.CurLordToil is LordToil_RHAH_VisitorSeek)
            {
                lord.ReceiveMemo("RHAH_WaitFood");
            }
        }

        internal static Verse.Pawn Receiver(List<Verse.Pawn> pawns)
        {
            Verse.Pawn best = null;
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn pawn = pawns[i];
                if (pawn == null || pawn.Dead || !pawn.Spawned || pawn.Downed)
                {
                    continue;
                }

                if (best == null || pawn.ageTracker.AgeBiologicalYears > best.ageTracker.AgeBiologicalYears)
                {
                    best = pawn;
                }
            }

            return best;
        }

        internal static Thing FindFood(Verse.Pawn giver)
        {
            List<Thing> foods = new List<Thing>();
            CollectFood(giver, foods);
            return foods.Count == 0 ? null : foods[0];
        }

        internal static void CollectFood(Verse.Pawn giver, List<Thing> result)
        {
            if (result == null || giver == null || giver.Map == null || giver.Map.listerThings == null)
            {
                return;
            }

            List<Thing> things = giver.Map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
            if (things == null)
            {
                return;
            }

            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (Accepts(giver, thing))
                {
                    result.Add(thing);
                }
            }

            result.Sort((left, right) =>
            {
                float leftDistance = left == null ? float.MaxValue : (left.PositionHeld - giver.Position).LengthHorizontalSquared;
                float rightDistance = right == null ? float.MaxValue : (right.PositionHeld - giver.Position).LengthHorizontalSquared;
                int distance = leftDistance.CompareTo(rightDistance);
                if (distance != 0)
                {
                    return distance;
                }

                string leftName = left == null || left.def == null ? null : left.def.defName;
                string rightName = right == null || right.def == null ? null : right.def.defName;
                return string.Compare(leftName, rightName, StringComparison.Ordinal);
            });
        }

        internal static int Received(Verse.Pawn receiver, ThingDef food)
        {
            if (receiver == null || receiver.inventory == null || food == null)
            {
                return 0;
            }

            int count = 0;
            ThingOwner<Thing> container = receiver.inventory.innerContainer;
            for (int i = 0; i < container.Count; i++)
            {
                if (container[i] != null && container[i].def == food)
                {
                    count += container[i].stackCount;
                }
            }

            return count;
        }

        static bool Accepts(Verse.Pawn giver, Thing thing)
        {
            return thing != null &&
                thing.Spawned &&
                !thing.IsForbidden(giver) &&
                giver.CanReserve(thing) &&
                RHAH_ReliefFood.GiveFoodAllowed(thing.def) &&
                thing.def.IsIngestible &&
                thing.def.ingestible.preferability >= FoodPreferability.MealAwful;
        }
    }
}
