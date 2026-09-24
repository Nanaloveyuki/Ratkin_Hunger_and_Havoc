using System.Collections.Generic;
using HungerAndHavoc.Incidents;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HungerAndHavoc.Narrative
{
    internal static class RHAH_Revisit
    {
        internal const int CheckInterval = 2500;
        internal const string LetterDefName = "RHAH_RevisitLetter";
        internal const string GloomDefName = "RHAH_Trait_FamineGloom";

        internal static void Tick(int tick)
        {
            if (!RHAH_NarrativePace.Due(tick, CheckInterval, RHAH_NarrativePace.Spread * 3) || Current.Game == null)
            {
                return;
            }

            SuiyinBook book = Current.Game.GetComponent<NarrativeState>()?.Book;
            if (book?.N004 == null)
            {
                return;
            }

            List<RimWorld.Planet.Caravan> caravans = PlayerCaravans();
            for (int i = 0; i < book.N004.Count; i++)
            {
                TryMeet(book.N004[i], caravans, tick);
            }
        }

        internal static bool TryBringHome(Verse.Pawn pawn, Map map)
        {
            if (pawn == null || pawn.Dead || pawn.Destroyed || pawn.Spawned || map == null)
            {
                return false;
            }

            if (!CellFinder.TryFindRandomEdgeCellWith(cell => cell.Standable(map) && !cell.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out IntVec3 edge))
            {
                return false;
            }

            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.Remove(pawn);
            }

            GenSpawn.Spawn(pawn, edge, map);
            return pawn.Spawned;
        }

        internal static int CostFor(SuiyinN004Case record)
        {
            return record == null ? 0 : Cost();
        }

        internal static bool CanPay(SuiyinN004Case record)
        {
            return record != null && CountSilver(MeetingCaravan(record)) >= Cost();
        }

        internal static void Spend(SuiyinN004Case record)
        {
            RimWorld.Planet.Caravan caravan = MeetingCaravan(record);
            int left = Cost();
            if (caravan == null || left <= 0 || ThingDefOf.Silver == null)
            {
                return;
            }

            List<Thing> taken = CaravanInventoryUtility.TakeThings(caravan, thing =>
            {
                if (thing == null || thing.def != ThingDefOf.Silver || left <= 0)
                {
                    return 0;
                }

                int have = thing.stackCount < left ? thing.stackCount : left;
                left -= have;
                return have;
            });
            for (int i = 0; i < taken.Count; i++)
            {
                if (taken[i] != null && !taken[i].Destroyed)
                {
                    taken[i].Destroy();
                }
            }
        }

        internal static void Kill(SuiyinN004Case record)
        {
            Verse.Pawn pawn = Survivor(record);
            if (pawn == null || pawn.Dead || pawn.Destroyed)
            {
                return;
            }

            pawn.Kill(null);
        }

        internal static SuiyinN004Case FindCase(int caseId)
        {
            SuiyinBook book = Current.Game?.GetComponent<NarrativeState>()?.Book;
            if (book?.N004 == null || caseId <= 0)
            {
                return null;
            }

            for (int i = 0; i < book.N004.Count; i++)
            {
                SuiyinN004Case record = book.N004[i];
                if (record != null && record.Id == caseId)
                {
                    return record;
                }
            }

            return null;
        }
        internal static bool BrokeToday(Verse.Pawn pawn, int tick, int until)
        {
            if (pawn == null || until < 0 || tick > until || pawn.MentalStateDef == null)
            {
                return false;
            }

            return pawn.MentalStateDef.defName != "Jailbreaker";
        }

        internal static bool TryGloom(Verse.Pawn pawn)
        {
            if (pawn?.story?.traits == null)
            {
                return false;
            }

            TraitDef def = DefDatabase<TraitDef>.GetNamedSilentFail(GloomDefName);
            if (def == null || HasTrait(pawn, def))
            {
                return false;
            }

            int degree = 0;
            if (def.degreeDatas != null && def.degreeDatas.Count > 0 && def.degreeDatas[0] != null)
            {
                degree = def.degreeDatas[0].degree;
            }

            pawn.story.traits.GainTrait(new Trait(def, degree, false), false);
            return true;
        }

        static void TryMeet(SuiyinN004Case record, List<RimWorld.Planet.Caravan> caravans, int tick)
        {
            if (record == null || record.Outcome != SuiyinN004Outcome.Banished || record.RevisitSeen || record.Revisit != SuiyinN004Revisit.None)
            {
                return;
            }

            if (record.RevisitDeadline >= 0 && tick > record.RevisitDeadline)
            {
                return;
            }

            Verse.Pawn pawn = Survivor(record);
            if (pawn == null)
            {
                return;
            }

            for (int i = 0; i < caravans.Count; i++)
            {
                RimWorld.Planet.Caravan caravan = caravans[i];
                if (caravan == null || caravan.Tile != pawn.Tile)
                {
                    continue;
                }

                record.RevisitSeen = true;
                record.MeetingCaravanId = caravan.ID;
                OpenLetter(record, pawn);
                return;
            }
        }

        static void OpenLetter(SuiyinN004Case record, Verse.Pawn pawn)
        {
            if (Find.LetterStack == null)
            {
                record.RevisitSeen = false;
                record.MeetingCaravanId = 0;
                return;
            }

            LetterDef def = DefDatabase<LetterDef>.GetNamedSilentFail(LetterDefName);
            if (def == null)
            {
                record.RevisitSeen = false;
                record.MeetingCaravanId = 0;
                return;
            }

            ChoiceLetter_RHAH_Revisit letter = (ChoiceLetter_RHAH_Revisit)LetterMaker.MakeLetter(def);
            letter.caseId = record.Id;
            letter.Label = "RHAH_Suiyin_N004Revisit_Label".Translate();
            letter.Text = "RHAH_Suiyin_N004Revisit_Text".Translate();
            letter.lookTargets = new LookTargets(pawn);
            Find.LetterStack.ReceiveLetter(letter);
        }

        static Verse.Pawn Survivor(SuiyinN004Case record)
        {
            Verse.Pawn mother = FindPawn(record.MotherId);
            if (Alive(mother))
            {
                return mother;
            }

            if (record.Children == null)
            {
                return null;
            }

            for (int i = 0; i < record.Children.Count; i++)
            {
                Verse.Pawn child = FindPawn(record.Children[i] == null ? 0 : record.Children[i].LoadId);
                if (Alive(child))
                {
                    return child;
                }
            }

            return null;
        }

        static bool Alive(Verse.Pawn pawn)
        {
            return pawn != null && !pawn.Dead && !pawn.Destroyed;
        }

        static RimWorld.Planet.Caravan MeetingCaravan(SuiyinN004Case record)
        {
            if (record == null || record.MeetingCaravanId <= 0 || Find.WorldObjects == null)
            {
                return null;
            }

            List<RimWorld.Planet.Caravan> caravans = Find.WorldObjects.Caravans;
            for (int i = 0; i < caravans.Count; i++)
            {
                RimWorld.Planet.Caravan caravan = caravans[i];
                if (caravan != null && caravan.ID == record.MeetingCaravanId && caravan.IsPlayerControlled)
                {
                    return caravan;
                }
            }

            return null;
        }

        static List<RimWorld.Planet.Caravan> PlayerCaravans()
        {
            List<RimWorld.Planet.Caravan> found = new List<RimWorld.Planet.Caravan>();
            if (Find.WorldObjects == null)
            {
                return found;
            }

            List<RimWorld.Planet.Caravan> caravans = Find.WorldObjects.Caravans;
            for (int i = 0; i < caravans.Count; i++)
            {
                RimWorld.Planet.Caravan caravan = caravans[i];
                if (caravan != null && caravan.IsPlayerControlled && caravan.PawnsListForReading.Count > 0)
                {
                    found.Add(caravan);
                }
            }

            return found;
        }

        static int CountSilver(RimWorld.Planet.Caravan caravan)
        {
            if (caravan == null || ThingDefOf.Silver == null)
            {
                return 0;
            }

            int count = 0;
            List<Thing> items = CaravanInventoryUtility.AllInventoryItems(caravan);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && items[i].def == ThingDefOf.Silver)
                {
                    count += items[i].stackCount;
                }
            }

            return count;
        }

        static int Cost()
        {
            SuiyinBook book = Current.Game?.GetComponent<NarrativeState>()?.Book;
            return book == null ? 250 : book.Config.RescueCost;
        }

        static bool HasTrait(Verse.Pawn pawn, TraitDef def)
        {
            List<Trait> traits = pawn.story.traits.allTraits;
            if (traits == null)
            {
                return false;
            }

            for (int i = 0; i < traits.Count; i++)
            {
                if (traits[i] != null && traits[i].def == def)
                {
                    return true;
                }
            }

            return false;
        }

        static Verse.Pawn FindPawn(int loadId)
        {
            int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            return RHAH_PawnIndex.Find(loadId, tick);
        }
    }
}
