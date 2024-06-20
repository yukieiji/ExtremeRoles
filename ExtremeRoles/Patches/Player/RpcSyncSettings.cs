using HarmonyLib;

namespace ExtremeRoles.Patches.Player;

#nullable enable

using ExtremeRoles.Module.CustomOption;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControlRpcSyncSettingsPatch
{
	public static void Postfix()
	{
		OptionManager.Instance.ShareOptionSelections();
	}
}