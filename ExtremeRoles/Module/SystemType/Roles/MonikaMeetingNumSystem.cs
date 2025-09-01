using System.Collections.Generic;
using System.Linq;


using Hazel;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;

#nullable enable

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaMeetingNumSystem : IExtremeSystemType
{
	public bool IsDirty => false;

	private sealed class MeetingNumData
	{
		public int Num { get; private set; }
		private readonly NetworkedPlayerInfo player;

		public MeetingNumData(NetworkedPlayerInfo player, int num)
		{
			this.player = player;
			this.Num = num;
		}
		public void Reduce()
		{
			this.Num--;
		}
		public bool IsValid()
			=>
				this.player != null &&
				!this.player.IsDead &&
				!this.player.Disconnected;
	}

	private readonly Dictionary<byte, MeetingNumData> meetingNums = new Dictionary<byte, MeetingNumData>(createData());

	public bool TryReduce()
	{
		var validNum = this.meetingNums.Where(
			(item) =>
			{
				var val = item.Value;
				byte key = item.Key;
				return
					val.Num > 0 &&
					ExtremeRoleManager.TryGetRole(key, out var role) &&
					role.Core.Id is not ExtremeRoleId.Monika &&
					role.CanCallMeeting() &&
					val.IsValid();
			});

		if (validNum.Any())
		{
			var item = validNum.OrderBy(
				x => RandomGenerator.Instance.Next()).First();
			RpcReduceTo(item.Key, true);

			return true;
		}
		return false;
	}
	public void RpcReduceTo(byte playerId, bool isForceReduce)
	{
		ExtremeRolesPlugin.Logger.LogInfo($"Reduce Meeting Button to:{playerId} isForce:{isForceReduce}");
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.MonikaMeetingNumSystem,
			x =>
			{
				x.Write(playerId);
				x.Write(isForceReduce);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl? resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		bool isForceReduce = msgReader.ReadBoolean();
		lock (this.meetingNums)
		{
			if (!this.meetingNums.TryGetValue(playerId, out var data))
			{
				return;
			}
			data?.Reduce();
		}
		var local = PlayerControl.LocalPlayer;
		if (isForceReduce &&
			local.PlayerId == playerId)
		{
			local.RemainingEmergencies--;
		}
	}

	private static IEnumerable<KeyValuePair<byte, MeetingNumData>> createData()
	{
		int num = GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			yield return new KeyValuePair<byte, MeetingNumData>(
				player.PlayerId, new MeetingNumData(player, num));
		}
	}
}
