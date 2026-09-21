using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
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

            if (!pawn.Spawned)
            {
                Cleanup(created);
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.GenerationFailed);
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

            if (request.Map == null)
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.NoMap);
            }

            if (request.PawnKind == null)
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.NoPawnKind);
            }

            if (request.SpawnCell == IntVec3.Invalid || !request.SpawnCell.InBounds(request.Map))
            {
                return HungerPawnCreationResult.Failed(HungerPawnCreationFailure.InvalidRequest);
            }

            return null;
        }

        static VersePawn Generate(HungerPawnRequest request)
        {
            VersePawn pawn = PawnGenerator.GeneratePawn(request.PawnKind, request.Faction);
            if (pawn != null && request.BiologicalAge.HasValue)
            {
                pawn.ageTracker.AgeBiologicalTicks = (long)(request.BiologicalAge.Value * 3600000f);
            }

            return pawn;
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
