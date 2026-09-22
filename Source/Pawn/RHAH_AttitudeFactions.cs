using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_AttitudeFactions
    {
        internal static Faction Resolve(HungerAttitude attitude)
        {
            FactionDef def = DefFor(attitude);
            if (def == null || Find.FactionManager == null)
            {
                return null;
            }

            return Find.FactionManager.FirstFactionOfDef(def);
        }

        internal static FactionDef DefFor(HungerAttitude attitude)
        {
            switch (attitude)
            {
                case HungerAttitude.Hostile:
                    return HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Hostile;
                case HungerAttitude.LeaningHostile:
                    return HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_LeaningHostile;
                case HungerAttitude.LeaningFriendly:
                    return HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_LeaningFriendly;
                case HungerAttitude.Friendly:
                    return HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Friendly;
                default:
                    return HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Neutral;
            }
        }

        internal static bool IsAttitudeFaction(Faction faction)
        {
            if (faction?.def == null)
            {
                return false;
            }

            FactionDef def = faction.def;
            return def == HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Hostile ||
                   def == HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_LeaningHostile ||
                   def == HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Neutral ||
                   def == HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_LeaningFriendly ||
                   def == HungerAndHavoc.Core.HungerAndHavocDefOf.RHAH_Faction_Friendly;
        }
    }
}
