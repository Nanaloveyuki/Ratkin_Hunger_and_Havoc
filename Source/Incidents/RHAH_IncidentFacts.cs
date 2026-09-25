using HungerAndHavoc.Core;
using System.Collections.Generic;
using HungerAndHavoc.Api;
using HungerAndHavoc.Generation;
using RimWorld;
using Verse;

namespace HungerAndHavoc.Incidents
{
    internal static class RHAH_IncidentFacts
    {
        internal static bool Submit(RHAH_IncidentContext context)
        {
            if (context == null || context.Map == null || context.PawnCount <= 0 ||
                context.SpawnCell == IntVec3.Invalid || context.SpawnBatchId <= 0 ||
                context.RelationshipGroupId <= 0 || context.Role == RHAH_PawnRole.Unspecified)
            {
                return false;
            }

            List<RHAH_PawnCreationResult> created = new List<RHAH_PawnCreationResult>();
            for (int i = 0; i < context.PawnCount; i++)
            {
                RHAH_PawnRole role = RHAH_IncidentRoster.RoleAt(context.DisplayId, context.Role, i);
                RHAH_PawnCreationResult result = RHAH_PawnFactory.Create(new RHAH_PawnRequest
                {
                    SourceIncidentDisplayId = context.DisplayId,
                    SpawnBatchId = context.SpawnBatchId,
                    RelationshipGroupId = context.RelationshipGroupId,
                    Role = role,
                    AttitudeAtArrival = context.Attitude,
                    CarriesPlague = context.CarriesPlague,
                    Map = context.Map,
                    PawnKind = Core.RHAH_DefOf.RHAH_PawnKind_Ratkin,
                    Faction = HungerAndHavoc.Pawn.RHAH_AttitudeFactions.Require(context.Attitude),
                    SpawnCell = context.SpawnCell,
                    Gender = RHAH_IncidentRoster.GenderAt(context.DisplayId, i),
                    BiologicalAge = GenerationAge(role),
                    StartLabor = RHAH_IncidentRoster.StartsLabor(context.DisplayId),
                    Shatter = RHAH_IncidentRoster.Shatters(context.DisplayId, i)
                }, registerBatch: false);

                if (!result.Succeeded)
                {
                    Rollback(created);
                    return false;
                }

                created.Add(result);
            }

            RHAH_Runtime.RegisterBatch(context.Map, context.SpawnBatchId);
            List<Verse.Pawn> arrived = new List<Verse.Pawn>();
            for (int i = 0; i < created.Count; i++)
            {
                for (int j = 0; j < created[i].Pawns.Count; j++)
                {
                    if (created[i].Pawns[j] != null)
                    {
                        arrived.Add(created[i].Pawns[j]);
                    }
                }
            }

            Pawn.Compat.RHAH_LeashBridge.TryLeashArrivals(arrived);
            List<int> loadIds = new List<int>(arrived.Count);
            for (int i = 0; i < arrived.Count; i++)
            {
                loadIds.Add(arrived[i].thingIDNumber);
            }

            int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            Current.Game?.GetComponent<Narrative.NarrativeState>()?.NoteIncident(new Narrative.SuiyinIncidentFact(
                context.DisplayId,
                context.Map.uniqueID,
                tick,
                context.SpawnBatchId,
                created.Count,
                context.CarriesPlague,
                loadIds.ToArray()));
            Narrative.RHAH_JournalRuntime.Open(context, loadIds);
            OpenChoice(context, created);

            return true;
        }
        static float? GenerationAge(RHAH_PawnRole role)
        {
            RHAH_Settings settings = RHAH_Mod.Settings;
            float min = settings == null ? 0f : settings.minGeneratedAge;
            float max = settings == null ? 50f : settings.maxGeneratedAge;
            bool youngFollows = settings != null && settings.youngAgeFollowsRange;
            return HungerAndHavoc.Pawn.RHAH_VisitorRules.GenerationAge(role, null, min, max, Rand.Value, youngFollows);
        }


