using HungerAndHavoc.Narrative;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class SuiyinLedgerTests
    {
        [Fact]
        public void Nodes_DefaultOpenUntilDisabled()
        {
            SuiyinLedger ledger = new SuiyinLedger();
            Assert.True(ledger.Allows(SuiyinNode.N007, 10));
            ledger.SetEnabled(SuiyinNode.N007, false);
            Assert.False(ledger.Allows(SuiyinNode.N007, 10));
            Assert.True(ledger.Allows(SuiyinNode.N001, 10));
        }

        [Fact]
        public void StartedNode_KeepsSavedDeadlineAfterSwitchOff()
        {
            SuiyinLedger ledger = new SuiyinLedger();
            ledger.Start(SuiyinNode.N008, 100);
            ledger.SetEnabled(SuiyinNode.N008, false);
            ledger.Start(SuiyinNode.N008, 20);

            Assert.True(ledger.Allows(SuiyinNode.N008, 100));
            Assert.False(ledger.Allows(SuiyinNode.N008, 101));
            Assert.Equal(100, ledger.Deadline(SuiyinNode.N008));
        }

        [Fact]
        public void CopyFrom_FillsMissingNodesWithoutDroppingOld()
        {
            SuiyinLedger ledger = new SuiyinLedger();
            ledger.Import(
                new System.Collections.Generic.List<bool> { false },
                new System.Collections.Generic.List<bool> { true },
                new System.Collections.Generic.List<int> { 40 });
            Assert.False(ledger.Enabled(SuiyinNode.N001));
            Assert.True(ledger.Started(SuiyinNode.N001));
            Assert.Equal(40, ledger.Deadline(SuiyinNode.N001));
            Assert.True(ledger.Enabled(SuiyinNode.R01));
            Assert.False(ledger.Started(SuiyinNode.R01));
            Assert.Equal(-1, ledger.Deadline(SuiyinNode.R01));
        }

        [Fact]
        public void Trust_ClampsToLedgerRange()
        {
            Assert.Equal(-100, SuiyinNodes.ClampTrust(-140));
            Assert.Equal(100, SuiyinNodes.ClampTrust(140));
            Assert.Equal(0, SuiyinNodes.ClampTrust(0));
        }

        [Fact]
        public void Facts_DoNotImplyLetters()
        {
            SuiyinIncidentFact incident = new SuiyinIncidentFact("I-042", 3, 20, 900, 2, true, new[] { 7 });
            SuiyinPlagueFact plague = new SuiyinPlagueFact(3, 1, 1);
            Assert.Equal("I-042", incident.DisplayId);
            Assert.Equal(20, incident.Tick);
            Assert.Equal(900, incident.BatchId);
            Assert.Equal(7, incident.VisitorIds[0]);
            Assert.Equal(1, plague.Recovered);
            Assert.Equal(1, plague.Died);
        }
    }
}
