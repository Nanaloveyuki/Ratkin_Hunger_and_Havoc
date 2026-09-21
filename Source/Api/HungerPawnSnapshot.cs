using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Api
{
    // 只读拷贝 后续 Comp 变化不回写
    public sealed class HungerPawnSnapshot : IHungerPawn
    {
        public HungerPawnSnapshot(
            string sourceIncidentDisplayId,
            int spawnBatchId,
            int relationshipGroupId,
            HungerPawnRole role,
            HungerLifecycle lifecycle,
            bool hasBeenFed,
            int leaveAfterGameTick,
            bool carriesPlague,
            HungerAttitude attitudeAtArrival,
            int parentPawnLoadId,
            IEnumerable<int> childPawnLoadIds)
        {
            SourceIncidentDisplayId = sourceIncidentDisplayId;
            SpawnBatchId = spawnBatchId;
            RelationshipGroupId = relationshipGroupId;
            Role = role;
            Lifecycle = lifecycle;
            HasBeenFed = hasBeenFed;
            LeaveAfterGameTick = leaveAfterGameTick;
            CarriesPlague = carriesPlague;
            AttitudeAtArrival = attitudeAtArrival;
            ParentPawnLoadId = parentPawnLoadId;
            ChildPawnLoadIds = CopyChildren(childPawnLoadIds);
        }

        public HungerPawnSnapshot(IHungerPawn source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            SourceIncidentDisplayId = source.SourceIncidentDisplayId;
            SpawnBatchId = source.SpawnBatchId;
            RelationshipGroupId = source.RelationshipGroupId;
            Role = source.Role;
            Lifecycle = source.Lifecycle;
            HasBeenFed = source.HasBeenFed;
            LeaveAfterGameTick = source.LeaveAfterGameTick;
            CarriesPlague = source.CarriesPlague;
            AttitudeAtArrival = source.AttitudeAtArrival;
            ParentPawnLoadId = source.ParentPawnLoadId;
            ChildPawnLoadIds = CopyChildren(source.ChildPawnLoadIds);
        }

        public string SourceIncidentDisplayId { get; }

        public int SpawnBatchId { get; }

        public int RelationshipGroupId { get; }

        public HungerPawnRole Role { get; }

        public HungerLifecycle Lifecycle { get; }

        public bool HasBeenFed { get; }

        public int LeaveAfterGameTick { get; }

        public bool CarriesPlague { get; }

        public HungerAttitude AttitudeAtArrival { get; }

        public int ParentPawnLoadId { get; }

        public IReadOnlyList<int> ChildPawnLoadIds { get; }

        public bool IsReleased => Lifecycle == HungerLifecycle.Released;

        public bool IsActiveVisitor => Lifecycle != HungerLifecycle.Released && Lifecycle != HungerLifecycle.Dead;

        static IReadOnlyList<int> CopyChildren(IEnumerable<int> source)
        {
            if (source == null)
            {
                return new List<int>();
            }

            return new List<int>(source);
        }
    }
}
