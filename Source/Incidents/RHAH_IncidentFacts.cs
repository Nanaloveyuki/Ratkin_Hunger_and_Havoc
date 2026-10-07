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
                    SpawnCell = context.DropPod ? IntVec3.Invalid : context.SpawnCell,
                    Gender = RHAH_IncidentRoster.GenderAt(context.DisplayId, i),
                    BiologicalAge = GenerationAge(role, context.DisplayId),
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
            StockCuisine(context, arrived);
            LinkFamily(context.DisplayId, arrived);
            if (Pawn.RHAH_VisitorGroup.TryStart(arrived, context.Map, context.SpawnCell, context.Role) == null)
            {
                Rollback(created);
                return false;
            }
            if (context.DropPod)
            {
                Pawn.RHAH_VisitorRules.DropPods(arrived, context.Map, context.SpawnCell, arrived[0].Faction);
            }

            RHAH_Runtime.RegisterBatch(context.Map, context.SpawnBatchId);
            List<int> loadIds = new List<int>(arrived.Count);
            for (int i = 0; i < arrived.Count; i++)
            {
                loadIds.Add(arrived[i].thingIDNumber);
            }

            int tick = Find.TickManager == null ? 0 : Find.TickManager.TicksGame;
            Current.Game?.GetComponent<Narrative.NarrativeState>()?.NoteIncident(new HungerAndHavoc.Storyteller.Suiyin.SuiyinIncidentFact(
                context.DisplayId,
                context.Map.uniqueID,
                tick,
                context.SpawnBatchId,
                created.Count,
                context.CarriesPlague,
                loadIds.ToArray()));
            HungerAndHavoc.Storyteller.Suiyin.RHAH_JournalRuntime.Open(context, loadIds);
            OpenChoice(context, created, arrived);
            EventMgr.RHAH_EventChainClock.NoteStarted(context.DisplayId, context.Map.uniqueID, 0, tick, context.SpawnBatchId);

            return true;
        }

        static void LinkFamily(string displayId, List<Verse.Pawn> arrived)
        {
            if (arrived.Count < 2 || (displayId != "I-003" && displayId != "I-004"))
            {
                return;
            }

            Identity.CompRHAH_Pawn mother = Identity.CompRHAH_Pawn.TryGet(arrived[0]);
            if (mother == null)
            {
                return;
            }

            for (int i = 1; i < arrived.Count; i++)
            {
                Identity.CompRHAH_Pawn child = Identity.CompRHAH_Pawn.TryGet(arrived[i]);
                if (child == null || !EventMgr.RHAH_EventFollowRules.IsYoung(child.State.role))
                {
                    continue;
                }

                child.State.parentPawnLoadId = arrived[0].thingIDNumber;
                mother.State.childPawnLoadIds.Add(arrived[i].thingIDNumber);
            }
        }

        static void StockCuisine(RHAH_IncidentContext context, List<Verse.Pawn> arrived)
        {
            if (!Trade.RHAH_CaravanStay.IsTradeCaravan(context.DisplayId, context.Role) || arrived.Count == 0)
            {
                return;
            }

            Verse.Pawn trader = null;
            Verse.Pawn carrier = null;
            float youngest = float.MaxValue;
            for (int i = 0; i < arrived.Count; i++)
            {
                Verse.Pawn pawn = arrived[i];
                IRHAH_Pawn snapshot = RHAH_Api.Get(pawn);
                if (snapshot != null && snapshot.Role == RHAH_PawnRole.Trader && trader == null)
                {
                    trader = pawn;
                }

                float age = pawn.ageTracker == null ? Pawn.Compat.RHAH_RatEggCuisine.AdultAge : pawn.ageTracker.AgeBiologicalYearsFloat;
                if (age < youngest)
                {
                    youngest = age;
                    carrier = pawn;
                }
            }

            if (carrier == trader)
            {
                return;
            }

            Verse.Pawn seller = trader ?? arrived[0];
            Pawn.Compat.RHAH_RatEggCuisine.Stock(seller, carrier);
            Pawn.Compat.RHAH_RatEggCuisine.OpenTrade(seller);
        }

        static float? GenerationAge(RHAH_PawnRole role, string displayId)
        {
            if (displayId == "I-032" || displayId == "I-046")
            {
                return HungerAndHavoc.Pawn.RHAH_VisitorRules.AirdropAge(Rand.Value);
            }

            RHAH_Settings settings = RHAH_Mod.Settings;
            float min = settings == null ? 0f : settings.minGeneratedAge;
            float max = settings == null ? 50f : settings.maxGeneratedAge;
            bool youngFollows = settings != null && settings.youngAgeFollowsRange;
            float motherMin = settings == null ? HungerAndHavoc.Pawn.RHAH_VisitorRules.DefaultMotherMinAge : settings.minMotherAge;
            return HungerAndHavoc.Pawn.RHAH_VisitorRules.GenerationAge(role, null, min, max, Rand.Value, youngFollows, motherMin);
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
        static void OpenChoice(RHAH_IncidentContext context, List<RHAH_PawnCreationResult> created, List<Verse.Pawn> arrived)
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
                ExpireTick = Find.TickManager.TicksGame + RHAH_RequestRules.TicksPerDay * (Core.RHAH_Mod.Settings == null ? RHAH_RequestRules.RequestDays : Core.RHAH_Mod.Settings.requestDays)
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

            SendLetter(context, record, Targets(created), arrived);
        }

        static void SendLetter(RHAH_IncidentContext context, RHAH_ChoiceRecord record, LookTargets targets, List<Verse.Pawn> arrived)
        {
            string label = LetterLabel(record);
            string text = LetterText(record, arrived);
            ChoiceLetter letter;
            if (record.Kind == RHAH_RequestKind.None)
            {
                ChoiceLetter_RHAH_Visitors visitors = (ChoiceLetter_RHAH_Visitors)LetterMaker.MakeLetter(
                    label, text, IncidentLetter(RHAH_DefOf.RHAH_ChoiceVisitors, RHAH_DefOf.RHAH_ChoiceVisitorsGreen), targets);
                visitors.choiceId = record.Id;
                visitors.choice = record.Choice;
                letter = visitors;
            }
            else
            {
                ChoiceLetter_RHAH_Request request = (ChoiceLetter_RHAH_Request)LetterMaker.MakeLetter(
                    label, text, IncidentLetter(RHAH_DefOf.RHAH_ChoiceRequest, RHAH_DefOf.RHAH_ChoiceRequestGreen), targets);
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

        static LetterDef IncidentLetter(LetterDef ordinary, LetterDef green)
        {
            RHAH_Settings settings = RHAH_Mod.Settings;
            return settings != null && settings.greenIncidentLetters && green != null ? green : ordinary;
        }

        static LookTargets Targets(List<RHAH_PawnCreationResult> created)
        {
            List<Verse.Pawn> pawns = new List<Verse.Pawn>();
            for (int i = 0; i < created.Count; i++)
            {
                for (int j = 0; j < created[i].Pawns.Count; j++)
                {
                    Verse.Pawn pawn = created[i].Pawns[j];
                    if (pawn != null && !pawn.Destroyed)
                    {
                        pawns.Add(pawn);
                    }
                }
            }

            return pawns.Count == 0 ? null : new LookTargets(pawns);
        }

        static string LetterLabel(RHAH_ChoiceRecord record)
        {
            return (ChoiceKey(record) + "_Label").Translate();
        }

        static string LetterText(RHAH_ChoiceRecord record, List<Verse.Pawn> arrived)
        {
            string choiceKey = ChoiceKey(record);
            string key = choiceKey + "_Text";
            string incident = IncidentLabel(record.DisplayId);
            RHAH_Settings settings = RHAH_Mod.Settings;
            int count = record.PawnLoadIds.Count;
            bool family = record.DisplayId == "I-003" || record.DisplayId == "I-004";
            bool hasYoungText = choiceKey == "RHAH_Choice_Abandoned" || choiceKey == "RHAH_Choice_ChildExchange" ||
                choiceKey == "RHAH_Choice_BeggarFamily" || choiceKey == "RHAH_Choice_Kinship" ||
                choiceKey == "RHAH_Choice_Airdrop" || choiceKey == "RHAH_Choice_YoungThieves" || choiceKey == "RHAH_Choice_WildYoung";
            bool young = !hasYoungText || YoungArrivals(arrived, family ? 1 : 0);
            if (!young)
            {
                switch (choiceKey)
                {
                    case "RHAH_Choice_Abandoned": key = "RHAH_Choice_AbandonedGrown_Text"; break;
                    case "RHAH_Choice_ChildExchange": key = "RHAH_Choice_ChildExchangeGrown_Text"; break;
                    case "RHAH_Choice_BeggarFamily": key = "RHAH_Choice_BeggarFamilyGrown_Text"; break;
                    case "RHAH_Choice_Kinship": key = "RHAH_Choice_KinshipGrown_Text"; break;
                    case "RHAH_Choice_Airdrop": key = "RHAH_Choice_AirdropGrown_Text"; break;
                    case "RHAH_Choice_YoungThieves": key = "RHAH_Choice_Thieves_Text"; break;
                    case "RHAH_Choice_WildYoung": key = "RHAH_Choice_Wild_Text"; break;
                }
            }
            if (choiceKey == "RHAH_Choice_Abandoned" && count == 1)
            {
                key = young ? "RHAH_Choice_AbandonedSingle_Text" : "RHAH_Choice_AbandonedSingleGrown_Text";
            }

            if (choiceKey == "RHAH_Choice_Kinship" && arrived != null && arrived.Count > 0 &&
                arrived[0].ageTracker != null && arrived[0].ageTracker.AgeBiologicalYearsFloat < 1f)
            {
                key = "RHAH_Choice_KinshipInfant_Text";
            }
            if ((choiceKey == "RHAH_Choice_Wild" || (choiceKey == "RHAH_Choice_WildYoung" && !young)) && count == 1)
            {
                key = "RHAH_Choice_WildSingle_Text";
            }

            string text;
            if (record.Kind == RHAH_RequestKind.None || record.Kind == RHAH_RequestKind.Baby)
            {
                text = key.Translate(incident, family ? count - 1 : count);
            }
            else
            {
                text = key.Translate(incident, record.Amount, GoodsLabel(record));
            }

            if (record.Choice == RHAH_ChoiceKind.ChildExchange)
            {
                int each = RHAH_RequestRules.FoodForChildren(1);
                if (settings != null && !settings.childExchangeFoodSubstitution)
                {
                    text += "\n\n" + "RHAH_Choice_ChildExchangeNoFoodHint".Translate();
                }
                else if (each <= 0)
                {
                    text += "\n\n" + "RHAH_Choice_ChildExchangeZeroFoodHint".Translate();
                }
                else
                {
                    text += "\n\n" + "RHAH_Choice_ChildExchangeFoodHint".Translate(each, RHAH_RequestRules.FoodForChildren(count));
                }
            }

            int days = settings == null ? RHAH_RequestRules.RequestDays : settings.requestDays;
            if (record.Kind == RHAH_RequestKind.None)
            {
                text += "\n\n" + "RHAH_Choice_VisitorHint".Translate(days);
            }
            else if (record.Choice == RHAH_ChoiceKind.Intel)
            {
                text += "\n\n" + "RHAH_Choice_IntelHint".Translate(days);
            }
            else
            {
                text += "\n\n" + "RHAH_Choice_RequestHint".Translate(days);
            }

            RHAH_IncidentEntry entry = RHAH_IncidentCatalog.GetByDisplayId(record.DisplayId);
            if (entry != null && entry.Category == RHAH_IncidentCategory.Plague)
            {
                string plagueKey = settings == null || settings.plagueEnabled
                    ? "RHAH_Choice_PlagueHint" : "RHAH_Choice_PlagueDisabledHint";
                text += "\n\n" + plagueKey.Translate();
            }

            return text;
        }

        static bool YoungArrivals(List<Verse.Pawn> arrived, int start)
        {
            if (arrived == null || arrived.Count <= start)
            {
                return false;
            }

            for (int i = start; i < arrived.Count; i++)
            {
                if (arrived[i].ageTracker == null || arrived[i].ageTracker.AgeBiologicalYearsFloat >= 14f)
                {
                    return false;
                }
            }

            return true;
        }

        static string ChoiceKey(RHAH_ChoiceRecord record)
        {
            switch (record.DisplayId)
            {
                case "I-003": return "RHAH_Choice_ShatteredMother";
                case "I-004": return "RHAH_Choice_BeggarFamily";
                case "I-005":
                case "I-042": return "RHAH_Choice_BeggarGroup";
                case "I-006": return "RHAH_Choice_Thieves";
                case "I-007":
                case "I-043": return "RHAH_Choice_YoungThieves";
                case "I-008":
                case "I-010":
                case "I-036": return "RHAH_Choice_Wild";
                case "I-009": return "RHAH_Choice_WildYoung";
                case "I-013": return "RHAH_Choice_ChildExchange";
                case "I-014":
                case "I-030":
                case "I-045": return "RHAH_Choice_Siege";
                case "I-029":
                case "I-044": return "RHAH_Choice_LaboringRefugees";
                case "I-031":
                case "I-039": return "RHAH_Choice_Passersby";
                case "I-037": return "RHAH_Choice_PlagueAbandoned";
                case "I-040": return "RHAH_Choice_Refugees";
                case "I-041": return "RHAH_Choice_Orphan";
            }

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
