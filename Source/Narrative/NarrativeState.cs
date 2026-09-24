using System.Collections.Generic;
using Verse;

namespace HungerAndHavoc.Narrative
{
    public enum NarrativeOutcome
    {
        Unresolved = 0,
        Rescue = 1,
        Loss = 2,
        Exposure = 3,
        Failure = 4,
        Massacre = 5
    }

    public sealed class NarrativeSnapshot
    {
        public int RevealedCount { get; }
        public int Trust { get; }
        public int Rescued { get; }
        public int Lost { get; }
        public bool Failed { get; }

        public NarrativeSnapshot(int revealedCount, int trust, int rescued, int lost, bool failed)
        {
            RevealedCount = revealedCount;
            Trust = trust;
            Rescued = rescued;
            Lost = lost;
            Failed = failed;
        }
    }

    public sealed class NarrativeState : GameComponent
    {
        int revealedCount;
        int trust;
        int rescued;
        int lost;
        bool failed;
        List<bool> suiyinEnabled = new List<bool>();
        List<bool> suiyinStarted = new List<bool>();
        List<int> suiyinDeadlineTick = new List<int>();
        readonly SuiyinLedger suiyin = new SuiyinLedger();
        readonly SuiyinBook book = new SuiyinBook();
        List<string> seenKinds = new List<string>();
        List<int> theftMaps = new List<int>();
        List<int> theftCounts = new List<int>();
        List<int> journalNoted = new List<int>();
        List<int> asidesSent = new List<int>();
        bool openingSent;
        bool progressSent;
        bool rewardClaimed;
        int rewardPaid;
        bool envoyClue;
        bool relicClue;
        int lastAsideTick = -1;
        int nextCaseId = 1;
        int aidCount;
        int broadcastCount;
        int expulsionCount;
        int adultCount;
        int completedKindCount;
        int firstFactTick = -1;
        int nextAdultCheckTick = -1;
        bool relicDone;
        bool endingE01;
        bool endingE02;
        bool endingE03;
        bool endingE04;
        bool endingE05;
        int identityTier;
        bool identityRefused;

        public NarrativeState(Game game)
        {
        }

        public void RecordRevelation()
        {
            revealedCount++;
        }

        public void RecordTrust(int amount)
        {
            trust += amount;
        }

        public void RecordRescue()
        {
            rescued++;
        }

        public void RecordLoss()
        {
            lost++;
        }

        public void MarkFailure()
        {
            failed = true;
        }
        internal bool SuiyinAllows(SuiyinNode node, int tick)
        {
            return suiyin.Allows(node, tick);
        }

        internal bool SuiyinEnabled(SuiyinNode node)
        {
            return suiyin.Enabled(node);
        }

        internal void SetSuiyinEnabled(SuiyinNode node, bool enabled)
        {
            suiyin.SetEnabled(node, enabled);
        }

        internal void StartSuiyin(SuiyinNode node, int deadlineTick)
        {
            suiyin.Start(node, deadlineTick);
        }

        internal void NoteIncident(SuiyinIncidentFact fact)
        {
            if (fact.DisplayId == null)
            {
                return;
            }

            book.Gates = suiyin;
            book.Trust = trust;
            book.Note(fact.DisplayId, fact.MapId, fact.Tick, fact.CarriesPlague, true);
            trust = book.Trust;
            if (!string.IsNullOrEmpty(fact.DisplayId) && !seenKinds.Contains(fact.DisplayId))
            {
                revealedCount = book.Distinct;
            }
        }

        internal void NotePlague(SuiyinPlagueFact fact)
        {
            book.Gates = suiyin;
            book.Trust = trust;
            book.NotePlague(fact.MapId, fact.Recovered, fact.Died);
            trust = book.Trust;
        }
        internal int AidCount => aidCount;
        internal int BroadcastCount => broadcastCount;
        internal int ExpulsionCount => expulsionCount;
        internal int AdultCount => adultCount;
        internal int CompletedKindCount => completedKindCount;
        internal bool RelicDone => relicDone;
        internal bool EndingShown(RHAH_EndingId id)
        {
            return RHAH_EndingRules.Shown(EndingFacts(0, true), id);
        }

        internal bool AsidesClosed => RHAH_EndingRules.AsidesClosed(trust, endingE05);

        internal void NoteAid(int tick)
        {
            aidCount++;
            NoteFirstFact(tick);
        }

        internal void NoteBroadcast(int tick)
        {
            broadcastCount++;
            NoteFirstFact(tick);
        }

        internal void NoteExpulsion(int tick)
        {
            expulsionCount++;
            NoteFirstFact(tick);
        }

        internal void NoteCompletedKind(int tick)
        {
            completedKindCount++;
            NoteFirstFact(tick);
        }

