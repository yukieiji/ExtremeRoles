using ExtremeRoles.Module.CustomOption.OLDS;
using HarmonyLib;

namespace ExtremeRoles.Patches.Player;

#nullable enable

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
public static class PlayerControlRpcSyncSettingsPatch
{
	public static void Postfix()
	{
		OptionManager.Instance.ShereAllOption();
	}
}