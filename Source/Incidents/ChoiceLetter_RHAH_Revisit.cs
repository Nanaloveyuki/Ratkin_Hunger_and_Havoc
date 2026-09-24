using System.Collections.Generic;
using HungerAndHavoc.Narrative;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_Revisit : ChoiceLetter
    {
        public int caseId;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                SuiyinN004Case record = FindRecord();
                if (ArchivedOnly || record == null || record.Revisit != SuiyinN004Revisit.None)
                {
                    yield return Option_Close;
                    yield break;
                }

                int cost = RHAH_Revisit.CostFor(record);
                yield return Act("RHAH_Revisit_Leave", Leave);
                DiaOption pay = new DiaOption("RHAH_Revisit_Pay".Translate(cost));
                if (!RHAH_Revisit.CanPay(record))
                {
                    pay.Disable("RHAH_Revisit_NoSilver".Translate(cost));
                }
                else
                {
                    pay.action = () => Pay();
                    pay.resolveTree = true;
                }

                yield return pay;
                yield return Act("RHAH_Revisit_Kill", Kill);
                if (lookTargets.IsValid())
                {
                    yield return Option_JumpToLocationAndPostpone;
                }

                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref caseId, "caseId", 0);
        }

        void Leave()
        {
            Choose(SuiyinN004Revisit.Ignore);
        }

        void Pay()
        {
            SuiyinN004Case record = FindRecord();
            if (record == null || !RHAH_Revisit.CanPay(record))
            {
                Messages.Message("RHAH_Revisit_NoSilver".Translate(RHAH_Revisit.CostFor(record)), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!Choose(SuiyinN004Revisit.Rescue))
            {
                return;
            }

            RHAH_Revisit.Spend(record);
        }

        void Kill()
        {
            SuiyinN004Case record = FindRecord();
            if (!Choose(SuiyinN004Revisit.Kill) || record == null)
            {
                return;
            }

            RHAH_Revisit.Kill(record);
        }

        bool Choose(SuiyinN004Revisit choice)
        {
            NarrativeState state = Current.Game?.GetComponent<NarrativeState>();
            SuiyinN004Case record = FindRecord();
            if (state == null || record == null || record.Revisit != SuiyinN004Revisit.None)
            {
                return false;
            }

            if (!state.Commit(book => book.ChooseRevisit(record, choice, Now(), choice != SuiyinN004Revisit.Rescue || RHAH_Revisit.CanPay(record))))
            {
                return false;
            }

            if (Find.LetterStack != null)
            {
                Find.LetterStack.RemoveLetter(this);
            }

            return true;
        }

        static int Now()
        {
            return Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
        }

        static DiaOption Act(string key, System.Action action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.action = action;
            option.resolveTree = true;
            return option;
        }

        SuiyinN004Case FindRecord()
        {
            return RHAH_Revisit.FindCase(caseId);
        }

    }
}