        internal void NoteRelicDone(int tick)
        {
            relicDone = true;
            NoteFirstFact(tick);
        }

        internal void SetAdultCount(int count, int tick)
        {
            adultCount = count < 0 ? 0 : count;
            nextAdultCheckTick = tick + 3600000;
        }

        internal bool AdultCheckDue(int tick)
        {
            return nextAdultCheckTick < 0 || tick >= nextAdultCheckTick;
        }

        internal RHAH_EndingFacts EndingFacts(int tick, bool narrator)
        {
            int waited = 0;
            if (firstFactTick >= 0 && tick >= firstFactTick)
            {
                waited = (tick - firstFactTick) / 60000;
            }

            return new RHAH_EndingFacts(
                trust,
                narrator,
                aidCount,
                broadcastCount,
                expulsionCount,
                adultCount,
                completedKindCount,
                relicDone,
                waited,
                endingE01,
                endingE02,
                endingE03,
                endingE04,
                endingE05,
                identityTier);
        }

        internal RHAH_EndingId PendingEnding(int tick, bool narrator, RHAH_EndingGoals goals)
        {
            return RHAH_EndingRules.Next(EndingFacts(tick, narrator), goals);
        }

        internal void MarkEnding(RHAH_EndingId id)
        {
            if (id == RHAH_EndingId.E01)
            {
                endingE01 = true;
            }
            else if (id == RHAH_EndingId.E02)
            {
                endingE02 = true;
            }
            else if (id == RHAH_EndingId.E03)
            {
                endingE03 = true;
            }
            else if (id == RHAH_EndingId.E04)
            {
                endingE04 = true;
            }
            else if (id == RHAH_EndingId.E05)
            {
                endingE05 = true;
            }
        }

        internal bool IdentityDue(int tick, bool narrator, RHAH_EndingGoals goals)
        {
            return !identityRefused && RHAH_EndingRules.IdentityDue(EndingFacts(tick, narrator), goals);
        }

        internal void MarkIdentity(RHAH_IdentityTier tier, bool refused)
        {
            if ((int)tier > identityTier)
            {
                identityTier = (int)tier;
            }

            if (refused)
            {
                identityRefused = true;
            }
        }

        void NoteFirstFact(int tick)
        {
            if (firstFactTick < 0 || (tick >= 0 && tick < firstFactTick))
            {
                firstFactTick = tick;
            }
        }


        internal SuiyinBook Book => book;


        public NarrativeSnapshot Snapshot()
        {
            return new NarrativeSnapshot(revealedCount, trust, rescued, lost, failed);
        }

        public NarrativeOutcome CalculateOutcome()
        {
            return NarrativeOutcomeCalculator.Calculate(Snapshot());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref revealedCount, "revealedCount", 0);
            Scribe_Values.Look(ref trust, "trust", 0);
            Scribe_Values.Look(ref rescued, "rescued", 0);
            Scribe_Values.Look(ref lost, "lost", 0);
            Scribe_Values.Look(ref failed, "failed", false);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                book.Trust = trust;
                book.ExportSave(seenKinds, theftMaps, theftCounts, journalNoted, asidesSent);
                openingSent = book.OpeningSent;
                progressSent = book.ProgressSent;
                rewardClaimed = book.RewardClaimed;
                rewardPaid = book.RewardPaid;
                envoyClue = book.EnvoyClue;
                relicClue = book.RelicClue;
                lastAsideTick = book.LastAsideTick;
                nextCaseId = book.NextCaseId;
                suiyin.Export(suiyinEnabled, suiyinStarted, suiyinDeadlineTick);
            }

