using System.Linq;

using HarmonyLib;
using Innersloth.IO;
using Newtonsoft.Json;

using ExtremeRoles.Module;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
public static class ServerManagerLoadServersPatch
{
	private const string nosServer = "Nebula on the Ship JP";

	public static void Prefix(ServerManager __instance)
	{
		if (!FileIO.Exists(__instance.serverInfoFileJson))
		{
			return;
		}

		var jsonServerData = JsonConvert.DeserializeObject<ServerManager.JsonServerData>(
			FileIO.ReadAllText(__instance.serverInfoFileJson), new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Auto
			});
		jsonServerData.CleanAndMerge(ServerManager.DefaultRegions);

		if (jsonServerData.Regions.Any(x => x.Name.Contains(nosServer)))
		{
			FileIO.Delete(__instance.serverInfoFileJson);
		}
	}

	public static void Postfix(ServerManager __instance)
	{
		CustomRegion.ReSelect(__instance);
		if (__instance.CurrentRegion != null)
		{
			__instance.CurrentUdpServer = __instance.CurrentRegion.Servers[0];
		}
	}
}

[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.ReselectServer))]
public static class ServerManagerReselectPatch
{
	public static void Prefix(ServerManager __instance)
	{
		CustomRegion.ReSelect(__instance);
	}
}
