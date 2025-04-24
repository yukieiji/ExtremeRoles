using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Module;

#nullable enable

public static class CustomRegion
{
	public static IRegionInfo? EditableServer =>
		ServerManager.Instance.AvailableRegions.FirstOrDefault(
			x => x.Name == IRegionInfoExtension.FullCustomServerName);

	private static readonly Dictionary<string, IRegionInfo> curCustomRegion = [];

	private static IRegionInfo[] newCustomRegion => [
		createStaticRegion(
			IRegionInfoExtension.ExROfficialServerTokyoManinName,
			"168.138.196.31", 22023, false), // Only ExtremeRoles!!
		createStaticRegion(
			IRegionInfoExtension.FullCustomServerName,
			ClientOption.Instance.Ip.Value,
			ClientOption.Instance.Port.Value, false),
	];

	public static void AddCustomServer()
	{
		curCustomRegion.Clear();

		var serverMngr = ServerManager.Instance;
		var currentRegion = serverMngr.CurrentRegion;

		foreach (IRegionInfo region in newCustomRegion)
		{
			if (currentRegion != null && region.Name == currentRegion.Name)
			{
				currentRegion = region;
			}
			serverMngr.AddOrUpdateRegion(region);
			curCustomRegion.Add(region.Name, region);
		}

		if (currentRegion != null)
		{
			serverMngr.SetRegion(currentRegion);
		}
	}

	public static void UpdateEditorableServer()
	{
		IEnumerable<IRegionInfo> newRegions = ServerManager.Instance.AvailableRegions.Where(
			(IRegionInfo r) => !(
					r.Name == IRegionInfoExtension.ExROfficialServerTokyoManinName ||
					r.Name == IRegionInfoExtension.FullCustomServerName
				));
		ServerManager.Instance.AvailableRegions = newRegions.ToArray();
		AddCustomServer();
	}

	public static void ReSelectRegion(ServerManager instance)
	{
		var curRegion = instance.CurrentRegion;
		if (curCustomRegion.TryGetValue(curRegion.Name, out var target))
		{
			instance.CurrentRegion = target;
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
	}
}
