using UnityEngine;

namespace ExtremeRoles.Compat.Interface
{
	public interface IMultiFloorModMap : IMapMod
	{
		public bool IsOtherFloor(PlayerControl player);
		public bool IsOtherFloor(byte playerId);
		public bool IsOtherFloor(GameData.PlayerInfo player);
		public Vector3 GetNearChangeFloorPos(PlayerControl player);
		public Vector3 GetNearChangeFloorPos(byte playerId);
		public Vector3 GetNearChangeFloorPos(GameData.PlayerInfo player);
	}
}
