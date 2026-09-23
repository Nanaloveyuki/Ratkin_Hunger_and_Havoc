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
            if (!RHAH_Api.IsVisitor(clickedPawn))
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
            if (RHAH_Api.Allows(clickedPawn, RHAH_BehaviorGate.JoinColony))
            {
                yield return new FloatMenuOption("RHAH_Choice_Join".Translate(), () =>
                {
                    RHAH_Api.ReleaseToColony(clickedPawn, RHAH_ReleaseReason.JoinedPlayerFaction);
                    clickedPawn.SetFaction(Faction.OfPlayer);
                });
            }

            if (RHAH_Api.Allows(clickedPawn, RHAH_BehaviorGate.Hire))
            {
                yield return new FloatMenuOption("RHAH_Choice_Hire".Translate(), () =>
                    RHAH_VisitorStay.Begin(clickedPawn, RHAH_StayKind.Hire));
            }
            if (RHAH_Api.Allows(clickedPawn, RHAH_BehaviorGate.JoinColony))
            {
                yield return new FloatMenuOption("RHAH_Choice_Shelter".Translate(), () =>
                    RHAH_VisitorStay.Begin(clickedPawn, RHAH_StayKind.Shelter));
            }

            if (RHAH_Api.Allows(clickedPawn, RHAH_BehaviorGate.FeedFromRelief))
            {
                yield return new FloatMenuOption("RHAH_Choice_Feed".Translate(), () =>
                    RHAH_Api.SetLifecycle(clickedPawn, RHAH_Lifecycle.Fed));
            }
        }
    }
}
