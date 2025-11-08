using HarmonyLib;

using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public static class ConstantsGetBroadcastVersionPatch
{
	public static void Postfix(ref int __result)
	{
		if (IsCustomServer)
		{
			return;
		}
		__result += 25;
	}
	public static bool IsCustomServer =>
		AmongUsClient.Instance == null ||
		AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
		ServerManager.Instance == null ||
		ServerManager.Instance.IsCustomServer();
}

[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class ConstantsIsVersionModdedPatch
{
	public static bool Prefix(ref bool __result)
	{
		__result = !ConstantsGetBroadcastVersionPatch.IsCustomServer;
		return false;
	}
}