using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Compat.Interface
{
	public interface IMultiFloorModMap : IMapMod
	{
		public int GetLocalPlayerFloor() => GetFloor(PlayerControl.LocalPlayer);
		public int GetFloor(byte playerId)
		{
			if (Player.TryGetPlayerControl(playerId, out var player))
			{
				return GetFloor(player);
			}
			return int.MaxValue;
		}
		public int GetFloor(PlayerControl player);
		public int GetFloor(Vector3 pos);
		public void ChangeLocalPlayerFloor(int floor) => ChangeFloor(PlayerControl.LocalPlayer, floor);
		public void ChangeFloor(byte playerId, int floor)
		{
			if (Player.TryGetPlayerControl(playerId, out var player))
			{
				ChangeFloor(player, floor);
			}
		}
		public void ChangeFloor(PlayerControl player, int floor);
	}
}
