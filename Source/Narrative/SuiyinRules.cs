using System.Collections.Generic;

namespace HungerAndHavoc.Narrative
{
    internal enum SuiyinLetter
    {
        None = 0,
        N001,
        N002,
        N003,
        N004Entry,
        N004ChildAlone,
        N004FamilyHere,
        N004Captive,
        N004Story,
        N004Banished,
        N004Revisit,
        N004RescuePaid,
        N004Killed,
        N004Regret,
        N005Accepted,
        N005Cared,
        N005Dead,
        N005Left,
        N005Missing,
        N005Rejected,
        N005TimedOut,
        N005Broken,
        N006Entry,
        N006Sealed,
        N006BaitSet,
        N006BaitGone,
        N006Traced,
        N006Stopped,
        N006Cleaned,
        N006Gone,
        N007Choice,
        N007Quarantine,
        N007Release,
        N007Defer,
        N007Broken,
        N007Recovered,
        N007Stayed,
        N007AllDead,
        N007Return,
        N008Arrive,
        N008Trade,
        N008Proof,
        N008NoProof,
        N008Refuse,
        N008Drive,
        N008Dead,
        N008Left,
        N008Missing,
        N008Timeout,
        N009Arrive,
        N009Take,
        N009Leave,
        N009Hand,
        N009Share,
        N009Destroy,
        N009Empty,
        Journal,
        Aside
    }

    internal enum SuiyinAside
    {
        None = 0,
        ChildToExchange = 1,
        GranaryToRelic = 2,
        QuarantineToEnvoy = 3,
        EnvoyToRelic = 4
    }

    internal enum SuiyinPresence
    {
        Unknown = 0,
        Here = 1,
        Dead = 2,
        Left = 3,
        Missing = 4
    }

    internal enum SuiyinCare
    {
        Free = 0,
        Captive = 1,
        Hungry = 2,
        Plague = 3
    }

    internal enum SuiyinN004Outcome
    {
        Pending = 0,
        ChildAlone = 1,
        FamilyHere = 2,
        Captive = 3,
        Story = 4,
        Banished = 5,
        Regret = 6,
        Unknown = 7
    }

    internal enum SuiyinN004Revisit
    {
        None = 0,
        Ignore = 1,
        Rescue = 2,
        Kill = 3
    }

