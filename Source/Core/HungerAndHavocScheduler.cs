using HungerAndHavoc.Incidents;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class HungerAndHavocScheduler
    {
        internal static bool QueueDebugIncident(string displayId)
        {
            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(displayId);
            if (!HungerAndHavocRuntime.AllowsNewContent || entry == null || Current.Game == null)
            {
                return false;
            }

            return Current.Game.GetComponent<GameComponent_HungerAndHavoc>()?.QueueIncident(entry.DisplayId) == true;
        }

        internal static bool IsEligible(string displayId, Map map)
        {
            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(displayId);
            return HungerAndHavocRuntime.AllowsNewContent && entry != null &&
                entry.Target == HungerIncidentTarget.Map && map != null;
        }
    }
}
