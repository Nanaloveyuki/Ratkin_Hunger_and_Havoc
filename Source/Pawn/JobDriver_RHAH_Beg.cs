using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HungerAndHavoc.Pawn
{
    public class JobDriver_RHAH_Beg : JobDriver
    {
        const int BegTicks = 150;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnDowned(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, BegTicks, true, false, false, TargetIndex.A);
        }
    }
}
