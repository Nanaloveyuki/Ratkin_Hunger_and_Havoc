using System;
using System.Collections.Generic;

namespace HungerAndHavoc.Api
{
    public static class HungerPawnBehaviors
    {
        static readonly List<IHungerPawnBehavior> handlers = new List<IHungerPawnBehavior>();

        public static void Register(IHungerPawnBehavior behavior)
        {
            if (behavior == null || handlers.Contains(behavior))
            {
                return;
            }

            handlers.Add(behavior);
        }

        public static void Unregister(IHungerPawnBehavior behavior)
        {
            if (behavior == null)
            {
                return;
            }

            handlers.Remove(behavior);
        }

        internal static bool? Query(Func<IHungerPawnBehavior, bool?> ask)
        {
            if (ask == null)
            {
                return null;
            }

            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                bool? result = ask(handlers[i]);
                if (result.HasValue)
                {
                    return result;
                }
            }

            return null;
        }

        internal static void ResetForTests()
        {
            handlers.Clear();
        }
    }
}
