namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public int MeetingCount => this.meetingCount;
        private int meetingCount = 0;

        public void IncreaseMeetingCount()
        {
            ++this.meetingCount;
        }

        private void resetMeetingCount()
        {
            meetingCount = 0;
        }
    }
}
