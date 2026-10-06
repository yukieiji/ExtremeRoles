using Hazel;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Neutral;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MastermindNoticeSystem : IExtremeSystemType
{
	public sealed class MastermindStringSerializer : IStringSerializer
	{
		public StringSerializerType Type => StringSerializerType.MastermindReport;

		public bool IsRpc { get; set; }

		private uint aliveMastermindCount;

		public MastermindStringSerializer()
		{
		}
		
		public MastermindStringSerializer(uint aliveMastermindCount)
		{
			this.aliveMastermindCount = aliveMastermindCount;
		}

		public void Deserialize(MessageReader reader)
		{
			aliveMastermindCount = reader.ReadUInt32();
		}

		public void Serialize(RPCOperator.RpcCaller caller)
		{
			caller.WritePackedUInt(aliveMastermindCount);
		}

		public override string ToString()
			=> Tr.GetString("mastermindAliveChat", aliveMastermindCount);
	}



	public const ExtremeSystemType Type = ExtremeSystemType.MastermindNotice;

	public static MastermindNoticeSystem CreateOrGet()
		=> ExtremeSystemTypeManager.Instance.CreateOrGet<MastermindNoticeSystem>(Type);

	public void UpdateSystem(PlayerControl player, MessageReader msgReader) { }

	public void Reset(ResetTiming timing, PlayerControl? player)
	{
		if (timing is not ResetTiming.MeetingStart || 
			AmongUsClient.Instance == null ||
			!AmongUsClient.Instance.AmHost)
		{
			return;
		}

		uint aliveMastermindCount = 0;
		foreach (var p in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (p.IsAlive() &&
				ExtremeRoleManager.TryGetSafeCastedRole<Mastermind>(p.PlayerId, out var role))
			{
				aliveMastermindCount++;
			}
		}

		MeetingReporter.Instance.AddMeetingChatReport(new MastermindStringSerializer(aliveMastermindCount));
	}
}
