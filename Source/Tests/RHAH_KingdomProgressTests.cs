using HungerAndHavoc.Storyteller.Suiyin;
using Xunit;

namespace HungerAndHavoc.Tests
{
    public class RHAH_KingdomProgressTests
    {
        [Theory]
        [InlineData(0, 3)]
        [InlineData(2, 2)]
        [InlineData(5, 1)]
        public void RepairUsesActualRecipients(int delivered, int outcome)
        {
            KingdomArcProgress progress = new KingdomArcProgress { Route = KingdomRoute.Repair, Delivered = delivered };
            progress.Finish(KingdomFailure.Expired);
            Assert.Equal((KingdomOutcome)outcome, progress.Outcome);
            Assert.Single(progress.RouteHistory);
            Assert.Equal(KingdomStage.Response, progress.Stage);
        }

        [Fact]
        public void PauseDoesNotInheritEarlierProtectSuccess()
        {
            KingdomArcProgress progress = new KingdomArcProgress { Route = KingdomRoute.Protect, Outcome = KingdomOutcome.Success };
            progress.Finish(KingdomFailure.None);
            progress.Route = KingdomRoute.Pause;
            progress.Finish(KingdomFailure.Cancelled);
            Assert.Equal(KingdomOutcome.Failed, progress.Outcome);
            Assert.Equal(2, progress.RouteHistory.Count);
        }

        [Fact]
        public void ChapterTransitionResetsCursorButKeepsDeliveredFacts()
        {
            KingdomArcProgress progress = new KingdomArcProgress { Cursor = 3, Delivered = 2, ConsentSafeSiteId = 19, SafeSiteId = 24 };
            progress.Move(KingdomStage.Routes);
            Assert.Equal(0, progress.Cursor);
            Assert.Equal(2, progress.Delivered);
            Assert.Equal(19, progress.ConsentSafeSiteId);
            Assert.Equal(24, progress.SafeSiteId);
        }
    }
}
