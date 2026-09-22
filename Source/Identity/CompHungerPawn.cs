using HungerAndHavoc.Api;
using Verse;

namespace HungerAndHavoc.Identity
{
    public class CompProperties_HungerPawn : HediffCompProperties
    {
        public CompProperties_HungerPawn()
        {
            compClass = typeof(CompHungerPawn);
        }
    }

    public class CompHungerPawn : HediffComp
    {
        readonly HungerPawnState state = new HungerPawnState();

        internal HungerPawnState State => state;

        internal string RoleLabelKey => "RHAH_Role_" + state.role;

        internal HungerPawnSnapshot ToSnapshot()
        {
            return state.ToSnapshot();
        }

        internal void ApplySeed(HungerPawnSeed seed)
        {
            state.ApplySeed(seed);
        }

        internal void SetLeaveAfter(int tick)
        {
            state.SetLeaveAfter(tick);
        }

        internal static CompHungerPawn TryGet(Verse.Pawn pawn)
        {
            Hediff_HungerMark mark = TryGetMark(pawn);
            return mark == null ? null : mark.Hunger;
        }

        internal static Hediff_HungerMark TryGetMark(Verse.Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null)
            {
                return null;
            }

            return pawn.health.hediffSet.GetFirstHediff<Hediff_HungerMark>();
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref state.sourceIncidentDisplayId, "sourceIncidentDisplayId");
            Scribe_Values.Look(ref state.spawnBatchId, "spawnBatchId", 0);
            Scribe_Values.Look(ref state.relationshipGroupId, "relationshipGroupId", 0);
            Scribe_Values.Look(ref state.role, "role", HungerPawnRole.Unspecified);
            Scribe_Values.Look(ref state.lifecycle, "lifecycle", HungerLifecycle.Arriving);
            Scribe_Values.Look(ref state.hasBeenFed, "hasBeenFed", false);
            Scribe_Values.Look(ref state.leaveAfterGameTick, "leaveAfterGameTick", -1);
            Scribe_Values.Look(ref state.carriesPlague, "carriesPlague", false);
            Scribe_Values.Look(ref state.attitudeAtArrival, "attitudeAtArrival", HungerAttitude.Neutral);
            Scribe_Values.Look(ref state.attitude, "attitude", HungerAttitude.Neutral);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && state.attitude == HungerAttitude.Neutral &&
                state.attitudeAtArrival != HungerAttitude.Neutral)
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
