using System.Collections.Generic;
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

            if (faction != null && !faction.IsPlayer)
            {
                LockOutside(Faction.OfPlayer);
                return faction;
            }

            return null;
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

        internal static void LockOutside(Faction player)
        {
            List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
            for (int i = 0; i < factions.Count; i++)
            {
                Faction owner = factions[i];
                if (!IsAttitudeFaction(owner))
                {
                    continue;
                }

                bool ownerHostile = owner.def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Hostile;
                bool ownerFriendly = owner.def == HungerAndHavoc.Core.RHAH_DefOf.RHAH_Faction_Friendly;
                for (int j = 0; j < factions.Count; j++)
                {
                    Faction other = factions[j];
                    if (other == null || other == owner || other == player || other.IsPlayer)
                    {
                        continue;
                    }

                    bool playerHostile = player != null && player.HostileTo(other);
                    RHAH_VisitorRules.OutsideRelation(
                        ownerHostile,
                        ownerFriendly,
                        IsAttitudeFaction(other),
                        playerHostile,
                        out FactionRelationKind kind,
                        out int goodwill);
                    PinPair(owner, other, kind, goodwill);
                }
            }
        }

        static void PinPair(Faction owner, Faction other, FactionRelationKind kind, int goodwill)
        {
            FactionRelation forward = owner.RelationWith(other, true);
            FactionRelation backward = other.RelationWith(owner, true);
            if (forward != null && backward != null &&
                forward.kind == kind && backward.kind == kind &&
                forward.baseGoodwill == goodwill && backward.baseGoodwill == goodwill)
            {
                return;
            }

            owner.SetRelation(new FactionRelation
            {
                other = other,
                kind = kind,
                baseGoodwill = goodwill
            });
            FactionRelation reverse = other.RelationWith(owner, true);
            if (reverse != null)
            {
                reverse.baseGoodwill = goodwill;
            }
        }

        static void Pin(Faction player, Faction faction, int goodwill)
        {
            if (faction == null || faction == player)
            {
                return;
            }

            // 隐藏派系不走原版好感变动 直接固定双向关系
            PinPair(player, faction,
                goodwill < 0 ? FactionRelationKind.Hostile : FactionRelationKind.Neutral,
                goodwill);
        }
    }
}
