using Verse;

namespace HungerAndHavoc.EventMgr
{
    public sealed class RHAH_EventChainRecord : IExposable
    {
        public int id;
        public string displayId = "";
        public int siteId;
        public int stage;
        public int startedTick;
        public int deadlineTick = -1;
        public string payload = "";
        public bool closed;
        public int end = (int)RHAH_EventChainEnd.None;

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id", 0);
            Scribe_Values.Look(ref displayId, "displayId", "");
            Scribe_Values.Look(ref siteId, "siteId", 0);
            Scribe_Values.Look(ref stage, "stage", 0);
            Scribe_Values.Look(ref startedTick, "startedTick", 0);
            Scribe_Values.Look(ref deadlineTick, "deadlineTick", -1);
            Scribe_Values.Look(ref payload, "payload", "");
            Scribe_Values.Look(ref closed, "closed", false);
            Scribe_Values.Look(ref end, "end", 0);
        }

        internal void Repair()
        {
            if (displayId == null)
            {
                displayId = "";
            }

            if (payload == null)
            {
                payload = "";
            }

            if (deadlineTick < -1)
            {
                deadlineTick = -1;
            }
        }
    }
}
