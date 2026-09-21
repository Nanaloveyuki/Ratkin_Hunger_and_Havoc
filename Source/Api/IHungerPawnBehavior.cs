using Verse;

namespace HungerAndHavoc.Api
{
    public interface IHungerPawnBehavior
    {
        bool? Allows(Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate);

        bool? ShouldReleaseToColony(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason);
    }
}