            Scribe_Collections.Look(ref seenKinds, "seenKinds", LookMode.Value);
            Scribe_Collections.Look(ref theftMaps, "theftMaps", LookMode.Value);
            Scribe_Collections.Look(ref theftCounts, "theftCounts", LookMode.Value);
            Scribe_Collections.Look(ref journalNoted, "journalNoted", LookMode.Value);
            Scribe_Collections.Look(ref asidesSent, "asidesSent", LookMode.Value);
            Scribe_Values.Look(ref openingSent, "openingSent", false);
            Scribe_Values.Look(ref progressSent, "progressSent", false);
            Scribe_Values.Look(ref rewardClaimed, "rewardClaimed", false);
            Scribe_Values.Look(ref rewardPaid, "rewardPaid", 0);
            Scribe_Values.Look(ref envoyClue, "envoyClue", false);
            Scribe_Values.Look(ref relicClue, "relicClue", false);
            Scribe_Values.Look(ref lastAsideTick, "lastAsideTick", -1);
            Scribe_Values.Look(ref nextCaseId, "nextCaseId", 1);
            Scribe_Values.Look(ref aidCount, "aidCount", 0);
            Scribe_Values.Look(ref broadcastCount, "broadcastCount", 0);
            Scribe_Values.Look(ref expulsionCount, "expulsionCount", 0);
            Scribe_Values.Look(ref adultCount, "adultCount", 0);
            Scribe_Values.Look(ref completedKindCount, "completedKindCount", 0);
            Scribe_Values.Look(ref firstFactTick, "firstFactTick", -1);
            Scribe_Values.Look(ref nextAdultCheckTick, "nextAdultCheckTick", -1);
            Scribe_Values.Look(ref relicDone, "relicDone", false);
            Scribe_Values.Look(ref endingE01, "endingE01", false);
            Scribe_Values.Look(ref endingE02, "endingE02", false);
            Scribe_Values.Look(ref endingE03, "endingE03", false);
            Scribe_Values.Look(ref endingE04, "endingE04", false);
            Scribe_Values.Look(ref endingE05, "endingE05", false);
            Scribe_Values.Look(ref identityTier, "identityTier", 0);
            Scribe_Values.Look(ref identityRefused, "identityRefused", false);

            Scribe_Collections.Look(ref suiyinEnabled, "suiyinEnabled", LookMode.Value);
            Scribe_Collections.Look(ref suiyinStarted, "suiyinStarted", LookMode.Value);
            Scribe_Collections.Look(ref suiyinDeadlineTick, "suiyinDeadlineTick", LookMode.Value);
            Scribe_Collections.Look(ref book.N004, "entrustCases", LookMode.Deep);
            Scribe_Collections.Look(ref book.N005, "exchangeCases", LookMode.Deep);
            Scribe_Collections.Look(ref book.N006, "holeCases", LookMode.Deep);
            Scribe_Collections.Look(ref book.N007, "quarantineCases", LookMode.Deep);
            Scribe_Collections.Look(ref book.N008, "envoyCases", LookMode.Deep);
            Scribe_Deep.Look(ref book.N009, "relicCase");
            Scribe_Collections.Look(ref book.Journals, "journalCases", LookMode.Deep);
            Scribe_Collections.Look(ref book.Pending, "pendingNotices", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                suiyinEnabled = suiyinEnabled ?? new List<bool>();
                theftMaps = theftMaps ?? new List<int>();
                theftCounts = theftCounts ?? new List<int>();
                journalNoted = journalNoted ?? new List<int>();
                asidesSent = asidesSent ?? new List<int>();
                suiyinStarted = suiyinStarted ?? new List<bool>();
                suiyinDeadlineTick = suiyinDeadlineTick ?? new List<int>();
                suiyin.Import(suiyinEnabled, suiyinStarted, suiyinDeadlineTick);
                book.Gates = suiyin;
                book.Trust = trust;
                book.Narrator = true;
                book.OpeningSent = openingSent;
                book.ProgressSent = progressSent;
                book.RewardClaimed = rewardClaimed;
                book.RewardPaid = rewardPaid;
                book.EnvoyClue = envoyClue;
                book.RelicClue = relicClue;
                book.LastAsideTick = lastAsideTick;
                book.NextCaseId = nextCaseId < 1 ? 1 : nextCaseId;
                book.N004 = book.N004 ?? new System.Collections.Generic.List<SuiyinN004Case>();
                book.N005 = book.N005 ?? new System.Collections.Generic.List<SuiyinN005Case>();
                book.N006 = book.N006 ?? new System.Collections.Generic.List<SuiyinN006Case>();
                book.N007 = book.N007 ?? new System.Collections.Generic.List<SuiyinN007Case>();
                book.N008 = book.N008 ?? new System.Collections.Generic.List<SuiyinN008Case>();
                book.Journals = book.Journals ?? new System.Collections.Generic.List<SuiyinJournalCase>();
                book.Pending = book.Pending ?? new System.Collections.Generic.List<SuiyinNotice>();
                book.ImportSave(seenKinds, theftMaps, theftCounts, journalNoted, asidesSent);
            }
        }
    }

    internal static class NarrativeOutcomeCalculator
    {
        internal static NarrativeOutcome Calculate(NarrativeSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Failed)
            {
                return NarrativeOutcome.Failure;
            }

            if (snapshot.Lost > snapshot.Rescued && snapshot.Lost > 0)
            {
                return NarrativeOutcome.Massacre;
            }

            if (snapshot.RevealedCount >= 10 && snapshot.Trust >= 5)
            {
                return NarrativeOutcome.Exposure;
            }

            if (snapshot.Rescued > 0 && snapshot.Trust >= 0)
            {
                return NarrativeOutcome.Rescue;
            }

            return NarrativeOutcome.Unresolved;
        }
    }
}
