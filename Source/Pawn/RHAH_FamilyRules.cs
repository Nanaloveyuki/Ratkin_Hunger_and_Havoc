using System.Collections.Generic;
using HungerAndHavoc.Api;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_FamilyRules
    {
        internal static bool CanDrop(HungerPawnRole role, bool leaving, bool enabled)
        {
            return enabled &&
                leaving &&
                (role == HungerPawnRole.Mother || role == HungerPawnRole.BeggarMother);
        }

        internal static bool MarkDropped(IList<int> dropped, int childLoadId)
        {
            if (dropped == null || childLoadId <= 0 || dropped.Contains(childLoadId))
            {
                return false;
            }

            dropped.Add(childLoadId);
            return true;
        }

        internal static bool AllDropped(IList<int> children, IList<int> dropped)
        {
            if (children == null || children.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] <= 0)
                {
                    continue;
                }

                if (dropped == null || !dropped.Contains(children[i]))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool CanMotherFeed(HungerPawnRole role, bool enabled, bool childHungry)
        {
            return enabled && childHungry && (role == HungerPawnRole.Mother || role == HungerPawnRole.BeggarMother);
        }

        internal static bool CanScavenge(bool enabled, bool prisoner, bool hungry)
        {
            return enabled && prisoner && hungry;
        }

        internal static bool CanTailBite(bool enabled, bool prisoner, bool hungry, bool targetAsleep, float targetAge)
        {
            return enabled && prisoner && hungry && targetAsleep && targetAge >= 0f && targetAge < 3f;
        }
    }
}
