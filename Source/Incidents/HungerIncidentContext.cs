using HungerAndHavoc.Generation;
using HungerAndHavoc.Api;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal sealed class HungerIncidentContext
    {
        public string DisplayId { get; set; }
        public int SpawnBatchId { get; set; }
        public int RelationshipGroupId { get; set; }
        public HungerPawnRole Role { get; set; }
        public HungerAttitude Attitude { get; set; }
        public bool CarriesPlague { get; set; }
        public Map Map { get; set; }
        public IntVec3 SpawnCell { get; set; }
        public int PawnCount { get; set; }
        public float Points { get; set; }
        public HungerPawnProfile Profile { get; set; }
    }
}
