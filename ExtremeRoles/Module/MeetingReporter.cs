using System.Collections.Generic;
using System.Text;

using Hazel;
using UnityEngine;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.Interface;


#nullable enable

namespace ExtremeRoles.Module;

public sealed class MeetingReporter : NullableSingleton<MeetingReporter>
{
	public bool HasChatReport => this.chatReport.Count > 0 || this.newChatReport.Count > 0;

	private HashSet<string> addedReport = new HashSet<string>();
	private StringBuilder exilReporter = new StringBuilder();
	private StringBuilder startReporter = new StringBuilder();

	private List<(string, bool)> chatReport = new List<(string, bool)>();
	private readonly Queue<IStringSerializer> newChatReport = new Queue<IStringSerializer>();
	private float waitTimer = 0.0f;

	public enum RpcOpType : byte
	{
		ChatSerializeDeserialize,
		ChatReport,
		TargetChatReport
	}

	public MeetingReporter()
	{
		this.addedReport.Clear();
		this.startReporter.Clear();
		this.chatReport.Clear();
		this.exilReporter.Clear();
	}

	public static void Reset()
	{
		if (IsExist)
		{
			Instance.Destroy();
		}
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

	public static void RpcAddTargetMeetingChatReport(byte targetPlayer, string report)
	{
		if (targetPlayer == CachedPlayerControl.LocalPlayer.PlayerId)
		{
			Instance.AddMeetingChatReport(report);
		}
		else
		{
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.MeetingReporterRpc))
			{
				caller.WriteByte((byte)RpcOpType.TargetChatReport);
				caller.WriteStr(report);
				caller.WriteByte(targetPlayer);
			}
		}
	}

	public static void RpcOp(ref MessageReader reader)
	{
		RpcOpType ops = (RpcOpType)reader.ReadByte();

		switch (ops)
		{
			case RpcOpType.ChatSerializeDeserialize:
				var serializer = IStringSerializer.DeserializeStatic(reader);
				// 無限共有が起きないようにRPCはここで無効化しておく
				serializer.IsRpc = false;
				Instance.AddMeetingChatReport(serializer);
				break;
			case RpcOpType.ChatReport:
				string report = reader.ReadString();
				Instance.AddMeetingChatReport(report);
				break;
			case RpcOpType.TargetChatReport:
				string targetChatReport = reader.ReadString();
				byte targetPlayer = reader.ReadByte();
				if (CachedPlayerControl.LocalPlayer.PlayerId != targetPlayer) { return; }
				Instance.AddMeetingChatReport(targetChatReport);
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

	public void AddMeetingChatReport(IStringSerializer serializer)
	{
		this.newChatReport.Enqueue(serializer);
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

		if (this.waitTimer > 0.0f)
		{
			this.waitTimer -= Time.deltaTime;
			return;
		}

		if (this.newChatReport.TryDequeue(out var serializer))
		{
			string chatBody = serializer.ToString();

			if (serializer.IsRpc)
			{
				using (var caller = RPCOperator.CreateCaller(
					RPCOperator.Command.MeetingReporterRpc))
				{
					caller.WriteByte((byte)RpcOpType.ChatSerializeDeserialize);
					caller.WriteByte((byte)serializer.Type);
					serializer.Serialize(caller);
				}
			}
			FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
				localPlayer, chatBody);

			this.resetWaitTimer();
		}
	}

	private void resetWaitTimer()
	{
		this.waitTimer = 1.0f;
	}
}
