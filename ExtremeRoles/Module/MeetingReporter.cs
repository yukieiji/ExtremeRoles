using System;
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
	public bool HasChatReport => this.chatReport.Count > 0;

	private sealed class NoRpcStringSerializer : IStringSerializer
	{
		public StringSerializerType Type => throw new NotImplementedException();

		public bool IsRpc { get; set; } = false;

		private readonly string chat;

		public NoRpcStringSerializer(string chatStr)
		{
			this.chat = chatStr;
		}

		public override string ToString() => this.chat;

		public void Deserialize(MessageReader reader)
		{
			throw new NotImplementedException();
		}
		public void Serialize(RPCOperator.RpcCaller caller)
		{
			throw new NotImplementedException();
		}
	}

	private readonly HashSet<string> addedReport = new HashSet<string>();
	private readonly StringBuilder exilReporter = new StringBuilder();
	private readonly StringBuilder startReporter = new StringBuilder();

	private readonly Queue<IStringSerializer> chatReport = new Queue<IStringSerializer>();
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

	public static void RpcAddTargetMeetingChatReport(byte targetPlayer, IStringSerializer report)
	{
		if (targetPlayer == PlayerControl.LocalPlayer.PlayerId)
		{
			Instance.AddMeetingChatReport(report);
		}
		else
		{
			using (var caller = RPCOperator.CreateCaller(
				RPCOperator.Command.MeetingReporterRpc))
			{
				caller.WriteByte((byte)RpcOpType.TargetChatReport);
				IStringSerializer.SerializeStatic(report, caller);
				caller.WriteByte(targetPlayer);
			}
		}
	}

	public static void RpcOp(ref MessageReader reader)
	{
		RpcOpType ops = (RpcOpType)reader.ReadByte();
		var serializer = IStringSerializer.DeserializeStatic(reader);
		// 無限共有が起きないようにRPCはここで無効化しておく
		serializer.IsRpc = false;

		switch (ops)
		{
			case RpcOpType.ChatSerializeDeserialize:
				break;
			case RpcOpType.TargetChatReport:
				byte targetPlayer = reader.ReadByte();
				var player = PlayerControl.LocalPlayer;
				if (player == null || player.PlayerId != targetPlayer)
				{
					return;
				}
				break;
			default:
				return;
		}
		Instance.AddMeetingChatReport(serializer);
	}

	public void AddMeetingStartReport(string report)
	{
		if (this.addedReport.Add(report))
		{
			this.startReporter.AppendLine(report);
		}
	}

	public void AddMeetingChatReport(string report)
	{
		AddMeetingChatReport(new NoRpcStringSerializer(report));
	}

	public void AddMeetingChatReport(IStringSerializer serializer)
	{
		this.chatReport.Enqueue(serializer);
	}

	public void AddMeetingEndReport(string report)
	{
		this.exilReporter.AppendLine(report);
	}

	public string GetMeetingStartReport() => this.startReporter.ToString();

	public string GetMeetingEndReport() => this.exilReporter.ToString();

	public void ReportMeetingChat()
	{
		if (this.waitTimer > 0.0f)
		{
			this.waitTimer -= Time.deltaTime;
			return;
		}

		if (this.chatReport.TryDequeue(out var serializer))
		{
			string chatBody = serializer.ToString();

			if (serializer.IsRpc)
			{
				using (var caller = RPCOperator.CreateCaller(
					RPCOperator.Command.MeetingReporterRpc))
				{
					caller.WriteByte((byte)RpcOpType.ChatSerializeDeserialize);
					IStringSerializer.SerializeStatic(serializer, caller);
				}
			}

			HudManager.Instance.Chat.AddChat(
				PlayerControl.LocalPlayer, chatBody);

			this.resetWaitTimer();
		}
	}

	private void resetWaitTimer()
	{
		this.waitTimer = this.chatReport.Count != 0 ? 1.0f : 0.0f;
	}
}
