using System.Linq;

using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles;

public static class CustomRegion
{
    public static IRegionInfo[] Default { private get; set; }

    public static void Update()
    {
        ServerManager serverManager = DestroyableSingleton<ServerManager>.Instance;
        IRegionInfo[] regions = Default;

        var opt = ClientOption.Instance;

        regions = regions.Concat(
            new IRegionInfo[]
            {
				createStaticRegion(
					ServerManagerExtension.ExROfficialServerTokyoManinName,
					"168.138.196.31", 22023, false), // Only ExtremeRoles!!
				createStaticRegion(
					ServerManagerExtension.FullCustomServerName,
					opt.Ip.Value, opt.Port.Value, false),
			}).ToArray();

        ServerManager.DefaultRegions = regions;
        serverManager.AvailableRegions = regions;
    }

	private static IRegionInfo createStaticRegion(
		string name, string ip, ushort port, bool useDtls)
		=> new StaticHttpRegionInfo(
			name, StringNames.NoTranslation, ip,
			createServerInfo(name, ip, port, useDtls)).Cast<IRegionInfo>();

	private static ServerInfo[] createServerInfo(string name, string ip, ushort port, bool useDtls)
	{
		if (!ip.StartsWith("http"))
		{
			ip = $"http://{ip}";
		}

		return new ServerInfo[]
		{
			new ServerInfo(name, ip, port, useDtls)
		};
	}
}
