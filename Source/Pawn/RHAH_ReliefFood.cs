using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Pawn.Compat;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    internal enum RHAH_FoodReject
    {
        None = 0,
        Missing,
        NotIngestible,
        WillNotEat,
        Zone,
        Forbidden,
        Unreachable,
        Reserved,
        NoStand,
        EmptyDispenser,
        DisabledFood,
        TooYoung
    }

    internal static class RHAH_ReliefFood
    {
        internal const float SatisfiedLevel = 0.82f;
        internal const float RefeedMalnutrition = 0.4f;
        internal const int RetryBaseTicks = 250;
        internal const float SearchRadius = 40f;

        internal static bool ReliefRulesApply
        {
            get
            {
                RHAH_Settings settings = RHAH_Mod.Settings;
                return settings == null || settings.reliefEnabled;
            }
        }

        internal static bool MayEatOutside(Verse.Pawn pawn)
        {
            if (!ReliefRulesApply)
            {
                return true;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            if (settings != null && !settings.allowEatOutsideRelief)
            {
                return false;
            }

            return RHAH_Api.Allows(pawn, RHAH_BehaviorGate.EatOutsideRelief);
        }

        internal static bool FoodAllowed(ThingDef def)
        {
            if (!ReliefRulesApply || def == null)
            {
                return true;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            return settings == null || settings.IsReliefFoodEnabled(def.defName);
        }

        internal static RHAH_FoodReject Reject(Verse.Pawn pawn, Thing food, bool insideZone)
        {
            return Reject(pawn, food, insideZone, false);
        }

        internal static RHAH_FoodReject Reject(
            Verse.Pawn pawn,
            Thing food,
            bool insideZone,
            bool ignoreZone)
        {
            if (pawn == null || food == null || food.Destroyed || food.MapHeld != pawn.Map)
            {
                return RHAH_FoodReject.Missing;
            }

            if (!RHAH_ChildMovement.CanFeedSelf(pawn))
            {
                return RHAH_FoodReject.TooYoung;
            }

            if (!food.IngestibleNow && !IsHarvestable(food) && !IsDispenser(food))
            {
                return RHAH_FoodReject.NotIngestible;
            }

            ThingDef eaten = EatenDef(food);
            if (eaten == null || pawn.RaceProps == null || !pawn.RaceProps.CanEverEat(eaten))
            {
                return RHAH_FoodReject.WillNotEat;
            }

            if (!ignoreZone && !FoodAllowed(eaten))
            {
                return RHAH_FoodReject.DisabledFood;
            }

            Need_Food need = pawn.needs != null ? pawn.needs.food : null;
            bool starving = need != null && need.CurCategory == HungerCategory.Starving;
            if (!pawn.WillEat(eaten, pawn, true, false) && !starving)
            {
                return RHAH_FoodReject.WillNotEat;
            }

            if (!ignoreZone && ReliefRulesApply && !InRequestedZone(pawn.Map, food, insideZone))
            {
                return RHAH_FoodReject.Zone;
            }

            if ((food.PositionHeld - pawn.Position).LengthManhattan > SearchRadius)
            {
                return RHAH_FoodReject.Unreachable;
            }

            if (food.IsForbidden(pawn))
            {
                return RHAH_FoodReject.Forbidden;
            }

            if (!pawn.CanReserve(food))
            {
                return RHAH_FoodReject.Reserved;
            }

            if (IsDispenser(food))
            {
                return RejectDispenser(pawn, food);
            }

            if (IsHarvestable(food))
            {
                return pawn.CanReach(food, PathEndMode.Touch, Danger.Deadly)
                    ? RHAH_FoodReject.None
                    : RHAH_FoodReject.Unreachable;
            }

            if (!pawn.CanReach(food, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return RHAH_FoodReject.Unreachable;
            }

            if (RHAH_ChildMovement.EatsOnFloor(pawn))
            {
                return RHAH_FoodReject.None;
            }

            IntVec3 spot;
            if (!Toils_Ingest.TryFindChairOrSpot(pawn, food, out spot) ||
                !spot.IsValid ||
                !spot.InBounds(pawn.Map) ||
                !spot.Standable(pawn.Map) ||
                !pawn.CanReserveSittableOrSpot(spot, false))
            {
                return RHAH_FoodReject.NoStand;
            }

            return RHAH_FoodReject.None;
        }

        internal static Job MakeJob(Verse.Pawn pawn, Thing food)
        {
            if (pawn == null || food == null || JobDefOf.Ingest == null)
            {
                return null;
            }

            if (IsHarvestable(food) && JobDefOf.Harvest != null)
            {
                return JobMaker.MakeJob(JobDefOf.Harvest, food);
            }

            Pawn_InventoryTracker holder = food.ParentHolder as Pawn_InventoryTracker;
            Verse.Pawn carrier = holder != null ? holder.pawn : null;
            if (carrier != null && carrier != pawn && JobDefOf.TakeFromOtherInventory != null)
            {
                Job take = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, food, carrier);
                take.count = StackCount(pawn, food);
                return take;
            }

            Job ingest = JobMaker.MakeJob(JobDefOf.Ingest, food);
            ingest.count = IsDispenser(food) ? 1 : StackCount(pawn, food);
            return ingest;
        }

        internal static int StackCount(Verse.Pawn pawn, Thing food)
        {
            ThingDef eaten = EatenDef(food);
            if (pawn == null || food == null || eaten == null)
            {
                return 1;
            }

            float nutrition = FoodUtility.NutritionForEater(pawn, food);
            int count = FoodUtility.WillIngestStackCountOf(pawn, eaten, nutrition);
            return count < 1 ? 1 : count;
        }

        internal static ThingDef EatenDef(Thing food)
        {
            if (food == null || food.def == null)
            {
                return null;
            }

            if (IsDispenser(food))
            {
                Building_NutrientPasteDispenser dispenser = food as Building_NutrientPasteDispenser;
                return dispenser != null ? dispenser.DispensableDef : ThingDefOf.MealNutrientPaste;
            }

            if (IsHarvestable(food))
            {
                return food.def.plant != null ? food.def.plant.harvestedThingDef : null;
            }

            return food.def;
        }

        internal static bool IsDispenser(Thing food)
        {
            return food is Building_NutrientPasteDispenser ||
                   (food != null && food.def != null && food.def.building != null && food.def.building.isMealSource);
        }

        internal static bool IsHarvestable(Thing food)
        {
            Plant plant = food as Plant;
            return plant != null &&
                   plant.HarvestableNow &&
                   plant.def != null &&
                   plant.def.plant != null &&
                   plant.def.plant.harvestedThingDef != null &&
                   plant.def.plant.harvestedThingDef.IsNutritionGivingIngestible;
        }

        static RHAH_FoodReject RejectDispenser(Verse.Pawn pawn, Thing food)
        {
            Building_NutrientPasteDispenser dispenser = food as Building_NutrientPasteDispenser;
            if (dispenser != null && !dispenser.HasEnoughFeedstockInHoppers())
            {
                return RHAH_FoodReject.EmptyDispenser;
            }

            IntVec3 cell = food.InteractionCell;
            if (!cell.IsValid || !cell.InBounds(pawn.Map) || !cell.Standable(pawn.Map) ||
                !pawn.CanReserveSittableOrSpot(cell, false) ||
                !pawn.CanReach(food, PathEndMode.InteractionCell, Danger.Deadly))
            {
                return RHAH_FoodReject.NoStand;
            }

            return RHAH_FoodReject.None;
        }

        static bool InRequestedZone(Map map, Thing food, bool insideZone)
        {
            if (food.ParentHolder is Pawn_InventoryTracker)
            {
                return false;
            }

            bool inside = RHAH_ReliefArea.Contains(map, food.PositionHeld);
            return insideZone ? inside : !inside;
        }

        internal static List<ThingDef> CandidateFoods()
        {
            List<ThingDef> result = new List<ThingDef>();
            AppendCandidateFoods(result);
            return result;
        }

        internal static void AppendCandidateFoods(List<ThingDef> result)
        {
            if (result == null || DefDatabase<ThingDef>.AllDefsListForReading == null)
            {
                return;
            }

            HashSet<ThingDef> seen = new HashSet<ThingDef>(result);
            List<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < defs.Count; i++)
            {
                ThingDef def = defs[i];
                if (def != null && def.IsNutritionGivingIngestible && seen.Add(def))
                {
                    result.Add(def);
                }
            }

            if (ThingDefOf.MealNutrientPaste != null && seen.Add(ThingDefOf.MealNutrientPaste))
            {
                result.Add(ThingDefOf.MealNutrientPaste);
            }
        }
    }
}
