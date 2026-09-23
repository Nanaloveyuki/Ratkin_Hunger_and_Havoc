using HungerAndHavoc.Caravan;
using HungerAndHavoc.Incidents;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Core
{
    internal static class RHAH_Scheduler
    {
        internal static bool ExecuteDebugIncident(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            RHAH_Settings settings = RHAH_Mod.Settings;
            if (!RHAH_Runtime.AllowsNewContent || entry == null || Current.Game == null ||
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
