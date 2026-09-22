using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Generation
{
    internal sealed class HungerPawnRequest
    {
        public string SourceIncidentDisplayId { get; set; }
        public int SpawnBatchId { get; set; }
        public int RelationshipGroupId { get; set; }
        public HungerPawnRole Role { get; set; }
        public HungerAttitude AttitudeAtArrival { get; set; }
        public bool CarriesPlague { get; set; }
        public Map Map { get; set; }
        public PawnKindDef PawnKind { get; set; }
        public Gender? Gender { get; set; }
        public float? BiologicalAge { get; set; }
        public Faction Faction { get; set; }
        public IntVec3 SpawnCell { get; set; }
        public int ParentPawnLoadId { get; set; }
        public int[] ChildPawnLoadIds { get; set; }
        public HungerPawnProfile Profile { get; set; }
    }
}
