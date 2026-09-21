using System.Collections.Generic;
using HungerAndHavoc.Api;
using RimWorld;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobDriver_RHAH_Gnaw : JobDriver
    {
        const int GnawTicks = 180;
        const float TokenNutrition = 0.2f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, GnawTicks, true);
            Toil finish = new Toil();
            finish.initAction = delegate
            {
                Need_Food food = pawn.needs != null ? pawn.needs.food : null;
                if (food != null)
                {
                    food.CurLevel += TokenNutrition;
                }

                HungerAndHavocApi.SetLifecycle(pawn, HungerLifecycle.Fed);
            };
            finish.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finish;
        }
    }
}
