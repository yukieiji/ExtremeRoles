using HarmonyLib;

using ExtremeRoles.Extension.Manager;

namespace ExtremeRoles.Patches;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
public static class ConstantsGetBroadcastVersionPatch
{
	public static void Postfix(ref int __result)
	{
		if (AmongUsClient.Instance == null ||
			AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame ||
			ServerManager.Instance == null ||
			ServerManager.Instance.IsCustomServer())
		{
			return;
		}
		__result += 25;
	}
}
