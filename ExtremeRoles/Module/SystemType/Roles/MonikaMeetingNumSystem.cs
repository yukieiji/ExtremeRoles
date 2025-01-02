using System.Collections.Generic;
using System.Linq;


using Hazel;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;

namespace ExtremeRoles.Module.SystemType.Roles;

public sealed class MonikaMeetingNumSystem : IExtremeSystemType
{
	public bool IsDirty { get; set; } = false;

	private readonly Dictionary<byte, int> meetingNums = new Dictionary<byte, int>(createData());

	public bool TryReduce()
	{
		var validNum = this.meetingNums.Where(
			(item) =>
				item.Value > 0 &&
				ExtremeRoleManager.TryGetRole(item.Key, out var role) &&
				role.Id is not ExtremeRoleId.Monika &&
				role.CanCallMeeting());

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

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		bool isForceReduce = msgReader.ReadBoolean();
		lock (this.meetingNums)
		{
			if (!this.meetingNums.TryGetValue(playerId, out int num))
			{
				return;
			}
			this.meetingNums[playerId] = num - 1;
		}
		var local = PlayerControl.LocalPlayer;
		if (isForceReduce &&
			local.PlayerId == playerId)
		{
			local.RemainingEmergencies--;
		}
	}

	private static IEnumerable<KeyValuePair<byte, int>> createData()
	{
		int num = GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings;
		foreach (var player in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			yield return new KeyValuePair<byte, int>(player.PlayerId, num);
		}
	}
}
