using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using RimWorld;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobGiver_RHAH_DropChild : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            return TryCreate(pawn);
        }

        internal static Job TryCreate(Verse.Pawn pawn)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (comp == null || pawn.Map == null || settings == null)
            {
                return null;
            }

            bool leaving = comp.State.lifecycle == HungerLifecycle.Leaving;
            if (!RHAH_FamilyRules.CanDrop(comp.State.role, leaving, settings.familyDropEnabled))
            {
                return null;
            }

            if (RHAH_FamilyRules.AllDropped(comp.State.childPawnLoadIds, comp.State.droppedChildLoadIds))
            {
                return null;
            }

            for (int i = 0; i < comp.State.childPawnLoadIds.Count; i++)
            {
                int childId = comp.State.childPawnLoadIds[i];
                if (comp.State.droppedChildLoadIds.Contains(childId))
                {
                    continue;
                }

                Verse.Pawn child = FindChild(pawn.Map, childId);
                if (child == null)
                {
                    continue;
                }

                RHAH_FamilyRules.MarkDropped(comp.State.droppedChildLoadIds, childId);
                return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_DropChild, child);
            }

            return null;
        }

        static Verse.Pawn FindChild(Map map, int loadId)
        {
            IReadOnlyList<Verse.Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i] != null && pawns[i].thingIDNumber == loadId)
                {
                    return pawns[i];
                }
            }

            return null;
        }
    }

    public class JobDriver_RHAH_DropChild : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil drop = ToilMaker.MakeToil("DropChild");
            drop.initAction = () =>
            {
                Verse.Pawn child = job.GetTarget(TargetIndex.A).Pawn;
                if (child != null && child.CarriedBy == pawn)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing _);
                }
            };
            drop.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return drop;
        }
    }

    public class JobGiver_RHAH_MotherFeed : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            CompHungerPawn comp = CompHungerPawn.TryGet(pawn);
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (comp == null || pawn.Map == null || settings == null || !HungerAndHavocApi.IsVisitor(pawn))
            {
                return null;
            }

            IReadOnlyList<Verse.Pawn> children = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < comp.State.childPawnLoadIds.Count; i++)
            {
                Verse.Pawn child = null;
                for (int j = 0; j < children.Count; j++)
                {
                    if (children[j] != null && children[j].thingIDNumber == comp.State.childPawnLoadIds[i])
                    {
                        child = children[j];
                        break;
                    }
                }

                bool hungry = child != null && child.needs?.food != null && child.needs.food.CurLevelPercentage < 0.3f;
                if (!RHAH_FamilyRules.CanMotherFeed(comp.State.role, settings.motherFeedEnabled, hungry))
                {
                    continue;
                }

                return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_MotherFeed, child);
            }

            return null;
        }
    }

    public class JobDriver_RHAH_MotherFeed : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
        }

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 180, true, false, false, TargetIndex.A);
        }
    }

    public class JobGiver_RHAH_Scavenge : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            bool hungry = pawn != null && pawn.needs?.food != null && pawn.needs.food.CurLevelPercentage < 0.2f;
            if (pawn == null || settings == null || !RHAH_FamilyRules.CanScavenge(settings.prisonerScavengeEnabled, pawn.IsPrisoner, hungry))
            {
                return null;
            }

            return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_Scavenge, pawn);
        }
    }

    public class JobDriver_RHAH_Scavenge : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Wait(120);
        }
    }

    public class JobGiver_RHAH_TailBite : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Verse.Pawn pawn)
        {
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (pawn == null || pawn.Map == null || settings == null)
            {
                return null;
            }

            bool hungry = pawn.needs?.food != null && pawn.needs.food.CurLevelPercentage < 0.2f;
            if (!RHAH_FamilyRules.CanTailBite(settings.tailBiteEnabled, pawn.IsPrisoner, hungry, false, 0f))
            {
                return null;
            }

            IReadOnlyList<Verse.Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Verse.Pawn target = pawns[i];
                if (target == null || target == pawn || !target.IsPrisoner)
                {
                    continue;
                }

                float age = target.ageTracker == null ? 99f : target.ageTracker.AgeBiologicalYearsFloat;
                if (RHAH_FamilyRules.CanTailBite(true, true, true, !target.Awake(), age))
                {
                    return JobMaker.MakeJob(HungerAndHavocDefOf.RHAH_TailBite, target);
                }
            }

            return null;
        }
    }

    public class JobDriver_RHAH_TailBite : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
        }

        protected override System.Collections.Generic.IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 150, true, false, false, TargetIndex.A);
        }
    }
}
