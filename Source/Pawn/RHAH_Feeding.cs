using HungerAndHavoc.Api;
using HungerAndHavoc.Core;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    internal static class RHAH_Feeding
    {
        internal static bool TryComplete(Verse.Pawn pawn)
        {
            if (!RHAH_Api.IsVisitor(pawn) || pawn.needs == null || pawn.needs.food == null)
            {
                return false;
            }

            float level = pawn.needs.food.CurLevelPercentage;
            if (float.IsNaN(level) || float.IsInfinity(level) || level < RHAH_ReliefFood.SatisfiedLevel)
            {
                return false;
            }

            IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
            if (snapshot == null)
            {
                return false;
            }

            bool already = snapshot.HasBeenFed;
            if (already)
            {
                return true;
            }

            RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Fed);
            ScheduleStay(pawn);
            TryAddRefeeding(pawn);

            return RHAH_Api.Get(pawn) != null && RHAH_Api.Get(pawn).HasBeenFed;
        }
        static void ScheduleStay(Verse.Pawn pawn)
        {
            RHAH_Settings settings = RHAH_Mod.Settings;
            Identity.CompRHAH_Pawn comp = Identity.CompRHAH_Pawn.TryGet(pawn);
            if (comp == null)
            {
                return;
            }

            if (settings != null && !settings.leaveAfterFed)
            {
                return;
            }

            bool wander = settings == null || settings.fedWanderEnabled;
            int hours = settings == null ? RHAH_VisitorRules.DefaultFedWanderHours : settings.fedWanderHours;
            int now = Find.TickManager != null ? Find.TickManager.TicksGame : 0;
            comp.SetLeaveAfter(now + RHAH_VisitorRules.FedWanderTicks(wander, hours));
        }

        static void TryAddRefeeding(Verse.Pawn pawn)
        {
            if (pawn.health == null || pawn.health.hediffSet == null)
            {
                return;
            }

            HediffDef syndrome = RHAH_DefOf.RHAH_RefeedingSyndrome;
            if (syndrome == null || pawn.health.hediffSet.HasHediff(syndrome))
            {
                return;
            }

            Hediff malnutrition = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
            if (malnutrition == null || malnutrition.Severity < RHAH_ReliefFood.RefeedMalnutrition)
            {
                return;
            }

            pawn.health.AddHediff(syndrome);
        }
    }
}