        static void Rollback(List<RHAH_PawnCreationResult> created)
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
        static void OpenChoice(RHAH_IncidentContext context, List<RHAH_PawnCreationResult> created)
        {
            RHAH_RequestSpec spec = RHAH_RequestRules.SpecFor(context.DisplayId);
            RHAH_ChoiceKind choice = spec.Choice;
            if (choice == RHAH_ChoiceKind.None && RHAH_RequestRules.OffersVisitorControl(context.DisplayId))
            {
                choice = RHAH_ChoiceKind.Visitors;
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            if (choice == RHAH_ChoiceKind.None || (settings != null && !settings.AllowsRequest(choice) && choice != RHAH_ChoiceKind.Visitors))
            {
                return;
            }

            if (choice == RHAH_ChoiceKind.Visitors && settings != null && !settings.visitorChoicesEnabled)
            {
                return;
            }

            GameComponent_RHAH_Game game = Current.Game?.GetComponent<GameComponent_RHAH_Game>();
            int amount = RHAH_RequestRules.Amount(spec.Kind, context.Map.wealthWatcher?.WealthTotal ?? 0f, context.SpawnBatchId % 5, context.Points);
            RHAH_ChoiceRecord record = RHAH_ChoiceRuntime.Open(game, new RHAH_ChoiceRecord
            {
                DisplayId = context.DisplayId,
                MapId = context.Map.uniqueID,
                BatchId = context.SpawnBatchId,
                Kind = spec.Kind,
                Site = spec.Site,
                Choice = choice,
                Amount = amount,
                ExpireTick = Find.TickManager.TicksGame + RHAH_RequestRules.TicksPerDay
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

        static void SendLetter(RHAH_IncidentContext context, RHAH_ChoiceRecord record)
        {
            string label = LetterLabel(record);
            string text = LetterText(record);
            ChoiceLetter letter;
            if (record.Kind == RHAH_RequestKind.None)
            {
                ChoiceLetter_RHAH_Visitors visitors = (ChoiceLetter_RHAH_Visitors)LetterMaker.MakeLetter(
                    label, text, RHAH_DefOf.RHAH_ChoiceVisitors);
                visitors.choiceId = record.Id;
                visitors.choice = record.Choice;
                letter = visitors;
            }
            else
            {
                ChoiceLetter_RHAH_Request request = (ChoiceLetter_RHAH_Request)LetterMaker.MakeLetter(
                    label, text, RHAH_DefOf.RHAH_ChoiceRequest);
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

        static string LetterLabel(RHAH_ChoiceRecord record)
        {
            return (ChoiceKey(record) + "_Label").Translate();
        }

        static string LetterText(RHAH_ChoiceRecord record)
        {
            string key = ChoiceKey(record) + "_Text";
            string incident = IncidentLabel(record.DisplayId);
            if (record.Kind == RHAH_RequestKind.Baby)
            {
                return key.Translate(incident);
            }

            if (record.Kind == RHAH_RequestKind.None)
            {
                return key.Translate(incident);
            }

            return key.Translate(incident, record.Amount, GoodsLabel(record));
        }

        static string ChoiceKey(RHAH_ChoiceRecord record)
        {
            if (record.Kind == RHAH_RequestKind.Baby)
            {
                return "RHAH_Choice_Baby";
            }

            switch (record.Choice)
            {
                case RHAH_ChoiceKind.Aid: return "RHAH_Choice_Aid";
                case RHAH_ChoiceKind.Intel: return "RHAH_Choice_Intel";
                case RHAH_ChoiceKind.Refugees: return "RHAH_Choice_Refugees";
                case RHAH_ChoiceKind.Abandoned: return "RHAH_Choice_Abandoned";
                case RHAH_ChoiceKind.ChildExchange: return "RHAH_Choice_ChildExchange";
                case RHAH_ChoiceKind.Kinship: return "RHAH_Choice_Kinship";
                case RHAH_ChoiceKind.Airdrop: return "RHAH_Choice_Airdrop";
                case RHAH_ChoiceKind.Visitors: return "RHAH_Choice_Visitors";
                default: return "RHAH_Choice";
            }
        }

        static string IncidentLabel(string displayId)
        {
            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(displayId);
            if (entry == null)
            {
                return displayId;
            }

            return entry.LabelKey.Translate();
        }

        static string GoodsLabel(RHAH_ChoiceRecord record)
        {
            switch (record.Kind)
            {
                case RHAH_RequestKind.SimpleMeal: return "RHAH_Choice_SimpleMeal".Translate();
                case RHAH_RequestKind.FineMeal: return "RHAH_Choice_FineMeal".Translate();
                case RHAH_RequestKind.Medicine: return "RHAH_Choice_Medicine".Translate();
                case RHAH_RequestKind.HerbalMedicine: return "RHAH_Choice_HerbalMedicine".Translate();
                case RHAH_RequestKind.Silver: return "RHAH_Choice_Silver".Translate();
                default: return record.DisplayId;
            }
        }
    }
}
