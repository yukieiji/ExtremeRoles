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

        // Only ExtremeRoles!!
        var exrOfficialTokyo = new DnsRegionInfo(
            "168.138.196.31",
            ServerManagerExtension.ExROfficialServerTokyoManinName,
            StringNames.NoTranslation,
            "168.138.196.31",
            22023,
            false);

        var opt = ClientOption.Instance;

        var customRegion = new DnsRegionInfo(
            opt.Ip.Value,
            ServerManagerExtension.FullCustomServerName,
            StringNames.NoTranslation,
            opt.Ip.Value,
            opt.Port.Value,
            false);

        regions = regions.Concat(
            new IRegionInfo[]
            {
                exrOfficialTokyo.Cast<IRegionInfo>(),
                customRegion.Cast<IRegionInfo>()
            }).ToArray();

        ServerManager.DefaultRegions = regions;
        serverManager.AvailableRegions = regions;
    }
}
