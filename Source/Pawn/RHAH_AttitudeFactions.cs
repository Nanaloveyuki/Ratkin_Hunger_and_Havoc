using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_AttitudeFactions
    {
        internal static Faction Resolve(RHAH_Attitude attitude)
        {
            FactionDef def = DefFor(attitude);
            if (def == null || Find.FactionManager == null)
            {
                return null;
            }

            return Find.FactionManager.FirstFactionOfDef(def);
        }

        internal static Faction Require(RHAH_Attitude attitude)
        {
            Faction faction = Resolve(attitude);
            if (faction == null || faction.IsPlayer)
            {
                Ensure(attitude);
                faction = Resolve(attitude);
            }

            return faction != null && !faction.IsPlayer ? faction : null;
        }

        internal static void Ensure(RHAH_Attitude attitude)
        {
            if (Resolve(attitude) != null || Find.FactionManager == null)
            {
                return;
            }

            FactionDef def = DefFor(attitude);
            if (def == null)
            {
                return;
            }

            FactionGenerator.CreateFactionAndAddToManager(def);
        }

        internal static void EnsureAll()
        {
            if (Find.FactionManager == null)
            {
                return;
            }

            Ensure(RHAH_Attitude.Hostile);
            Ensure(RHAH_Attitude.LeaningHostile);
            Ensure(RHAH_Attitude.Neutral);
            Ensure(RHAH_Attitude.LeaningFriendly);
            Ensure(RHAH_Attitude.Friendly);
        }

        internal static FactionDef DefFor(RHAH_Attitude attitude)
        {
            switch (attitude)
            {
                case RHAH_Attitude.Hostile:
                    return HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Hostile;
                case RHAH_Attitude.LeaningHostile:
                    return HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_LeaningHostile;
                case RHAH_Attitude.LeaningFriendly:
                    return HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_LeaningFriendly;
                case RHAH_Attitude.Friendly:
                    return HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Friendly;
                default:
                    return HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Neutral;
            }
        }

        internal static bool IsAttitudeFaction(Faction faction)
        {
            if (faction?.def == null)
            {
                return false;
            }

            FactionDef def = faction.def;
            return def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Hostile ||
                   def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_LeaningHostile ||
                   def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Neutral ||
                   def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_LeaningFriendly ||
                   def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Friendly;
        }

        internal static void LockGoodwill()
        {
            EnsureAll();
            Faction player = Faction.OfPlayer;
            if (player == null || Find.FactionManager == null)
            {
                return;
            }

            RHAH_Attitude[] attitudes =
            {
                RHAH_Attitude.Hostile,
                RHAH_Attitude.LeaningHostile,
                RHAH_Attitude.Neutral,
                RHAH_Attitude.LeaningFriendly,
                RHAH_Attitude.Friendly
            };
            for (int i = 0; i < attitudes.Length; i++)
            {
                Pin(player, Resolve(attitudes[i]), RHAH_VisitorRules.LockedGoodwill(attitudes[i]));
            }
        }

        static void Pin(Faction player, Faction faction, int goodwill)
        {
            if (faction == null || faction == player)
            {
                return;
            }

            FactionRelation relation = player.RelationWith(faction, true);
            if (relation == null)
            {
                player.TryMakeInitialRelationsWith(faction);
                relation = player.RelationWith(faction, true);
            }

            if (relation == null || relation.baseGoodwill == goodwill)
            {
                return;
            }

            player.TryAffectGoodwillWith(faction, goodwill - relation.baseGoodwill, false, false, null, null);
        }
    }
}
