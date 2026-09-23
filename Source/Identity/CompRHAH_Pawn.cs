using HungerAndHavoc.Api;
using Verse;

namespace HungerAndHavoc.Identity
{
    public class CompProperties_RHAH_Pawn : HediffCompProperties
    {
        public CompProperties_RHAH_Pawn()
        {
            compClass = typeof(CompRHAH_Pawn);
        }
    }

    public class CompRHAH_Pawn : HediffComp
    {
        readonly RHAH_PawnState state = new RHAH_PawnState();

        internal RHAH_PawnState State => state;

        internal string RoleLabelKey => "RHAH_Role_" + state.role;

        internal RHAH_PawnSnapshot ToSnapshot()
        {
            return state.ToSnapshot();
        }

        internal void ApplySeed(RHAH_PawnSeed seed)
        {
            state.ApplySeed(seed);
        }

        internal void SetLeaveAfter(int tick)
        {
            state.SetLeaveAfter(tick);
        }

        internal static CompRHAH_Pawn TryGet(Verse.Pawn pawn)
        {
            Hediff_RHAH_Mark mark = TryGetMark(pawn);
            return mark == null ? null : mark.Hunger;
        }

        internal static Hediff_RHAH_Mark TryGetMark(Verse.Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return null;
            }

            return pawn.health.hediffSet.GetFirstHediff<Hediff_RHAH_Mark>();
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref state.sourceIncidentDisplayId, "sourceIncidentDisplayId");
            Scribe_Values.Look(ref state.spawnBatchId, "spawnBatchId", 0);
            Scribe_Values.Look(ref state.relationshipGroupId, "relationshipGroupId", 0);
            Scribe_Values.Look(ref state.role, "role", RHAH_PawnRole.Unspecified);
            Scribe_Values.Look(ref state.lifecycle, "lifecycle", RHAH_Lifecycle.Arriving);
            Scribe_Values.Look(ref state.hasBeenFed, "hasBeenFed", false);
            Scribe_Values.Look(ref state.leaveAfterGameTick, "leaveAfterGameTick", -1);
            Scribe_Values.Look(ref state.carriesPlague, "carriesPlague", false);
            Scribe_Values.Look(ref state.attitudeAtArrival, "attitudeAtArrival", RHAH_Attitude.Neutral);
            Scribe_Values.Look(ref state.attitude, "attitude", RHAH_Attitude.Neutral);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && state.attitude == RHAH_Attitude.Neutral &&
                state.attitudeAtArrival != RHAH_Attitude.Neutral)
            {
                state.attitude = state.attitudeAtArrival;
            }
            Scribe_Values.Look(ref state.parentPawnLoadId, "parentPawnLoadId", 0);
            Scribe_Collections.Look(ref state.childPawnLoadIds, "childPawnLoadIds", LookMode.Value);
            Scribe_Collections.Look(ref state.droppedChildLoadIds, "droppedChildLoadIds", LookMode.Value);
            Scribe_Collections.Look(ref state.gateOverrides, "gateOverrides", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref state.extraData, "extraData", LookMode.Value, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                state.EnsureCollections();
            }
        }
    }
}
