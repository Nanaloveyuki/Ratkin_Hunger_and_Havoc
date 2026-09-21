using System;
using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Identity
{
    public class HungerRaceExtension : DefModExtension
    {
        public bool isRatkin;
    }

    internal static class HungerRace
    {
        const string RatkinDefName = "Ratkin";

        static readonly List<Func<ThingDef, bool>> matchers = new List<Func<ThingDef, bool>>();

        static HungerRace()
        {
            Register(DefaultMatch);
        }

        internal static void Register(Func<ThingDef, bool> matcher)
        {
            if (matcher == null || matchers.Contains(matcher))
            {
                return;
            }

            matchers.Add(matcher);
        }

        internal static bool IsRatkin(ThingDef def)
        {
            if (def?.race == null)
            {
                return false;
            }

            for (int i = matchers.Count - 1; i >= 0; i--)
            {
                if (matchers[i](def))
                {
                    return true;
                }
            }

            return false;
        }

        static bool DefaultMatch(ThingDef def)
        {
            if (string.Equals(def.defName, RatkinDefName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return def.GetModExtension<HungerRaceExtension>()?.isRatkin == true;
        }
    }
}
