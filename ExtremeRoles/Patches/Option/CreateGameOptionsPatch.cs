using HarmonyLib;

using ExtremeRoles.Helper;

namespace ExtremeRoles.Patches.Option;

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Show))]
public static class CreateGameOptionsShowPatch
{
	public static void Postfix(CreateGameOptions __instance)
	{
		AprilFools.UpdateApilSkinToggle(__instance);
	}
}

[HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.CloseServerDropdown))]
public static class CreateGameCloseServerDropdownPatch
{
	public static void Postfix(CreateGameOptions __instance)
	{
		AprilFools.UpdateApilSkinToggle(__instance);
	}
}
