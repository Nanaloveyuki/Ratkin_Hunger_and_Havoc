using HungerAndHavoc.Core;
using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Generation;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class HungerIncidentFacts
    {
        internal static bool Submit(HungerIncidentContext context)
        {
            if (context == null || context.Map == null || context.PawnCount <= 0 ||
                context.SpawnCell == IntVec3.Invalid || context.SpawnBatchId <= 0 ||
                context.RelationshipGroupId <= 0 || context.Role == HungerPawnRole.Unspecified)
            {
                return false;
            }

            List<HungerPawnCreationResult> created = new List<HungerPawnCreationResult>();
            for (int i = 0; i < context.PawnCount; i++)
            {
                HungerPawnCreationResult result = HungerPawnFactory.Create(new HungerPawnRequest
                {
                    SourceIncidentDisplayId = context.DisplayId,
                    SpawnBatchId = context.SpawnBatchId,
                    RelationshipGroupId = context.RelationshipGroupId,
                    Role = context.Role,
                    AttitudeAtArrival = context.Attitude,
                    CarriesPlague = context.CarriesPlague,
                    Map = context.Map,
                    PawnKind = PawnKindDefOf.Colonist,
                    Faction = HungerAndHavoc.Pawn.RHAH_AttitudeFactions.Resolve(context.Attitude) ?? Faction.OfPlayer,
                    SpawnCell = context.SpawnCell,
                    Profile = context.Profile
                });

                if (!result.Succeeded)
                {
                    Rollback(created);
                    return false;
                }

                created.Add(result);
            }
            Current.Game?.GetComponent<Narrative.NarrativeState>()?.NoteIncident(new Narrative.SuiyinIncidentFact(
                context.DisplayId,
                context.Map.uniqueID,
                context.SpawnBatchId,
                created.Count,
                context.CarriesPlague));
            OpenChoice(context, created);

            return true;
        }

        static void Rollback(List<HungerPawnCreationResult> created)
        {
            for (int i = 0; i < created.Count; i++)
            {
                for (int j = 0; j < created[i].Pawns.Count; j++)
                {
                    Verse.Pawn pawn = created[i].Pawns[j];
                    if (pawn != null && !pawn.Destroyed)
                    {
                        pawn.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }
        static void OpenChoice(HungerIncidentContext context, List<HungerPawnCreationResult> created)
        {
            HungerRequestSpec spec = HungerRequestRules.SpecFor(context.DisplayId);
            HungerChoiceKind choice = spec.Choice;
            if (choice == HungerChoiceKind.None && HungerRequestRules.OffersVisitorControl(context.DisplayId))
            {
                choice = HungerChoiceKind.Visitors;
            }

            HungerAndHavocSettings settings = HungerAndHavocMod.Settings;
            if (choice == HungerChoiceKind.None || (settings != null && !settings.AllowsRequest(choice) && choice != HungerChoiceKind.Visitors))
            {
                return;
            }

            if (choice == HungerChoiceKind.Visitors && settings != null && !settings.visitorChoicesEnabled)
            {
                return;
            }

            GameComponent_HungerAndHavoc game = Current.Game?.GetComponent<GameComponent_HungerAndHavoc>();
            int amount = HungerRequestRules.Amount(spec.Kind, context.Map.wealthWatcher?.WealthTotal ?? 0f, context.SpawnBatchId % 5, context.Points);
            HungerChoiceRecord record = HungerChoiceRuntime.Open(game, new HungerChoiceRecord
            {
                DisplayId = context.DisplayId,
                MapId = context.Map.uniqueID,
                BatchId = context.SpawnBatchId,
                Kind = spec.Kind,
                Site = spec.Site,
                Choice = choice,
                Amount = amount,
                ExpireTick = Find.TickManager.TicksGame + HungerRequestRules.TicksPerDay
            });
            if (record == null)
            {
                return;
            }

            for (int i = 0; i < created.Count; i++)
            {
                for (int j = 0; j < created[i].Pawns.Count; j++)
                {
                    Verse.Pawn pawn = created[i].Pawns[j];
                    if (pawn != null)
                    {
                        record.PawnLoadIds.Add(pawn.thingIDNumber);
                    }
                }
            }

            SendLetter(context, record);
        }

        static void SendLetter(HungerIncidentContext context, HungerChoiceRecord record)
        {
            string label = "RHAH_Choice_Label".Translate();
            string text = "RHAH_Choice_Text".Translate(record.Amount, HungerRequestRules.ThingDefName(record.Kind) ?? record.DisplayId);
            ChoiceLetter letter;
            if (record.Kind == HungerRequestKind.None)
            {
                ChoiceLetter_RHAH_Visitors visitors = (ChoiceLetter_RHAH_Visitors)LetterMaker.MakeLetter(
                    label, text, LetterDefOf.NeutralEvent);
                visitors.choiceId = record.Id;
                visitors.choice = record.Choice;
                letter = visitors;
            }
            else
            {
                ChoiceLetter_RHAH_Request request = (ChoiceLetter_RHAH_Request)LetterMaker.MakeLetter(
                    label, text, LetterDefOf.NeutralEvent);
                request.choiceId = record.Id;
                request.mapId = record.MapId;
                request.kind = record.Kind;
                request.site = record.Site;
                request.amount = record.Amount;
                request.expireTick = record.ExpireTick;
                letter = request;
            }

            Find.LetterStack.ReceiveLetter(letter);
        }
    }
}
