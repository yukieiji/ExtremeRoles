using HarmonyLib;

namespace ExtremeRoles.Patches.Player.Meeting;

#nullable enable

// HotFix : 以下の2つのバニラの不具合の修正
// 1. ぷるぷるとはしご使用時にキルクールタイムを強制的に進めるフラグがリセットされず、その後ベントを使用してもキルクールが進む不具合
// 2. はしごを登っている状態がリセットされない不具合
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ResetForMeeting))]
public static class PlayerControlResetForMeetingPatch
{
	public static void Postfix(PlayerControl __instance)
	{
		__instance.ForceKillTimerContinue = false;
		__instance.onLadder = false;
	}
}
