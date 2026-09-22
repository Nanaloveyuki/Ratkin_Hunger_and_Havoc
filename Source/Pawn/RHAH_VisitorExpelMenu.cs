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

        protected override FloatMenuOption GetSingleOptionFor(Verse.Pawn clickedPawn, FloatMenuContext context)
        {
            if (!HungerAndHavocApi.IsVisitor(clickedPawn))
            {
                return null;
            }

            Verse.Pawn actor = context.FirstSelectedPawn;
            if (actor == null || actor.Faction != Faction.OfPlayer || actor.Dead)
            {
                return null;
            }

            return new FloatMenuOption(
                "RHAH_Menu_Expel".Translate(clickedPawn.LabelShort),
                () => RHAH_BatchAttitude.TryShift(clickedPawn, true),
                MenuOptionPriority.Default,
                null,
                clickedPawn);
        }
    }
}
