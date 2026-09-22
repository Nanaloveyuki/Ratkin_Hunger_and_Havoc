using HungerAndHavoc.Api;

namespace HungerAndHavoc.Identity
{
    internal static class HungerPawnDefaults
    {
        public static bool Allows(IHungerPawn pawn, HungerBehaviorGate gate)
        {
            if (pawn == null)
            {
                return AllowsInactive(gate);
            }

            return Allows(pawn.Role, pawn.Lifecycle, pawn.AttitudeAtArrival, gate);
        }

        public static bool Allows(
            HungerPawnRole role,
            HungerLifecycle lifecycle,
            HungerAttitude attitude,
            HungerBehaviorGate gate)
        {
            if (lifecycle == HungerLifecycle.Released || lifecycle == HungerLifecycle.Dead)
            {
                return AllowsInactive(gate);
            }

            switch (gate)
            {
                case HungerBehaviorGate.Beg:
                    return IsBeggar(role);
                case HungerBehaviorGate.Steal:
                    return IsThief(role);
                case HungerBehaviorGate.Fight:
                    return role == HungerPawnRole.Siege ||
                           attitude == HungerAttitude.Hostile;
                case HungerBehaviorGate.LeaveAfterFed:
                    return true;
                case HungerBehaviorGate.EatOutsideRelief:
                    return attitude != HungerAttitude.Friendly;
                case HungerBehaviorGate.FeedFromRelief:
                    return true;
                case HungerBehaviorGate.Gnaw:
                    return true;
                case HungerBehaviorGate.TailBite:
                    return false;
                case HungerBehaviorGate.Leash:
                    return IsChild(role);
                case HungerBehaviorGate.Carry:
                    return IsChild(role);
                case HungerBehaviorGate.JoinColony:
                    return true;
                case HungerBehaviorGate.Hire:
                    return true;
                case HungerBehaviorGate.Transfer:
                    return true;
                case HungerBehaviorGate.Imprison:
                    return true;
                case HungerBehaviorGate.DropOffChild:
                    return role == HungerPawnRole.Mother ||
                           role == HungerPawnRole.BeggarMother;
                case HungerBehaviorGate.ExitMap:
                    return true;
                default:
                    return false;
            }
        }

        static bool AllowsInactive(HungerBehaviorGate gate)
        {
            return gate == HungerBehaviorGate.JoinColony ||
                   gate == HungerBehaviorGate.Hire ||
                   gate == HungerBehaviorGate.Transfer ||
                   gate == HungerBehaviorGate.Imprison ||
                   gate == HungerBehaviorGate.Leash ||
                   gate == HungerBehaviorGate.Carry;
        }

        static bool IsBeggar(HungerPawnRole role)
        {
            return role == HungerPawnRole.Beggar ||
                   role == HungerPawnRole.BeggarMother ||
                   role == HungerPawnRole.BeggarChild ||
                   role == HungerPawnRole.Refugee ||
                   role == HungerPawnRole.Labor;
        }

        static bool IsThief(HungerPawnRole role)
        {
            return role == HungerPawnRole.Thief || role == HungerPawnRole.ThiefChild;
        }

        static bool IsChild(HungerPawnRole role)
        {
            return role == HungerPawnRole.RatkinYoung ||
                   role == HungerPawnRole.BeggarChild ||
                   role == HungerPawnRole.ThiefChild ||
                   role == HungerPawnRole.WildChild;
        }
    }
}
