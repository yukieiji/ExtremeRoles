using ExtremeRoles.Module.Interface;
using Hazel;

using System.Collections.Generic;

namespace ExtremeRoles.Module.SystemType;

public sealed class IronMateGurdSystem : IExtremeSystemType
{
	private readonly Dictionary<byte, int> shield = new Dictionary<byte, int>();
	public bool TryGetShield(byte playerId, out int num)
		=> this.shield.TryGetValue(playerId, out num) && num > 0;

	public void SetUp(byte playerId, int guardNum)
	{
		lock (shield)
		{
			shield[playerId] = guardNum;
		}
	}

	public void RpcUpdateNum(byte playerId)
	{
		ExtremeSystemTypeManager.RpcUpdateSystem(
			ExtremeSystemType.IronMateGuard,
			x => x.Write(playerId));
	}

	public void Reset(ResetTiming timing, PlayerControl resetPlayer = null)
	{ }

	public void UpdateSystem(PlayerControl player, MessageReader msgReader)
	{
		byte playerId = msgReader.ReadByte();
		lock (shield)
		{
			if (!shield.TryGetValue(playerId, out int num))
			{
				return;
			}
			shield[playerId] = num - 1;
		}
	}
}
