namespace ExtremeRoles.Extension.Manager;

public static class ServerManagerExtension
{
	public static bool IsCustomServer(this ServerManager mng)
		=> mng.CurrentRegion.IsCustomServer();

	public static bool IsExROnlyServer(this ServerManager mng)
		=> mng.CurrentRegion.IsExROnlyServer();
}
