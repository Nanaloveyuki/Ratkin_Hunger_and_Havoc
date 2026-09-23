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
            if (!already && snapshot.Lifecycle == RHAH_Lifecycle.SeekingFood)
            {
                RHAH_Api.SetLifecycle(pawn, RHAH_Lifecycle.Fed);
            }

            if (!already)
            {
                TryAddRefeeding(pawn);
            }

            return RHAH_Api.Get(pawn) != null && RHAH_Api.Get(pawn).HasBeenFed;
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
