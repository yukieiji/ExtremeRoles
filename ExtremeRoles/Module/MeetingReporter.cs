using System.Collections.Generic;
using System.Text;

using Hazel;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Module;

public sealed class MeetingReporter : NullableSingleton<MeetingReporter>
{
    public bool HasChatReport => this.chatReport.Count > 0;

    private HashSet<string> addedReport = new HashSet<string>();
    private StringBuilder exilReporter = new StringBuilder();
    private StringBuilder startReporter = new StringBuilder();

    private List<(string, bool)> chatReport = new List<(string, bool)>();

    public enum RpcOpType : byte
    {
        ChatReport
    }

    public MeetingReporter()
    {
        this.addedReport.Clear();
        this.startReporter.Clear();
        this.chatReport.Clear();
        this.exilReporter.Clear();
    }

    public static void RpcAddMeetingChatReport(string report)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.MeetingReporterRpc))
        {
            caller.WriteByte((byte)RpcOpType.ChatReport);
            caller.WriteStr(report);
        }
        Instance.AddMeetingChatReport(report);
    }

    public static void RpcOp(ref MessageReader reader)
    {
        RpcOpType ops = (RpcOpType)reader.ReadByte();
        string report = reader.ReadString();
        
        switch (ops)
        {
            case RpcOpType.ChatReport:
                Instance.AddMeetingChatReport(report);
                break;
        }
    }

    public void AddMeetingStartReport(string report)
    {
        if (this.addedReport.Add(report))
        {
            this.startReporter.AppendLine(report);
        }
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
