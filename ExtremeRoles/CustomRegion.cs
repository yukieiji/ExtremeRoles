using System.Linq;


namespace ExtremeRoles;


public static class CustomRegion
{
    // public static IRegionInfo[] Default { private get; set; }

    public static void Update()
    {
		/*
        ServerManager serverManager = ServerManager.Instance;
        IRegionInfo[] defaultRegions = Default;

        var opt = ClientOption.Instance;

		var exrTokyo = createStaticRegion(
			IRegionInfoExtension.ExROfficialServerTokyoManinName,
			"168.138.196.31", 22023, false); // Only ExtremeRoles!!
		IRegionInfo[] customServerRegion =
		[
			exrTokyo,
			createStaticRegion(
				IRegionInfoExtension.FullCustomServerName,
				opt.Ip.Value, opt.Port.Value, false),
		];

		bool isBetaMode = PublicBeta.Instance.Enable;
		var allRegion = isBetaMode ?
			customServerRegion :
			defaultRegions.Concat(customServerRegion).ToArray();

		ServerManager.DefaultRegions = allRegion;
        serverManager.AvailableRegions = allRegion;

		var curServer = serverManager.CurrentRegion;
		if (isBetaMode && !serverManager.IsCustomServer())
		{
			serverManager.SetRegion(exrTokyo);
		}
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

		return
		[
			new ServerInfo(name, ip, port, useDtls)
		];
		*/
	}
}
