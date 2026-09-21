namespace HungerAndHavoc.Incidents
{
    internal static class HungerIncidentFacts
    {
        internal static bool Submit(HungerIncidentContext context)
        {
            return context != null && context.Map != null && context.PawnCount > 0;
        }
    }
}
