using System.Collections.Generic;
using HungerAndHavoc.Narrative;
using HungerAndHavoc.Storyteller.Suiyin;
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
                    pay.action = () => Wrapped(pay, Pay);
                    pay.resolveTree = false;
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
            if (!Choose(SuiyinN004Revisit.Ignore))
            {
                Messages.Message("RHAH_Revisit_Stale".Translate(), MessageTypeDefOf.RejectInput);
            }
        }

        void Pay()
        {
            SuiyinN004Case record = FindRecord();
            if (record == null || record.Revisit != SuiyinN004Revisit.None)
            {
                Messages.Message("RHAH_Revisit_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!RHAH_Revisit.CanPay(record))
            {
                Messages.Message("RHAH_Revisit_NoSilver".Translate(RHAH_Revisit.CostFor(record)), MessageTypeDefOf.RejectInput);
                return;
            }

            if (!Choose(SuiyinN004Revisit.Rescue))
            {
                Messages.Message("RHAH_Revisit_Stale".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            RHAH_Revisit.Spend(record);
        }

        void Kill()
        {
            SuiyinN004Case record = FindRecord();
            if (!Choose(SuiyinN004Revisit.Kill) || record == null)
            {
                Messages.Message("RHAH_Revisit_Stale".Translate(), MessageTypeDefOf.RejectInput);
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

        DiaOption Act(string key, System.Action action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.resolveTree = false;
            option.action = () => Wrapped(option, action);
            return option;
        }

        // 仅当信件真的离开信栈才关闭它自己的窗口
        void Wrapped(DiaOption option, System.Action action)
        {
            bool open = Find.LetterStack != null && Find.LetterStack.LettersListForReading.Contains(this);
            action();
            if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
            {
                option.dialog?.Close();
            }
        }

        SuiyinN004Case FindRecord()
        {
            return RHAH_Revisit.FindCase(caseId);
        }

    }
}
