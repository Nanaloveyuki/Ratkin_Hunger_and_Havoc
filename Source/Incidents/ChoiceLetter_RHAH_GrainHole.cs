using System.Collections.Generic;
using HungerAndHavoc.Core;
using HungerAndHavoc.Narrative;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    public class ChoiceLetter_RHAH_GrainHole : ChoiceLetter
    {
        public int mapId;
        public bool followUp;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                SuiyinN006Case record = FindRecord();
                if (ArchivedOnly || record == null || !Open(record))
                {
                    yield return Option_Close;
                    yield break;
                }

                if (followUp)
                {
                    yield return Act("RHAH_Hole_Trace", Trace);
                    yield return Act("RHAH_Hole_Stop", Stop);
                }
                else
                {
                    yield return Act("RHAH_Hole_Seal", Seal);
                    yield return Act("RHAH_Hole_Bait", Bait);
                    yield return Act("RHAH_Hole_Clean", Clean);
                    yield return Act("RHAH_Hole_Ignore", Ignore);
                }

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
            Scribe_Values.Look(ref mapId, "mapId", 0);
            Scribe_Values.Look(ref followUp, "followUp", false);
        }

        void Seal()
        {
            Map map = ResolveMap();
            SuiyinBook book = Book();
            SuiyinN006Case record = FindRecord();
            if (map == null || book == null || record == null || record.Outcome != SuiyinN006Outcome.Pending)
            {
                Stale();
                return;
            }

            if (RHAH_GrainHole.CountWood(map) < RHAH_GrainHole.WoodCost)
            {
                Messages.Message("RHAH_Hole_NoWood".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            record.Wood = RHAH_GrainHole.WoodCost;
            if (!book.ChooseHole(record, SuiyinN006Action.Seal, Now()))
            {
                record.Wood = 0;
                Stale();
                return;
            }

            if (!RHAH_GrainHole.SpendWood(map, RHAH_GrainHole.WoodCost))
            {
                Messages.Message("RHAH_Hole_NoWood".Translate(), MessageTypeDefOf.RejectInput);
            }

            RHAH_GrainHole.RemoveHole(map, map.GetComponent<MapComponent_RHAH_Map>());
            Close();
        }

        void Bait()
        {
            Map map = ResolveMap();
            SuiyinBook book = Book();
            SuiyinN006Case record = FindRecord();
            Thing hole = Hole(map);
            if (map == null || book == null || record == null || hole == null || record.Outcome != SuiyinN006Outcome.Pending)
            {
                Stale();
                return;
            }

            if (!RHAH_GrainHole.TakeFood(map, hole.Position, 1, RHAH_GrainHole.LossRange))
            {
                Messages.Message("RHAH_Hole_NoFood".Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            record.BaitStock = true;
            if (!book.ChooseHole(record, SuiyinN006Action.Bait, Now()))
            {
                record.BaitStock = false;
                Stale();
                return;
            }

            Close();
        }

        void Clean()
        {
            Map map = ResolveMap();
            SuiyinBook book = Book();
            SuiyinN006Case record = FindRecord();
            Thing hole = Hole(map);
            if (map == null || book == null || record == null || record.Outcome != SuiyinN006Outcome.Pending)
            {
                Stale();
                return;
            }

            if (hole != null)
            {
                RHAH_GrainHole.TakeFood(map, hole.Position, RHAH_GrainHole.CleanPortions, RHAH_GrainHole.LossRange);
            }

            if (!book.ChooseHole(record, SuiyinN006Action.Clean, Now()))
            {
                Stale();
                return;
            }

            RHAH_GrainHole.RemoveHole(map, map.GetComponent<MapComponent_RHAH_Map>());
            Close();
        }

        void Ignore()
        {
            Close();
        }

        void Trace()
        {
            Finish(true);
        }

        void Stop()
        {
            Finish(false);
        }

        void Finish(bool trace)
        {
            Map map = ResolveMap();
            SuiyinBook book = Book();
            SuiyinN006Case record = FindRecord();
            if (map == null || book == null || record == null || record.Outcome != SuiyinN006Outcome.BaitSet)
            {
                Stale();
                return;
            }

            bool followed = trace && RHAH_ChoiceRuntime.TryCreateSite(map, RHAH_IntelSiteKind.Treasure);
            if (!book.FinishBait(record, Now(), followed))
            {
                Stale();
                return;
            }

            RHAH_GrainHole.RemoveHole(map, map.GetComponent<MapComponent_RHAH_Map>());
            Close();
        }

        void Close()
        {
            if (Find.LetterStack != null)
            {
                Find.LetterStack.RemoveLetter(this);
            }
        }

        static void Stale()
        {
            Messages.Message("RHAH_Hole_Stale".Translate(), MessageTypeDefOf.RejectInput);
        }

        static int Now()
        {
            return Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
        }

        DiaOption Act(string key, System.Action action)
        {
            DiaOption option = new DiaOption(key.Translate());
            option.resolveTree = false;
            option.action = () =>
            {
                bool open = Find.LetterStack != null && Find.LetterStack.LettersListForReading.Contains(this);
                action();
                if (open && (Find.LetterStack == null || !Find.LetterStack.LettersListForReading.Contains(this)))
                {
                    Find.WindowStack.TryRemove(typeof(Dialog_NodeTree), true);
                }
            };
            return option;
        }

        bool Open(SuiyinN006Case record)
        {
            if (followUp)
            {
                return record.Outcome == SuiyinN006Outcome.BaitSet;
            }

            return record.Outcome == SuiyinN006Outcome.Pending && record.Hole;
        }

        SuiyinN006Case FindRecord()
        {
            SuiyinBook book = Book();
            if (book?.N006 == null)
            {
                return null;
            }

            for (int i = 0; i < book.N006.Count; i++)
            {
                SuiyinN006Case record = book.N006[i];
                if (record != null && record.MapId == mapId)
                {
                    return record;
                }
            }

            return null;
        }

        static SuiyinBook Book()
        {
            return Current.Game?.GetComponent<NarrativeState>()?.Book;
        }

        Map ResolveMap()
        {
            if (Find.Maps == null)
            {
                return null;
            }

            for (int i = 0; i < Find.Maps.Count; i++)
            {
                if (Find.Maps[i] != null && Find.Maps[i].uniqueID == mapId)
                {
                    return Find.Maps[i];
                }
            }

            return null;
        }

        Thing Hole(Map map)
        {
            if (map == null)
            {
                return null;
            }

            MapComponent_RHAH_Map component = map.GetComponent<MapComponent_RHAH_Map>();
            if (component == null || component.HoleThingId <= 0)
            {
                return null;
            }

            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(RHAH_GrainHole.HoleDefName);
            if (def == null)
            {
                return null;
            }

            List<Thing> holes = map.listerThings.ThingsOfDef(def);
            for (int i = 0; i < holes.Count; i++)
            {
                Thing hole = holes[i];
                if (hole != null && hole.thingIDNumber == component.HoleThingId && hole.Spawned)
                {
                    return hole;
                }
            }

            return null;
        }
    }
}
