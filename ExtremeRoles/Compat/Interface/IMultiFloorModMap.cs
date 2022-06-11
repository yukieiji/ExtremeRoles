namespace ExtremeRoles.Compat.Interface
{
	public interface IMultiFloorModMap : IMapMod
	{
		public int GetLocalPlayerFloor();
		public int GetFloor(PlayerControl player);
		public void ChangeLocalPlayerFloor(int floor);
		public void ChangeFloor(PlayerControl player, int floor);
	}
}
