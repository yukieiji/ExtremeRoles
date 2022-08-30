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

        public void ResetOnMeeting()
        {
            resetGhostAbilityReport();
            resetGlobalAction();
        }

        public string GetAditionalInfo()
        {
            return this.getRoleAditionalInfo();
        }

        public bool IsShowAditionalInfo()
        {
            return this.isShowRoleAditionalInfo();
        }

        private void resetMeetingCount()
        {
            meetingCount = 0;
        }
    }
}
