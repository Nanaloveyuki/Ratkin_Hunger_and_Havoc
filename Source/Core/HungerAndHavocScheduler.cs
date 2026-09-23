using HungerAndHavoc.Caravan;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class HungerAndHavocScheduler
    {
        internal static bool ExecuteDebugIncident(string displayId)
        {
            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(displayId);
            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (!HungerAndHavocRuntime.AllowsNewContent || entry == null || Current.Game == null ||
                (settings != null && !settings.IsIncidentEnabled(entry.DisplayId)))
            {
                return false;
            }

            IncidentDef def = DefDatabase<IncidentDef>.GetNamedSilentFail(entry.DefName);
            if (def?.Worker == null)
            {
                return false;
            }

            IIncidentTarget target = ResolveTarget(entry);
            if (target == null)
            {
                return false;
            }

            IncidentParms parms = StorytellerUtility.DefaultParmsNow(def.category, target);
            float catalog = entry.DebugPoints;
            parms.points = settings == null
                ? catalog
                : settings.IncidentDebugPoints(entry.DisplayId, catalog);
            return def.Worker.TryExecute(parms);
        }

        static IIncidentTarget ResolveTarget(HungerIncidentEntry entry)
        {
            if (entry.Target == HungerIncidentTarget.Caravan)
            {
                return CaravanTargetResolver.ResolvePlayerCaravan();
            }

            return HungerMapResolver.Resolve();
        }

        internal static bool IsEligible(string displayId, Map map)
        {
            HungerIncidentEntry entry = HungerIncidentCatalog.GetByDisplayId(displayId);
            return HungerAndHavocRuntime.AllowsNewContent && entry != null &&
                entry.Target == HungerIncidentTarget.Map && map != null;
        }
    }
}
