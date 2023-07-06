using HarmonyLib;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHudClosePatch
{
	public static void Prefix()
	{
		InfoOverlay.Instance.Hide();
	}
}