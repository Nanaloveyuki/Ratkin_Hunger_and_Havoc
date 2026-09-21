using System.Collections.Generic;

namespace HungerAndHavoc.Api
{
    public sealed class HungerPawnSeed
    {
        public HungerPawnSeed(
            string sourceIncidentDisplayId = null,
            int spawnBatchId = 0,
            int relationshipGroupId = 0,
            HungerPawnRole role = HungerPawnRole.Unspecified,
            HungerLifecycle lifecycle = HungerLifecycle.Arriving,
            bool carriesPlague = false,
            HungerAttitude attitudeAtArrival = HungerAttitude.Neutral,
            int leaveAfterGameTick = -1,
            int parentPawnLoadId = 0,
            IEnumerable<int> childPawnLoadIds = null,
            IEnumerable<KeyValuePair<HungerBehaviorGate, bool>> gateOverrides = null)
        {
            SourceIncidentDisplayId = sourceIncidentDisplayId;
            SpawnBatchId = spawnBatchId;
            RelationshipGroupId = relationshipGroupId;
            Role = role;
            Lifecycle = lifecycle;
            CarriesPlague = carriesPlague;
            AttitudeAtArrival = attitudeAtArrival;
            LeaveAfterGameTick = leaveAfterGameTick;
            ParentPawnLoadId = parentPawnLoadId;
            ChildPawnLoadIds = CopyChildren(childPawnLoadIds);
            GateOverrides = CopyGates(gateOverrides);
        }

        public string SourceIncidentDisplayId { get; }

        public int SpawnBatchId { get; }

        public int RelationshipGroupId { get; }

        public HungerPawnRole Role { get; }

        public HungerLifecycle Lifecycle { get; }

        public bool CarriesPlague { get; }

        public HungerAttitude AttitudeAtArrival { get; }

        public int LeaveAfterGameTick { get; }

        public int ParentPawnLoadId { get; }

        public IReadOnlyList<int> ChildPawnLoadIds { get; }

        public IReadOnlyDictionary<HungerBehaviorGate, bool> GateOverrides { get; }

        public HungerPawnSeed Clone()
        {
            return new HungerPawnSeed(
                SourceIncidentDisplayId,
                SpawnBatchId,
                RelationshipGroupId,
                Role,
                Lifecycle,
                CarriesPlague,
                AttitudeAtArrival,
                LeaveAfterGameTick,
                ParentPawnLoadId,
                ChildPawnLoadIds,
                GateOverrides);
        }

        static IReadOnlyList<int> CopyChildren(IEnumerable<int> source)
        {
            if (source == null)
            {
                return new List<int>();
            }

            return new List<int>(source);
        }

        static IReadOnlyDictionary<HungerBehaviorGate, bool> CopyGates(
            IEnumerable<KeyValuePair<HungerBehaviorGate, bool>> source)
        {
            Dictionary<HungerBehaviorGate, bool> copy = new Dictionary<HungerBehaviorGate, bool>();
            if (source == null)
            {
                return copy;
            }

            foreach (KeyValuePair<HungerBehaviorGate, bool> pair in source)
            {
                copy[pair.Key] = pair.Value;
            }

            return copy;
        }
    }
}
