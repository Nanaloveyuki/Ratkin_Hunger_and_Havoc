using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Generation;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class HungerIncidentFacts
    {
        internal static bool Submit(HungerIncidentContext context)
        {
            if (context == null || context.Map == null || context.PawnCount <= 0 ||
                context.SpawnCell == IntVec3.Invalid || context.SpawnBatchId <= 0 ||
                context.RelationshipGroupId <= 0 || context.Role == HungerPawnRole.Unspecified)
            {
                return false;
            }

            List<HungerPawnCreationResult> created = new List<HungerPawnCreationResult>();
            for (int i = 0; i < context.PawnCount; i++)
            {
                HungerPawnCreationResult result = HungerPawnFactory.Create(new HungerPawnRequest
                {
                    SourceIncidentDisplayId = context.DisplayId,
                    SpawnBatchId = context.SpawnBatchId,
                    RelationshipGroupId = context.RelationshipGroupId,
                    Role = context.Role,
                    AttitudeAtArrival = context.Attitude,
                    CarriesPlague = context.CarriesPlague,
                    Map = context.Map,
                    PawnKind = PawnKindDefOf.Colonist,
                    Faction = HungerAndHavoc.Pawn.RHAH_AttitudeFactions.Resolve(context.Attitude) ?? Faction.OfPlayer,
                    SpawnCell = context.SpawnCell,
                    Profile = context.Profile
                });

                if (!result.Succeeded)
                {
                    Rollback(created);
                    return false;
                }

                created.Add(result);
            }

            return true;
        }

        static void Rollback(List<HungerPawnCreationResult> created)
        {
            for (int i = 0; i < created.Count; i++)
            {
                for (int j = 0; j < created[i].Pawns.Count; j++)
                {
                    Verse.Pawn pawn = created[i].Pawns[j];
                    if (pawn != null && !pawn.Destroyed)
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
    }
}
