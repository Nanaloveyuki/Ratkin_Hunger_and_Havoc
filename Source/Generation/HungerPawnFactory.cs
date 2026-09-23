using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using HungerAndHavoc.Identity;
using HungerAndHavoc.Pawn;
using RimWorld;
using Verse;
using VersePawn = Verse.Pawn;
namespace HungerAndHavoc.Generation
{
    internal static class HungerPawnFactory
    {
        internal static HungerPawnCreationResult Create(HungerPawnRequest request)
        {
            HungerPawnCreationResult validation = Validate(request);
            if (validation != null)
            {
                return validation;
            }

            if (HungerAndHavocRuntime.IsBatchActive(request.Map, request.SpawnBatchId))
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.DuplicateBatch);
            }

            VersePawn pawn = Generate(request);
            if (pawn == null)
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.GenerationFailed);
            }

            List<VersePawn> created = new List<VersePawn> { pawn };
            HungerPawnSeed seed = new HungerPawnSeed(
                request.SourceIncidentDisplayId,
                request.SpawnBatchId,
                request.RelationshipGroupId,
                request.Role,
                HungerLifecycle.Arriving,
                request.CarriesPlague,
                request.AttitudeAtArrival,
                -1,
                request.ParentPawnLoadId,
                request.ChildPawnLoadIds);

            if (HungerAndHavocApi.TryMarkOrigin(pawn, seed) == null)
            {
                Cleanup(created);
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.MarkingFailed);
            }


            if (!TryApplyRelationships(pawn, request))
            {
                Cleanup(created);
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.RelationshipFailed);
            }

            if (request.Map != null && request.SpawnCell.IsValid && !pawn.Spawned)
            {
                GenSpawn.Spawn(pawn, request.SpawnCell, request.Map);
            }

            if (request.Map != null && !pawn.Spawned)
            {
                Cleanup(created);
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.GenerationFailed);
            }

            HungerPlague.InfectCarrier(pawn, request.CarriesPlague);
            if (request.CarriesPlague)
            {
                request.Map?.GetComponent<MapComponent_HungerAndHavoc>()?.Quarantine(pawn.thingIDNumber);
            }
            RHAH_VisitorGroup.TryStart(created, request.Map, request.SpawnCell, request.Role);
            HungerAndHavocRuntime.RegisterBatch(request.Map, request.SpawnBatchId);
            return HungerPawnCreationResult.Success(created);
        }

        static HungerPawnCreationResult Validate(HungerPawnRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.SourceIncidentDisplayId) ||
                request.SpawnBatchId <= 0 || request.RelationshipGroupId <= 0 ||
                request.Role == HungerPawnRole.Unspecified)
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.InvalidRequest);
            }

            if (request.PawnKind == null)
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.NoPawnKind);
            }

            if (request.Map != null &&
                (request.SpawnCell == IntVec3.Invalid || !request.SpawnCell.InBounds(request.Map)))
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.InvalidRequest);
            }

            return null;
        }

        static VersePawn Generate(HungerPawnRequest request)
        {
            HungerPawnProfile profile = request.Profile ?? new HungerPawnProfile();
            VersePawn pawn = PawnGenerator.GeneratePawn(HungerGenerationOptimizer.BuildRequest(request, profile));
            if (pawn == null)
            {
                return null;
            }

            ApplyProfile(pawn, profile);
            HungerContentApplier.Apply(pawn, request, !profile.UseExplicitBackstory);

            HungerXenotypeResolver.ApplyEnabledGenes(pawn);
            return pawn;
        }

        static void ApplyProfile(VersePawn pawn, HungerPawnProfile profile)
        {
            if (profile.UseExplicitBackstory)
            {
                ApplyBackstory(pawn, profile);
            }

            if (profile.UseExplicitApparel)
            {
                ApplyApparel(pawn, profile);
            }

            if (profile.UseExplicitHealth)
            {
                ApplyHealth(pawn, profile);
            }
        }

        static void ApplyBackstory(VersePawn pawn, HungerPawnProfile profile)
        {
            if (pawn.story == null)
            {
                return;
            }

            pawn.story.Childhood = profile.Childhood;
            pawn.story.Adulthood = pawn.ageTracker != null && pawn.ageTracker.AgeBiologicalYearsFloat >= 20f
                ? profile.Adulthood
                : null;
        }

        static void ApplyApparel(VersePawn pawn, HungerPawnProfile profile)
        {
            if (pawn.apparel == null)
            {
                return;
            }

            pawn.apparel.DestroyAll();
            for (int i = 0; i < profile.Apparel.Count; i++)
            {
                ThingDef def = profile.Apparel[i];
                if (def == null || !def.IsApparel || !ApparelUtility.HasPartsToWear(pawn, def))
                {
                    continue;
                }

                Thing thing = ThingMaker.MakeThing(def);
                Apparel apparel = thing as Apparel;
                if (apparel != null)
                {
                    pawn.apparel.Wear(apparel, false, false);
                }
            }
        }

        static void ApplyHealth(VersePawn pawn, HungerPawnProfile profile)
        {
            if (pawn.health == null)
            {
                return;
            }

            for (int i = 0; i < profile.Hediffs.Count; i++)
            {
                HediffDef def = profile.Hediffs[i];
                if (def != null && !pawn.health.hediffSet.HasHediff(def))
                {
                    pawn.health.AddHediff(def);
                }
            }
        }

        static bool TryApplyRelationships(Verse.Pawn pawn, HungerPawnRequest request)
        {
            if (request.ParentPawnLoadId == 0 &&
                (request.ChildPawnLoadIds == null || request.ChildPawnLoadIds.Length == 0))
            {
                return true;
            }

            IHungerPawn snapshot = HungerAndHavocApi.Get(pawn);
            return snapshot != null && snapshot.RelationshipGroupId == request.RelationshipGroupId;
        }

        static void Cleanup(List<Verse.Pawn> pawns)
        {
            for (int i = 0; i < pawns.Count; i++)
            {
                VersePawn pawn = pawns[i];
                if (pawn == null)
                {
                    continue;
                }

                RHAH_VisitorGroup.NotifyReleased(pawn);
                if (!pawn.Destroyed)
                {
                    pawn.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
