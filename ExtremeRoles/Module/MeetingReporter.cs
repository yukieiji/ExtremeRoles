using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module
{
    public sealed class MeetingReporter : NullableSingleton<MeetingReporter>
    {
        public bool HasChatReport => this.chatReport.Count > 0;

        private StringBuilder exilReporter = new StringBuilder();
        private StringBuilder startReporter = new StringBuilder();

        private List<(string, bool)> chatReport = new List<(string, bool)>();

        public MeetingReporter()
        {
            this.startReporter.Clear();
            this.chatReport.Clear();
            this.exilReporter.Clear();
        }

        public void AddMeetingStartReport(string report)
        {
            this.startReporter.AppendLine(report);
        }

        public void AddMeetingChatReport(string report, bool isRpc = false)
        {
            this.chatReport.Add((report, isRpc));
        }

        public void AddMeetingEndReport(string report)
        {
            this.exilReporter.AppendLine(report);
        }

        public string GetMeetingStartReport() => this.startReporter.ToString();

        public string GetMeetingEndReport() => this.exilReporter.ToString();

        public void ReportMeetingChat()
        {

            PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;

            foreach (var (report, isRpc) in this.chatReport)
            {
                if (isRpc)
                {
                    localPlayer.RpcSendChat(report);
                }
                else
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                        localPlayer, report);
                }
            }

            this.chatReport.Clear();
        }
    }
}