    internal enum SuiyinN005Outcome
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        TimedOut = 3,
        Broken = 4,
        Cared = 5,
        Dead = 6,
        Left = 7,
        Missing = 8
    }

    internal enum SuiyinN006Action
    {
        None = 0,
        Seal = 1,
        Bait = 2,
        Clean = 3,
        Ignore = 4,
        Trace = 5,
        Stop = 6
    }

    internal enum SuiyinN006Outcome
    {
        Pending = 0,
        Sealed = 1,
        BaitSet = 2,
        Traced = 3,
        Stopped = 4,
        Cleaned = 5,
        Gone = 6
    }

    internal enum SuiyinN007Action
    {
        None = 0,
        Quarantine = 1,
        Release = 2,
        Defer = 3,
        AcceptRecovered = 4
    }

    internal enum SuiyinN007Outcome
    {
        Pending = 0,
        Quarantine = 1,
        Release = 2,
        Defer = 3,
        Broken = 4,
        RecoveredLeft = 5,
        RecoveredStayed = 6,
        AllDead = 7,
        Missing = 8
    }

    internal enum SuiyinN008Action
    {
        None = 0,
        Trade = 1,
        Proof = 2,
        Refuse = 3,
        Drive = 4
    }

    internal enum SuiyinN008Outcome
    {
        Pending = 0,
        Waiting = 1,
        Checking = 2,
        Traded = 3,
        Refused = 4,
        Driven = 5,
        Dead = 6,
        Left = 7,
        Missing = 8,
        TimedOut = 9,
        NoProof = 10
    }

    internal enum SuiyinN009Action
    {
        None = 0,
        Take = 1,
        Leave = 2,
        Hand = 3,
        Share = 4,
        Destroy = 5
    }

    internal enum SuiyinN009Outcome
    {
        Pending = 0,
        Taken = 1,
        Left = 2,
        Handed = 3,
        Shared = 4,
        Destroyed = 5,
        Empty = 6
    }

    internal readonly struct SuiyinNotice
    {
        internal readonly SuiyinLetter Letter;
        internal readonly int Arg;
        internal readonly bool Private;

        internal SuiyinNotice(SuiyinLetter letter, int arg, bool isPrivate)
        {
            Letter = letter;
            Arg = arg;
            Private = isPrivate;
        }
    }

    internal sealed class SuiyinConfig
    {
        internal int ProgressKinds = 3;
        internal int RewardKinds = 8;
        internal int EnvoyKinds = 5;
        internal int RelicKinds = 8;
        internal int TheftKinds = 2;
        internal int RewardSilver = 300;
        internal int RescueCost = 250;
        internal int RescueReward = 2500;
        internal int RelicTakeSilver = 200;
        internal int RelicHandSilver = 100;
        internal int CareDays = 5;
        internal int MissingDays = 1;
        internal int ObserveDays = 30;
        internal int HoleIgnoreDays = 3;
        internal int EnvoyWaitDays = 3;
        internal int EnvoyCheckDays = 1;
        internal int RelicDays = 15;
        internal int ReturnDays = 15;
        internal int RevisitYears = 4;
        internal int AsideCooldownDays = 3;
        internal int TrustAsideCutoff = -75;
        internal int AdultYears = 18;
    }

    internal sealed class SuiyinMember
    {
        internal int LoadId;
        internal SuiyinPresence Presence = SuiyinPresence.Here;
        internal SuiyinCare Care;
        internal bool Child;
        internal int CareTicks;
        internal int MissingSince = -1;
    }

    internal sealed class SuiyinN004Case
    {
        internal int Id;
        internal int MotherId;
        internal int MapId;
        internal int StartedTick;
        internal SuiyinN004Outcome Outcome;
        internal bool EffectsApplied;
        internal int RevisitDeadline = -1;
        internal bool RevisitSeen;
        internal SuiyinN004Revisit Revisit;
        internal int RescueDueTick = -1;
        internal bool RescuePaid;
        internal int BreakUntil = -1;
        internal bool BreakTrait;
        internal readonly List<SuiyinMember> Children = new List<SuiyinMember>();
    }

    internal sealed class SuiyinN005Case
    {
        internal int Id;
        internal int MapId;
        internal int StartedTick;
        internal SuiyinN005Outcome Outcome;
        internal bool CareClosed;
        internal readonly List<SuiyinMember> Children = new List<SuiyinMember>();
    }

    internal sealed class SuiyinN006Case
    {
        internal int MapId;
        internal int Thefts;
        internal int StartedTick = -1;
        internal int IgnoreUntil = -1;
        internal int BaitUntil = -1;
        internal bool Hole;
        internal bool FoodPresent = true;
        internal int Wood;
        internal bool BaitStock;
        internal SuiyinN006Outcome Outcome;
    }

    internal sealed class SuiyinN007Case
    {
        internal int MapId;
        internal int StartedTick;
        internal SuiyinN007Outcome Outcome;
        internal int ReturnDueTick = -1;
        internal int ReturnPawnId;
        internal bool ReturnDone;
        internal readonly List<SuiyinMember> Visitors = new List<SuiyinMember>();
    }

    internal sealed class SuiyinN008Case
    {
        internal int MapId;
        internal int PawnId;
        internal int StartedTick;
        internal int Deadline = -1;
        internal int CheckUntil = -1;
        internal SuiyinPresence Presence = SuiyinPresence.Here;
        internal int MissingSince = -1;
        internal SuiyinN008Outcome Outcome;
        internal bool MealsReady;
        internal bool ProofAvailable;
    }

    internal sealed class SuiyinN009Case
    {
        internal int StartedTick;
        internal int Deadline = -1;
        internal bool MapPresent = true;
        internal bool PlayersInside;
        internal bool EnvoyHere;
        internal bool BoxDestroyed;
        internal SuiyinN009Outcome Outcome;
    }

    internal sealed class SuiyinJournalCase
    {
        internal int Id;
        internal int MapId;
        internal int BatchId;
        internal int StartedTick;
        internal bool Delivered;
        internal bool Driven;
        internal bool Closed;
        internal bool Counted;
        internal readonly List<SuiyinMember> People = new List<SuiyinMember>();
    }

    internal sealed class SuiyinBook
    {
        internal readonly SuiyinConfig Config = new SuiyinConfig();
        internal SuiyinLedger Gates;
        internal readonly List<string> Seen = new List<string>();
        internal readonly List<int> TheftMaps = new List<int>();
        internal readonly List<int> TheftCounts = new List<int>();
        internal readonly List<SuiyinN004Case> N004 = new List<SuiyinN004Case>();
        internal readonly List<SuiyinN005Case> N005 = new List<SuiyinN005Case>();
        internal readonly List<SuiyinN006Case> N006 = new List<SuiyinN006Case>();
        internal readonly List<SuiyinN007Case> N007 = new List<SuiyinN007Case>();
        internal readonly List<SuiyinN008Case> N008 = new List<SuiyinN008Case>();
        internal SuiyinN009Case N009;
        internal readonly List<SuiyinJournalCase> Journals = new List<SuiyinJournalCase>();
        internal readonly List<int> JournalNoted = new List<int>();
        internal readonly List<int> AsidesSent = new List<int>();
        internal bool OpeningSent;
        internal bool ProgressSent;
        internal bool RewardClaimed;
        internal int RewardPaid;
        internal bool EnvoyClue;
        internal bool RelicClue;
        internal int LastAsideTick = -1;
        internal int NextCaseId = 1;
        internal int Trust;
        internal bool Narrator = true;
        internal readonly List<SuiyinNotice> Pending = new List<SuiyinNotice>();

        internal int Distinct => Seen.Count;
        internal void ExportSave(
            List<string> seen,
            List<int> theftMaps,
            List<int> theftCounts,
            List<int> journalNoted,
            List<int> asides)
        {
            Copy(Seen, seen);
            Copy(TheftMaps, theftMaps);
            Copy(TheftCounts, theftCounts);
            Copy(JournalNoted, journalNoted);
            Copy(AsidesSent, asides);
        }

        internal void ImportSave(
            List<string> seen,
            List<int> theftMaps,
            List<int> theftCounts,
            List<int> journalNoted,
            List<int> asides)
        {
            Replace(Seen, seen);
            Replace(TheftMaps, theftMaps);
            Replace(TheftCounts, theftCounts);
            Replace(JournalNoted, journalNoted);
            Replace(AsidesSent, asides);
            Pending.Clear();
        }

        static void Copy<T>(List<T> source, List<T> destination)
        {
            if (destination == null)
            {
                return;
            }

            destination.Clear();
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }

        static void Replace<T>(List<T> destination, List<T> source)
        {
            destination.Clear();
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                destination.Add(source[i]);
            }
        }


        internal void Note(string displayId, int mapId, int tick, bool plague, bool enabled)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return;
            }

            if (Catalog(displayId) && !Seen.Contains(displayId))
            {
                Seen.Add(displayId);
            }

            if (!enabled)
            {
                return;
            }

            if (!OpeningSent && Allows(SuiyinNode.N001))
            {
                OpeningSent = true;
                Queue(SuiyinLetter.N001, 0, true);
            }

            if (!ProgressSent && Allows(SuiyinNode.N002) && Distinct >= Config.ProgressKinds)
            {
                ProgressSent = true;
                Queue(SuiyinLetter.N002, Distinct, true);
            }

            if (!RewardClaimed && Allows(SuiyinNode.N003) && Distinct >= Config.RewardKinds)
            {
                RewardClaimed = true;
                RewardPaid = Scaled(Config.RewardSilver);
                Queue(SuiyinLetter.N003, RewardPaid, true);
            }

            if (plague && Allows(SuiyinNode.N007) && OpenN007(mapId) == null)
            {
                SuiyinN007Case quarantine = new SuiyinN007Case
                {
                    MapId = mapId,
                    StartedTick = tick
                };
                N007.Add(quarantine);
                Queue(SuiyinLetter.N007Choice, mapId, false);
            }

            if (IsTheft(displayId))
            {
                NoteTheft(mapId, tick);
            }

            if (displayId == "I-013" && Allows(SuiyinNode.N005))
            {
                Queue(SuiyinLetter.N005Accepted, 0, false);
            }

            TryEnvoy(tick);
            TryRelic(tick);
            TryAsides(tick);
        }

        internal void NotePlague(int mapId, int recovered, int died)
        {
            SuiyinN007Case open = OpenN007(mapId);
            if (open == null)
            {
                return;
            }

            if (recovered > 0 && !Has(SuiyinLetter.N007Recovered))
            {
                Queue(SuiyinLetter.N007Recovered, recovered, false);
            }

            if (died > 0 && recovered <= 0 && open.Visitors.Count > 0 && AllDead(open))
            {
                open.Outcome = SuiyinN007Outcome.AllDead;
                Queue(SuiyinLetter.N007AllDead, died, false);
            }
        }

        internal bool OpenEntrust(int motherId, int mapId, int tick, IList<int> childIds)
        {
            if (motherId <= 0 || childIds == null || childIds.Count == 0 || !Allows(SuiyinNode.N004))
            {
                return false;
            }

            for (int i = 0; i < N004.Count; i++)
            {
                if (N004[i].MotherId == motherId && N004[i].Outcome == SuiyinN004Outcome.Pending)
                {
                    return false;
                }
            }

            SuiyinN004Case record = new SuiyinN004Case
            {
                Id = NextCaseId++,
                MotherId = motherId,
                MapId = mapId,
                StartedTick = tick
            };
            for (int i = 0; i < childIds.Count; i++)
            {
                if (childIds[i] > 0)
                {
                    record.Children.Add(new SuiyinMember { LoadId = childIds[i], Child = true });
                }
            }

            if (record.Children.Count == 0)
            {
                return false;
            }

            N004.Add(record);
            Queue(SuiyinLetter.N004Entry, record.Id, true);
            return true;
        }

        internal SuiyinN004Outcome ResolveEntrust(SuiyinN004Case record, int tick, SuiyinPresence mother)
        {
            if (record == null || record.Outcome != SuiyinN004Outcome.Pending)
            {
                return record == null ? SuiyinN004Outcome.Pending : record.Outcome;
            }

            int alive = 0;
            int dead = 0;
            int present = 0;
            int captive = 0;
            int grown = 0;
            for (int i = 0; i < record.Children.Count; i++)
            {
                SuiyinMember child = record.Children[i];
                if (child.Presence == SuiyinPresence.Dead)
                {
                    dead++;
                    continue;
                }

                if (child.Presence == SuiyinPresence.Unknown)
                {
                    continue;
                }

                alive++;
                if (child.Presence == SuiyinPresence.Here)
                {
                    present++;
                }

                if (child.Care == SuiyinCare.Captive)
                {
                    captive++;
                }

                if (!child.Child)
                {
                    grown++;
                }
            }

            SuiyinN004Outcome next = SuiyinN004Outcome.Pending;
            if (mother == SuiyinPresence.Dead && alive == 0 && dead == record.Children.Count)
            {
                next = SuiyinN004Outcome.Story;
            }
            else if (mother == SuiyinPresence.Here && alive == 0 && dead > 0)
            {
                next = SuiyinN004Outcome.Regret;
            }
            else if (mother == SuiyinPresence.Here && alive > 0 && present == alive && grown == alive && captive == 0)
            {
                next = SuiyinN004Outcome.FamilyHere;
            }
            else if (mother == SuiyinPresence.Dead && alive > 0 && grown == alive && captive == alive)
            {
                next = SuiyinN004Outcome.Captive;
            }
            else if (mother == SuiyinPresence.Dead && alive > 0 && grown == alive && present == alive)
            {
                next = SuiyinN004Outcome.ChildAlone;
            }
            else if ((mother == SuiyinPresence.Left || mother == SuiyinPresence.Missing) && present < alive)
            {
                next = SuiyinN004Outcome.Banished;
            }
            else if (tick - record.StartedTick >= Days(Config.ObserveDays) && grown < alive)
            {
                next = SuiyinN004Outcome.Unknown;
            }

            if (next == SuiyinN004Outcome.Pending)
            {
                return next;
            }

            record.Outcome = next;
            if (!record.EffectsApplied)
            {
                record.EffectsApplied = true;
                Trust = SuiyinNodes.ClampTrust(Trust + TrustFor(next));
                if (next == SuiyinN004Outcome.Banished)
                {
                    record.RevisitDeadline = tick + Years(Config.RevisitYears);
                }

                if (next == SuiyinN004Outcome.ChildAlone)
                {
                    record.BreakUntil = tick + Days(1);
                }

                Queue(LetterFor(next), record.Id, next == SuiyinN004Outcome.Story || next == SuiyinN004Outcome.Banished);
            }

            return next;
        }

        internal bool ChooseRevisit(SuiyinN004Case record, SuiyinN004Revisit choice, int tick, bool canPay)
        {
            if (record == null || record.Outcome != SuiyinN004Outcome.Banished || record.Revisit != SuiyinN004Revisit.None || record.RevisitSeen == false)
            {
                return false;
            }

            if (choice == SuiyinN004Revisit.Rescue && (!canPay || record.RescuePaid))
            {
                return false;
            }

            record.Revisit = choice;
            if (choice == SuiyinN004Revisit.Rescue)
            {
                record.RescuePaid = true;
                record.RescueDueTick = tick + Years(Config.RevisitYears);
                Queue(SuiyinLetter.N004RescuePaid, Config.RescueCost, false);
            }
            else if (choice == SuiyinN004Revisit.Kill)
            {
                Trust = SuiyinNodes.ClampTrust(Trust - 10);
                Queue(SuiyinLetter.N004Killed, record.Id, true);
            }

            return true;
        }

        internal int ClaimRescue(SuiyinN004Case record, int tick)
        {
            if (record == null || !record.RescuePaid || record.RescueDueTick < 0 || tick < record.RescueDueTick)
            {
                return 0;
            }

            int paid = record.RescueDueTick;
            record.RescueDueTick = -1;
            return paid == 0 ? 0 : Config.RescueReward;
        }

        internal bool AcceptExchange(int id, int mapId, int tick, IList<int> childIds)
        {
            if (id <= 0 || !Allows(SuiyinNode.N005) || FindN005(id) != null)
            {
                return false;
            }

            SuiyinN005Case record = new SuiyinN005Case
            {
                Id = id,
                MapId = mapId,
                StartedTick = tick,
                Outcome = SuiyinN005Outcome.Accepted
            };
            if (childIds != null)
            {
                for (int i = 0; i < childIds.Count; i++)
                {
                    if (childIds[i] > 0)
                    {
                        record.Children.Add(new SuiyinMember { LoadId = childIds[i], Child = true });
                    }
                }
            }

            N005.Add(record);
            EnvoyClue = false;
            TryAsides(tick);
            Queue(SuiyinLetter.N005Accepted, id, false);
            TryEnvoy(tick);
            return true;
        }

        internal SuiyinN005Outcome AdvanceCare(SuiyinN005Case record, int tick, int delta)
        {
            if (record == null || record.Outcome != SuiyinN005Outcome.Accepted || record.CareClosed)
            {
                return record == null ? SuiyinN005Outcome.Pending : record.Outcome;
            }

            if (record.Children.Count == 0)
            {
                record.CareClosed = true;
                record.Outcome = SuiyinN005Outcome.Missing;
                Queue(SuiyinLetter.N005Missing, record.Id, false);
                return record.Outcome;
            }

            bool pending = false;
            bool anyDead = false;
            bool anyMissing = false;
            bool anyLeft = false;
            bool allSettled = true;
            for (int i = 0; i < record.Children.Count; i++)
            {
                SuiyinMember child = record.Children[i];
                if (child.Presence == SuiyinPresence.Unknown)
                {
                    if (child.MissingSince < 0)
                    {
                        child.MissingSince = tick;
                    }
                    if (tick - child.MissingSince >= Days(Config.MissingDays))
                    {
                        child.Presence = SuiyinPresence.Missing;
                    }
                }

                if (child.Presence == SuiyinPresence.Here && child.Care != SuiyinCare.Hungry && child.Care != SuiyinCare.Plague)
                {
                    child.CareTicks += delta;
                }

                if (child.Presence == SuiyinPresence.Here)
                {
                    if (child.CareTicks < Days(Config.CareDays))
                    {
                        pending = true;
                        allSettled = false;
                    }
                }
                else if (child.Presence == SuiyinPresence.Dead)
                {
                    anyDead = true;
                    allSettled = false;
                }
                else if (child.Presence == SuiyinPresence.Left)
                {
                    anyLeft = true;
                    allSettled = false;
                }
                else if (child.Presence == SuiyinPresence.Unknown)
                {
                    pending = true;
                    allSettled = false;
                }
                else if (child.Presence == SuiyinPresence.Missing)
                {
                    anyMissing = true;
                    allSettled = false;
                }
            }

            if (pending)
            {
                return record.Outcome;
            }

            record.CareClosed = true;
            if (anyDead)
            {
                record.Outcome = SuiyinN005Outcome.Dead;
                Queue(SuiyinLetter.N005Dead, record.Id, false);
            }
            else if (allSettled)
            {
                record.Outcome = SuiyinN005Outcome.Cared;
                Trust = SuiyinNodes.ClampTrust(Trust + 3);
                EnvoyClue = true;
                Queue(SuiyinLetter.N005Cared, record.Id, true);
                TryEnvoy(tick);
            }
            else if (anyMissing)
            {
                record.Outcome = SuiyinN005Outcome.Missing;
                Queue(SuiyinLetter.N005Missing, record.Id, false);
            }
            else if (anyLeft)
            {
                record.Outcome = SuiyinN005Outcome.Left;
                Queue(SuiyinLetter.N005Left, record.Id, false);
            }
            else
            {
                record.Outcome = SuiyinN005Outcome.Missing;
                Queue(SuiyinLetter.N005Missing, record.Id, false);
            }

            TryAsides(tick);
            return record.Outcome;
        }

        internal bool NoteTheft(int mapId, int tick)
        {
            int index = TheftMaps.IndexOf(mapId);
            if (index < 0)
            {
                TheftMaps.Add(mapId);
                TheftCounts.Add(1);
                index = TheftMaps.Count - 1;
            }
            else
            {
                TheftCounts[index]++;
            }

            if (TheftCounts[index] < Config.TheftKinds || !Allows(SuiyinNode.N006))
            {
                return false;
            }

            SuiyinN006Case open = FindN006(mapId);
            if (open != null)
            {
                return false;
            }

            N006.Add(new SuiyinN006Case
            {
                MapId = mapId,
                Thefts = TheftCounts[index],
                StartedTick = tick,
                IgnoreUntil = tick + Days(Config.HoleIgnoreDays),
                Hole = false,
                FoodPresent = false
            });
            return true;
        }

        internal bool PlaceHole(SuiyinN006Case record)
        {
            if (record == null || record.Outcome != SuiyinN006Outcome.Pending || record.Hole || !record.FoodPresent)
            {
                return false;
            }

            record.Hole = true;
            Queue(SuiyinLetter.N006Entry, record.MapId, false);
            return true;
        }

        internal bool ChooseHole(SuiyinN006Case record, SuiyinN006Action action, int tick)
        {
            if (record == null || record.Outcome != SuiyinN006Outcome.Pending || !record.Hole)
            {
                return false;
            }

            if (action == SuiyinN006Action.Seal)
            {
                if (record.Wood < 20)
                {
                    return false;
                }

                record.Wood -= 20;
                record.Outcome = SuiyinN006Outcome.Sealed;
                record.Hole = false;
                Trust = SuiyinNodes.ClampTrust(Trust + 1);
                Queue(SuiyinLetter.N006Sealed, record.MapId, false);
                return true;
            }

            if (action == SuiyinN006Action.Bait)
            {
                if (!record.BaitStock || record.BaitUntil >= 0)
                {
                    return false;
                }

                record.BaitStock = false;
                record.BaitUntil = tick + Days(1);
                record.Outcome = SuiyinN006Outcome.BaitSet;
                Queue(SuiyinLetter.N006BaitSet, record.MapId, false);
                return true;
            }

            if (action == SuiyinN006Action.Clean)
            {
                record.Outcome = SuiyinN006Outcome.Cleaned;
                record.Hole = false;
                Trust = SuiyinNodes.ClampTrust(Trust - 1);
                Queue(SuiyinLetter.N006Cleaned, record.MapId, false);
                return true;
            }

            if (action == SuiyinN006Action.Ignore && tick >= record.IgnoreUntil)
            {
                record.Outcome = SuiyinN006Outcome.Gone;
                record.Hole = false;
                Queue(SuiyinLetter.N006Gone, record.MapId, false);
                return true;
            }

            return false;
        }

        internal bool FinishBait(SuiyinN006Case record, int tick, bool trace)
        {
            if (record == null || record.Outcome != SuiyinN006Outcome.BaitSet || record.BaitUntil < 0 || tick < record.BaitUntil)
            {
                return false;
            }

            record.Hole = false;
            if (trace)
            {
                record.Outcome = SuiyinN006Outcome.Traced;
                Trust = SuiyinNodes.ClampTrust(Trust + 3);
                RelicClue = true;
                Queue(SuiyinLetter.N006Traced, record.MapId, false);
                TryRelic(tick);
            }
            else
            {
                record.Outcome = SuiyinN006Outcome.Stopped;
                Queue(SuiyinLetter.N006Stopped, record.MapId, false);
            }

            TryAsides(tick);
            return true;
        }

        internal bool ChooseQuarantine(SuiyinN007Case record, SuiyinN007Action action)
        {
            if (record == null || record.Outcome != SuiyinN007Outcome.Pending)
            {
                return false;
            }

            if (action == SuiyinN007Action.Quarantine)
            {
                record.Outcome = SuiyinN007Outcome.Quarantine;
                Queue(SuiyinLetter.N007Quarantine, record.MapId, false);
                return true;
            }

            if (action == SuiyinN007Action.Release)
            {
                record.Outcome = SuiyinN007Outcome.Release;
                Queue(SuiyinLetter.N007Release, record.MapId, false);
                return true;
            }

            if (action == SuiyinN007Action.Defer)
            {
                record.Outcome = SuiyinN007Outcome.Defer;
                Queue(SuiyinLetter.N007Defer, record.MapId, false);
                return true;
            }

            if (action == SuiyinN007Action.AcceptRecovered && HasRecovered(record))
            {
                record.Outcome = SuiyinN007Outcome.RecoveredStayed;
                Trust = SuiyinNodes.ClampTrust(Trust + 1);
                Queue(SuiyinLetter.N007Stayed, record.MapId, false);
                return true;
            }

            return false;
        }

        internal bool CloseQuarantine(SuiyinN007Case record, SuiyinN007Outcome outcome, int tick, int returnPawnId)
        {
            if (record == null || record.Outcome == SuiyinN007Outcome.AllDead || record.Outcome == SuiyinN007Outcome.RecoveredLeft || record.Outcome == SuiyinN007Outcome.RecoveredStayed || record.Outcome == SuiyinN007Outcome.Missing)
            {
                return false;
            }

            record.Outcome = outcome;
            if (outcome == SuiyinN007Outcome.RecoveredLeft)
            {
                Trust = SuiyinNodes.ClampTrust(Trust + 2);
                EnvoyClue = true;
                if (returnPawnId > 0 && record.ReturnDueTick < 0)
                {
                    record.ReturnPawnId = returnPawnId;
                    record.ReturnDueTick = tick + Days(Config.ReturnDays);
                }

                Queue(SuiyinLetter.N007Recovered, record.MapId, false);
                TryEnvoy(tick);
            }
            else if (outcome == SuiyinN007Outcome.Broken)
            {
                Trust = SuiyinNodes.ClampTrust(Trust - 2);
                Queue(SuiyinLetter.N007Broken, record.MapId, false);
            }
            else if (outcome == SuiyinN007Outcome.AllDead)
            {
                Queue(SuiyinLetter.N007AllDead, record.MapId, false);
            }
            else if (outcome == SuiyinN007Outcome.Missing)
            {
                Queue(SuiyinLetter.N007Release, record.MapId, false);
            }

            TryAsides(tick);
            return true;
        }

        internal bool ClaimReturn(SuiyinN007Case record, int tick, bool alive)
        {
            if (record == null || record.ReturnDone || record.ReturnDueTick < 0 || tick < record.ReturnDueTick || record.ReturnPawnId <= 0)
            {
                return false;
            }

            record.ReturnDone = true;
            if (!alive)
            {
                return false;
            }

            Queue(SuiyinLetter.N007Return, record.ReturnPawnId, false);
            return true;
        }

        internal bool ArriveEnvoy(int mapId, int pawnId, int tick)
        {
            if (!Allows(SuiyinNode.N008) || pawnId <= 0 || FindN008() != null)
            {
                return false;
            }

            if (Distinct < Config.EnvoyKinds && !EnvoyClue)
            {
                return false;
            }

            N008.Add(new SuiyinN008Case
            {
                MapId = mapId,
                PawnId = pawnId,
                StartedTick = tick,
                Deadline = tick + Days(Config.EnvoyWaitDays),
                Outcome = SuiyinN008Outcome.Waiting
            });
            Queue(SuiyinLetter.N008Arrive, pawnId, false);
            return true;
        }

        internal bool ChooseEnvoy(SuiyinN008Case record, SuiyinN008Action action, int tick)
        {
            if (record == null || record.Presence == SuiyinPresence.Dead)
            {
                return false;
            }

            if (action == SuiyinN008Action.Trade && record.Outcome == SuiyinN008Outcome.Waiting)
            {
                if (!record.MealsReady || record.Outcome == SuiyinN008Outcome.Traded)
                {
                    return false;
                }

                record.Outcome = SuiyinN008Outcome.Traded;
                RelicClue = true;
                Queue(SuiyinLetter.N008Trade, record.PawnId, false);
                TryRelic(tick);
                TryAsides(tick);
                return true;
            }

            if (action == SuiyinN008Action.Proof && record.Outcome == SuiyinN008Outcome.Waiting)
            {
                record.CheckUntil = tick + Days(Config.EnvoyCheckDays);
                if (!record.ProofAvailable)
                {
                    record.Outcome = SuiyinN008Outcome.NoProof;
                    Queue(SuiyinLetter.N008NoProof, record.PawnId, false);
                    return true;
                }

                record.Outcome = SuiyinN008Outcome.Checking;
                Queue(SuiyinLetter.N008Proof, Distinct, false);
                return true;
            }

            if (action == SuiyinN008Action.Refuse && record.Outcome == SuiyinN008Outcome.Waiting)
            {
                record.Outcome = SuiyinN008Outcome.Refused;
                Queue(SuiyinLetter.N008Refuse, record.PawnId, false);
                return true;
            }

            if (action == SuiyinN008Action.Drive && record.Outcome == SuiyinN008Outcome.Waiting)
            {
                record.Outcome = SuiyinN008Outcome.Driven;
                Trust = SuiyinNodes.ClampTrust(Trust - 2);
                Queue(SuiyinLetter.N008Drive, record.PawnId, false);
                return true;
            }

            return false;
        }

        internal bool FinishEnvoy(SuiyinN008Case record, int tick)
        {
            if (record == null || record.Outcome == SuiyinN008Outcome.Dead || record.Outcome == SuiyinN008Outcome.Left || record.Outcome == SuiyinN008Outcome.Missing || record.Outcome == SuiyinN008Outcome.TimedOut)
            {
                return false;
            }

            if (record.Presence == SuiyinPresence.Dead)
            {
                record.Outcome = SuiyinN008Outcome.Dead;
                Queue(SuiyinLetter.N008Dead, record.PawnId, false);
                return true;
            }

            if (record.Presence == SuiyinPresence.Left)
            {
                record.Outcome = SuiyinN008Outcome.Left;
                Queue(SuiyinLetter.N008Left, record.PawnId, false);
                return true;
            }

            if (record.Presence == SuiyinPresence.Unknown)
            {
                if (record.MissingSince < 0)
                {
                    record.MissingSince = tick;
                }

                if (tick - record.MissingSince >= Days(Config.MissingDays))
                {
                    record.Presence = SuiyinPresence.Missing;
                    record.Outcome = SuiyinN008Outcome.Missing;
                    Queue(SuiyinLetter.N008Missing, record.PawnId, false);
                    return true;
                }
            }

            if (record.Outcome == SuiyinN008Outcome.Waiting && record.Deadline >= 0 && tick >= record.Deadline)
            {
                record.Outcome = SuiyinN008Outcome.TimedOut;
                Queue(SuiyinLetter.N008Timeout, record.PawnId, false);
                return true;
            }

            return false;
        }

        internal bool OpenRelic(int tick)
        {
            if (N009 != null || !Allows(SuiyinNode.N009))
            {
                return false;
            }

            N009 = new SuiyinN009Case
            {
                StartedTick = tick,
                Deadline = tick + Days(Config.RelicDays)
            };
            Queue(SuiyinLetter.N009Arrive, 0, false);
            return true;
        }

        internal int ChooseRelic(SuiyinN009Action action, int tick)
        {
            if (N009 == null || N009.Outcome != SuiyinN009Outcome.Pending)
            {
                return 0;
            }

            if ((action == SuiyinN009Action.Hand || action == SuiyinN009Action.Share) && !N009.EnvoyHere)
            {
                return 0;
            }

            int silver = 0;
            if (action == SuiyinN009Action.Take)
            {
                N009.Outcome = SuiyinN009Outcome.Taken;
                silver = Scaled(Config.RelicTakeSilver);
                Queue(SuiyinLetter.N009Take, silver, true);
            }
            else if (action == SuiyinN009Action.Leave)
            {
                N009.Outcome = SuiyinN009Outcome.Left;
                Queue(SuiyinLetter.N009Leave, 0, false);
            }
            else if (action == SuiyinN009Action.Hand)
            {
                N009.Outcome = SuiyinN009Outcome.Handed;
                silver = Scaled(Config.RelicHandSilver);
                Queue(SuiyinLetter.N009Hand, silver, false);
            }
            else if (action == SuiyinN009Action.Share)
            {
                N009.Outcome = SuiyinN009Outcome.Shared;
                Queue(SuiyinLetter.N009Share, 0, false);
            }
            else if (action == SuiyinN009Action.Destroy || N009.BoxDestroyed)
            {
                N009.Outcome = SuiyinN009Outcome.Destroyed;
                Trust = SuiyinNodes.ClampTrust(Trust - 2);
                Queue(SuiyinLetter.N009Destroy, 0, false);
            }
            else
            {
                return 0;
            }

            TryAsides(tick);
            return silver;
        }

        internal bool ExpireRelic(int tick)
        {
            if (N009 == null || N009.Outcome != SuiyinN009Outcome.Pending)
            {
                return false;
            }

            if (N009.PlayersInside)
            {
                return false;
            }

            if (N009.BoxDestroyed)
            {
                ChooseRelic(SuiyinN009Action.Destroy, tick);
                return true;
            }

            if (!N009.MapPresent || (N009.Deadline >= 0 && tick >= N009.Deadline))
            {
                N009.Outcome = SuiyinN009Outcome.Empty;
                Queue(SuiyinLetter.N009Empty, 0, false);
                return true;
            }

            return false;
        }

        internal bool OpenJournal(int journal, int mapId, int batchId, int tick, int people, bool delivered)
        {
            if (journal < 1 || journal > 14 || batchId <= 0 || FindJournal(batchId) != null)
            {
                return false;
            }

            SuiyinJournalCase record = new SuiyinJournalCase
            {
                Id = journal,
                MapId = mapId,
                BatchId = batchId,
                StartedTick = tick,
                Delivered = delivered
            };
            for (int i = 0; i < people; i++)
            {
                record.People.Add(new SuiyinMember { LoadId = batchId * 100 + i + 1 });
            }

            Journals.Add(record);
            if (!JournalNoted.Contains(journal))
            {
                JournalNoted.Add(journal);
                Queue(SuiyinLetter.Journal, journal, false);
            }

            return true;
        }

        internal bool CloseJournal(SuiyinJournalCase record, int tick)
        {
            if (record == null || record.Closed || record.Counted)
            {
                return false;
            }

            int left = 0;
            int settled = 0;
            int pending = 0;
            for (int i = 0; i < record.People.Count; i++)
            {
                SuiyinMember person = record.People[i];
                if (person.Presence == SuiyinPresence.Unknown)
                {
                    if (person.MissingSince < 0)
                    {
                        person.MissingSince = tick;
                    }

                    if (tick - person.MissingSince >= Days(Config.MissingDays))
                    {
                        person.Presence = SuiyinPresence.Missing;
                    }
                }

                if (person.Presence == SuiyinPresence.Here && person.Care != SuiyinCare.Hungry && person.Care != SuiyinCare.Plague)
                {
                    if (tick - record.StartedTick >= Days(Config.CareDays))
                    {
                        settled++;
                    }
                    else
                    {
                        pending++;
                    }
                }
                else if (person.Presence == SuiyinPresence.Left)
                {
                    left++;
                }
                else if (person.Presence == SuiyinPresence.Here || person.Presence == SuiyinPresence.Unknown)
                {
                    pending++;
                }
            }

            if (pending > 0 && tick - record.StartedTick < Days(Config.ObserveDays))
            {
                return false;
            }

            record.Closed = true;
            bool aid = record.People.Count > 0 && !record.Driven && left + settled == record.People.Count && (record.Delivered || settled == record.People.Count);
            record.Counted = aid;
            return aid;
        }

        internal int TakeNotices(List<SuiyinNotice> destination)
        {
            if (destination == null)
            {
                return 0;
            }

            int count = Pending.Count;
            for (int i = 0; i < Pending.Count; i++)
            {
                destination.Add(Pending[i]);
            }

            Pending.Clear();
            return count;
        }

        static bool Catalog(string displayId)
        {
            if (displayId == null || displayId.Length != 5 || displayId[0] != 'I' || displayId[1] != '-')
            {
                return false;
            }

            int number = 0;
            for (int i = 2; i < 5; i++)
            {
                if (displayId[i] < '0' || displayId[i] > '9')
                {
                    return false;
                }

                number = number * 10 + (displayId[i] - '0');
            }

            return number >= 1 && number <= 51;
        }

        internal int Scaled(int silver)
        {
            if (silver <= 0 || Trust <= 0)
            {
                return silver < 0 ? 0 : silver;
            }

            int bonus = Trust > 100 ? 25 : Trust * 25 / 100;
            return silver + silver * bonus / 100;
        }

        internal static int JournalFor(string displayId)
        {
            if (string.IsNullOrEmpty(displayId))
            {
                return 0;
            }

            if (displayId == "I-032" || displayId == "I-046") return 1;
            if (displayId == "I-033" || displayId == "I-047") return 2;
            if (displayId == "I-029" || displayId == "I-044") return 3;
            if (displayId == "I-031" || displayId == "I-039") return 4;
            if (displayId == "I-006" || displayId == "I-007" || displayId == "I-043") return 5;
            if (displayId == "I-001") return 6;
            if (displayId.StartsWith("I-02") && displayId != "I-029") return 7;
            if (displayId == "I-012" || displayId == "I-038") return 8;
            if (displayId == "I-014" || displayId == "I-030" || displayId == "I-045") return 9;
            if (displayId == "I-034" || displayId == "I-048") return 10;
            if (displayId == "I-049") return 11;
            if (displayId == "I-035" || displayId == "I-050") return 12;
            if (displayId == "I-008" || displayId == "I-009" || displayId == "I-010" || displayId == "I-036" || displayId == "I-041") return 13;
            if (displayId == "I-015" || displayId == "I-016" || displayId == "I-017" || displayId == "I-018" || displayId == "I-019" ||
                displayId == "I-004" || displayId == "I-005" || displayId == "I-011" || displayId == "I-040" || displayId == "I-042") return 14;
            return 0;
        }

        internal static bool IsTheft(string displayId)
        {
            return displayId == "I-006" || displayId == "I-007" || displayId == "I-043";
        }

        static int TrustFor(SuiyinN004Outcome outcome)
        {
            if (outcome == SuiyinN004Outcome.ChildAlone || outcome == SuiyinN004Outcome.FamilyHere) return 5;
            if (outcome == SuiyinN004Outcome.Captive) return -5;
            if (outcome == SuiyinN004Outcome.Story) return -2;
            if (outcome == SuiyinN004Outcome.Regret) return -3;
            if (outcome == SuiyinN004Outcome.Banished) return -1;
            return 0;
        }

        static SuiyinLetter LetterFor(SuiyinN004Outcome outcome)
        {
            if (outcome == SuiyinN004Outcome.ChildAlone) return SuiyinLetter.N004ChildAlone;
            if (outcome == SuiyinN004Outcome.FamilyHere) return SuiyinLetter.N004FamilyHere;
            if (outcome == SuiyinN004Outcome.Captive) return SuiyinLetter.N004Captive;
            if (outcome == SuiyinN004Outcome.Story) return SuiyinLetter.N004Story;
            if (outcome == SuiyinN004Outcome.Banished) return SuiyinLetter.N004Banished;
            if (outcome == SuiyinN004Outcome.Regret) return SuiyinLetter.N004Regret;
            return SuiyinLetter.None;
        }

        void TryEnvoy(int tick)
        {
        }

        void TryRelic(int tick)
        {
            if (N009 != null || !Allows(SuiyinNode.N009))
            {
                return;
            }

            if (Distinct >= Config.RelicKinds || RelicClue)
            {
                OpenRelic(tick);
            }
        }

        void TryAsides(int tick)
        {
            if (!Narrator || Trust <= Config.TrustAsideCutoff)
            {
                return;
            }

            if (LastAsideTick >= 0 && tick - LastAsideTick < Days(Config.AsideCooldownDays))
            {
                return;
            }

            SuiyinAside next = SuiyinAside.None;
            if (HasOutcomeN004() && HasN005() && !AsidesSent.Contains(1)) next = SuiyinAside.ChildToExchange;
            else if (HasTracedHole() && N009 != null && !AsidesSent.Contains(2)) next = SuiyinAside.GranaryToRelic;
            else if (HasRecoveredLeave() && FindN008() != null && !AsidesSent.Contains(3)) next = SuiyinAside.QuarantineToEnvoy;
            else if (HasTradedEnvoy() && N009 != null && N009.Outcome != SuiyinN009Outcome.Pending && N009.Outcome != SuiyinN009Outcome.Empty && !AsidesSent.Contains(4)) next = SuiyinAside.EnvoyToRelic;
            if (next == SuiyinAside.None)
            {
                return;
            }

            AsidesSent.Add((int)next);
            LastAsideTick = tick;
            Queue(SuiyinLetter.Aside, (int)next, true);
        }
        internal void TryAsideForTest(int tick)
        {
            TryAsides(tick);
        }

        bool HasOutcomeN004()
        {
            for (int i = 0; i < N004.Count; i++)
            {
                if (N004[i].Outcome != SuiyinN004Outcome.Pending && N004[i].Outcome != SuiyinN004Outcome.Unknown)
                {
                    return true;
                }
            }

            return false;
        }

        bool HasN005()
        {
            return N005.Count > 0;
        }

        bool HasTracedHole()
        {
            for (int i = 0; i < N006.Count; i++)
            {
                if (N006[i].Outcome == SuiyinN006Outcome.Traced)
                {
                    return true;
                }
            }

            return false;
        }

        bool HasRecoveredLeave()
        {
            for (int i = 0; i < N007.Count; i++)
            {
                if (N007[i].Outcome == SuiyinN007Outcome.RecoveredLeft)
                {
                    return true;
                }
            }

            return false;
        }

        bool HasTradedEnvoy()
        {
            SuiyinN008Case envoy = FindN008();
            return envoy != null && envoy.Outcome == SuiyinN008Outcome.Traded;
        }

        bool HasRecovered(SuiyinN007Case record)
        {
            for (int i = 0; i < record.Visitors.Count; i++)
            {
                if (record.Visitors[i].Presence == SuiyinPresence.Here && record.Visitors[i].Care != SuiyinCare.Plague)
                {
                    return true;
                }
            }

            return false;
        }

        static bool AllDead(SuiyinN007Case record)
        {
            if (record.Visitors.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < record.Visitors.Count; i++)
            {
                if (record.Visitors[i].Presence != SuiyinPresence.Dead)
                {
                    return false;
                }
            }

            return true;
        }

        SuiyinN005Case FindN005(int id)
        {
            for (int i = 0; i < N005.Count; i++)
            {
                if (N005[i].Id == id)
                {
                    return N005[i];
                }
            }

            return null;
        }

        SuiyinN006Case FindN006(int mapId)
        {
            for (int i = 0; i < N006.Count; i++)
            {
                if (N006[i].MapId == mapId)
                {
                    return N006[i];
                }
            }

            return null;
        }

        SuiyinN007Case OpenN007(int mapId)
        {
            for (int i = 0; i < N007.Count; i++)
            {
                if (N007[i].MapId == mapId && N007[i].Outcome == SuiyinN007Outcome.Pending)
                {
                    return N007[i];
                }
            }

            return null;
        }

        SuiyinN008Case FindN008()
        {
            return N008.Count == 0 ? null : N008[0];
        }

        SuiyinJournalCase FindJournal(int batchId)
        {
            for (int i = 0; i < Journals.Count; i++)
            {
                if (Journals[i].BatchId == batchId)
                {
                    return Journals[i];
                }
            }

            return null;
        }

        bool Has(SuiyinLetter letter)
        {
            for (int i = 0; i < Pending.Count; i++)
            {
                if (Pending[i].Letter == letter)
                {
                    return true;
                }
            }

            return false;
        }

        bool Allows(SuiyinNode node)
        {
            if (Gates == null)
            {
                return true;
            }

            return Gates.Enabled(node) || Gates.Started(node);
        }

        void Queue(SuiyinLetter letter, int arg, bool isPrivate)
        {
            if (letter == SuiyinLetter.None)
            {
                return;
            }

            if (isPrivate && !Narrator)
            {
                return;
            }

            for (int i = 0; i < Pending.Count; i++)
            {
                if (Pending[i].Letter == letter && Pending[i].Arg == arg)
                {
                    return;
                }
            }

            Pending.Add(new SuiyinNotice(letter, arg, isPrivate));
        }

        static int Days(int days)
        {
            return days * 60000;
        }

        static int Years(int years)
        {
            return years * 3600000;
        }
    }
}
