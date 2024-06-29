using HarmonyLib;

using ExtremeRoles;
using ExtremeRoles.Performance;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
public static class MeetingHudSetForegroundForDeadPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger ||
			FastDestroyableSingleton<HudManager>.Instance == null) { return true; }

		if (PlayerControl.LocalPlayer.PlayerId !=
			ExtremeRolesPlugin.ShipState.ExiledAssassinId)
		{
			return true;
		}
		else
		{
			var meeting = FastDestroyableSingleton<HudManager>.Instance.MeetingPrefab;

			__instance.amDead = false;
			__instance.Glass.sprite = meeting.Glass.sprite;
			__instance.Glass.color = meeting.Glass.color;
			return false;
		}
	}
}
