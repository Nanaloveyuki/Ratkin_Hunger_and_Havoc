using System.Collections.Generic;
using HungerAndHavoc.Api;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Pawn
{
    // 原版扫描公开子类 殖民者右键在场来客时出现
    public class RHAH_VisitorExpelMenu : FloatMenuOptionProvider
    {
        protected override bool Drafted => true;

        protected override bool Undrafted => true;

        protected override bool Multiselect => false;

        public override IEnumerable<FloatMenuOption> GetOptionsFor(Verse.Pawn clickedPawn, FloatMenuContext context)
        {
            if (!HungerAndHavocApi.IsVisitor(clickedPawn))
            {
                yield break;
            }

            Verse.Pawn actor = context.FirstSelectedPawn;
            if (actor == null || actor.Faction != Faction.OfPlayer || actor.Dead)
            {
                yield break;
            }

            yield return new FloatMenuOption(
                "RHAH_Menu_Expel".Translate(clickedPawn.LabelShort),
                () => RHAH_BatchAttitude.TryShift(clickedPawn, true),
                MenuOptionPriority.Default,
                null,
                clickedPawn);
            if (HungerAndHavocApi.Allows(clickedPawn, HungerBehaviorGate.JoinColony))
            {
                yield return new FloatMenuOption("RHAH_Choice_Join".Translate(), () =>
                {
                    HungerAndHavocApi.ReleaseToColony(clickedPawn, HungerReleaseReason.JoinedPlayerFaction);
                    clickedPawn.SetFaction(Faction.OfPlayer);
                });
            }

            if (HungerAndHavocApi.Allows(clickedPawn, HungerBehaviorGate.Hire))
            {
                yield return new FloatMenuOption("RHAH_Choice_Hire".Translate(), () =>
                    HungerAndHavocApi.ReleaseToColony(clickedPawn, HungerReleaseReason.Recruited));
            }

            if (HungerAndHavocApi.Allows(clickedPawn, HungerBehaviorGate.FeedFromRelief))
            {
                yield return new FloatMenuOption("RHAH_Choice_Feed".Translate(), () =>
                    HungerAndHavocApi.SetLifecycle(clickedPawn, HungerLifecycle.Fed));
            }
        }
    }
}
