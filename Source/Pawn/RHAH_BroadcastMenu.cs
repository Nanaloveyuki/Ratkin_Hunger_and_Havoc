using System.Collections.Generic;
using HungerAndHavoc.Core;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    public class RHAH_BroadcastMenu : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;
        protected override bool Undrafted => true;
        protected override bool Multiselect => false;

        protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
        {
            Building_CommsConsole console = clickedThing as Building_CommsConsole;
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (console == null || settings == null || !settings.broadcastEnabled)
            {
                return null;
            }

            Verse.Pawn actor = context.FirstSelectedPawn;
            if (actor == null || actor.Faction != Faction.OfPlayer || actor.Dead)
            {
                return null;
            }

            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            int tick = Find.TickManager.TicksGame;
            if (game == null || !RHAH_BroadcastRules.Ready(tick, game.BroadcastCooldownUntilTick))
            {
                return new FloatMenuOption("RHAH_Broadcast_Cooldown".Translate(), null);
            }

            List<string> candidates = RHAH_BroadcastRules.Candidates(RHAH_IncidentCatalog.All, Disabled(settings));
            if (candidates.Count == 0)
            {
                return new FloatMenuOption("RHAH_Broadcast_Empty".Translate(), null);
            }

            return new FloatMenuOption("RHAH_Broadcast_Label".Translate(), () => Queue(game, candidates, tick, settings.broadcastCooldownDays, console.Map));
        }

        static void Queue(GameComponent_RHAH_Game game, List<string> candidates, int tick, int days, Map map)
        {
            string displayId = RHAH_BroadcastRules.Pick(candidates, tick % candidates.Count);
            int targetId = map == null ? RHAH_IncidentSchedule.UnspecifiedTargetId : RHAH_IncidentSchedule.TargetId(false, map.uniqueID);
            if (displayId != null && game.QueueIncident(displayId, GameComponent_RHAH_Game.PointsFor(displayId), targetId))
            {
                game.BroadcastCooldownUntilTick = RHAH_BroadcastRules.NextCooldown(tick, days);
                if (HungerAndHavoc.Storyteller.Suiyin.RHAH_EndingRuntime.CountsNow())
                {
                    Current.Game?.GetComponent<HungerAndHavoc.Narrative.NarrativeState>()?.NoteBroadcast(tick);
                }

                Messages.Message("RHAH_Broadcast_Queued".Translate(displayId), MessageTypeDefOf.NeutralEvent);
            }
        }

        static HashSet<string> Disabled(RHAH_Settings settings)
        {
            HashSet<string> disabled = new HashSet<string>();
            IReadOnlyList<RHAH_IncidentEntry> entries = RHAH_IncidentCatalog.All;
            for (int i = 0; i < entries.Count; i++)
            {
                if (!settings.IsIncidentEnabled(entries[i].DisplayId))
                {
                    disabled.Add(entries[i].DisplayId);
                }
            }

            return disabled;
        }
    }
}
