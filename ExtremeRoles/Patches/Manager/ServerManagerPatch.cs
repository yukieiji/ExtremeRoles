using ExtremeRoles.Module;
using HarmonyLib;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
public static class ServerManagerLoadServersPatch
{
	public static void Postfix(ServerManager __instance)
	{
		CustomRegion.ReSelectRegion(__instance);
		__instance.CurrentUdpServer = __instance.CurrentRegion.Servers[0];
	}
}

[HarmonyPatch(typeof(ServerManager), nameof(ServerManager.ReselectServer))]
public static class ServerManagerReselectPatch
{
	public static void Prefix(ServerManager __instance)
	{
		CustomRegion.ReSelectRegion(__instance);
	}
}
