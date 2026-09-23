using HungerAndHavoc.Caravan;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class RHAH_Scheduler
    {
        internal static bool QueueDebugIncident(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            RHAH_Settings settings = RHAH_Mod.Settings;
            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            if (!CanQueueDebug(entry, settings, game != null, ResolveTarget(entry) != null))
            {
                return false;
            }

            float catalog = entry.DebugPoints;
            float points = settings == null
                ? catalog
                : settings.IncidentDebugPoints(entry.DisplayId, catalog);
            return game.QueueIncident(entry.DisplayId, points);
        }

        internal static bool CanQueueDebug(
            RHAH_IncidentEntry entry,
            RHAH_Settings settings,
            bool gameLoaded,
            bool targetReady)
        {
            return RHAH_Runtime.AllowsNewContent && entry != null && gameLoaded && targetReady &&
                (settings == null || settings.IsIncidentEnabled(entry.DisplayId));
        }

        static IIncidentTarget ResolveTarget(RHAH_IncidentEntry entry)
        {
            if (entry.Target == RHAH_IncidentTarget.Caravan)
            {
                return CaravanTargetResolver.ResolvePlayerCaravan();
            }

            return RHAH_MapResolver.Resolve();
        }

        internal static bool IsEligible(string displayId, Map map)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            return RHAH_Runtime.AllowsNewContent && entry != null &&
                entry.Target == RHAH_IncidentTarget.Map && map != null;
        }
    }
}
