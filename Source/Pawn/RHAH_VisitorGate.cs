using HungerAndHavoc.Api;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_VisitorGate
    {
        internal static bool IsVisitor(Verse.Pawn pawn)
        {
            return HungerAndHavocApi.IsVisitor(pawn);
        }

        internal static bool Allows(Verse.Pawn pawn, HungerBehaviorGate gate)
        {
            if (!IsVisitor(pawn))
            {
                return false;
            }

            return HungerAndHavocApi.Allows(pawn, gate);
        }
    }
}
