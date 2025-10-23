using HarmonyLib;

using PlayerHelper = ExtremeRoles.Helper.Player;

namespace ExtremeRoles.Patches.Player;

#nullable enable

// サーバーのアプデでMODだと問答無用でBANされるっぽいので修正パッチを当てる
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
public static class PlayerControlCmdReportDeadBodyPatch
{
	public static bool Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] NetworkedPlayerInfo target)
	{
		PlayerHelper.RpcUncheckReportDeadBody(target);
		return false;
	}
}