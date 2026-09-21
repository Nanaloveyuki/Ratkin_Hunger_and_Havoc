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
