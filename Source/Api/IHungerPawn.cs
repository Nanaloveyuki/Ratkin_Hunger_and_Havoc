using System.Collections.Generic;

namespace HungerAndHavoc.Api
{
    public interface IHungerPawn
    {
        string SourceIncidentDisplayId { get; }

        int SpawnBatchId { get; }

        int RelationshipGroupId { get; }

        HungerPawnRole Role { get; }

        HungerLifecycle Lifecycle { get; }

        bool HasBeenFed { get; }

        int LeaveAfterGameTick { get; }

        bool CarriesPlague { get; }

        HungerAttitude AttitudeAtArrival { get; }

        int ParentPawnLoadId { get; }

        IReadOnlyList<int> ChildPawnLoadIds { get; }

        bool IsReleased { get; }

        bool IsActiveVisitor { get; }
    }
}
