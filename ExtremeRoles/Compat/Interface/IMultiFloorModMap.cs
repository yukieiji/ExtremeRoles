using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Compat.Interface
{
	public interface IMultiFloorModMap : IMapMod
	{
		public int GetLocalPlayerFloor() => GetFloor(CachedPlayerControl.LocalPlayer);
		public int GetFloor(byte playerId) => GetFloor(Player.GetPlayerControlById(playerId));
		public int GetFloor(PlayerControl player);
		public int GetFloor(Vector3 pos);
		public void ChangeLocalPlayerFloor(int floor) => ChangeFloor(CachedPlayerControl.LocalPlayer, floor);
		public void ChangeFloor(byte playerId, int floor) => ChangeFloor(Player.GetPlayerControlById(playerId), floor);
		public void ChangeFloor(PlayerControl player, int floor);
	}
}
