using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Helper;

public static class AprilFools
{
	public static void UpdateApilSkinToggle(
		CreateGameOptions instance)
	{
		var mng = ServerManager.Instance;
		instance.AprilFoolsToggle.SetActive(
			mng != null && mng.IsCustomServer());
	}
}
