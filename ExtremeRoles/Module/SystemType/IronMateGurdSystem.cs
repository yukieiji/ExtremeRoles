using Hazel;

using System.Collections.Generic;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.SystemType;

public sealed class IronMateGurdSystem(float speedMod, float speedTime) : IExtremeSystemType
{
	private readonly Dictionary<byte, int> shield = new Dictionary<byte, int>();

	private readonly float speedMod = speedMod;
	private readonly float speedTime = speedTime;

	public bool IsContains(byte playerId) => this.shield.ContainsKey(playerId);

	public bool TryGetShield(byte playerId, out int num)
		=> this.shield.TryGetValue(playerId, out num) && num > 0;

	public void SetUp(byte playerId, int guardNum)
	{
		lock (shield)
		{
			shield[playerId] = guardNum;
		}
	}

	public void RpcUpdateNum(byte playerId, int newNum)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.IronMateGuard,
			x => {
				x.Write(playerId);
				x.WritePacked(newNum);
			});
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		lock (shield)
		{
			shield[playerId] = msgReader.ReadPackedInt32();
		}

		if (PlayerControl.LocalPlayer == null ||
 			playerId != PlayerControl.LocalPlayer.PlayerId)
		{
			return;
		}

		var speedMod = PlayerControl.LocalPlayer.gameObject.TryAddComponent<SpeedMod>();
		speedMod.SetUp(this.speedMod, this.speedTime);
	}
}
