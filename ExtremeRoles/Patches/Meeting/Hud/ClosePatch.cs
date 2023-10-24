using ExtremeRoles.Module.SystemType;
using HarmonyLib;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHudClosePatch
{
	public static void Prefix()
	{
		InfoOverlay.Instance.Hide();
		ExtremeSystemTypeManager.Instance.Reset(null, (byte)ResetTiming.MeetingEnd);
		ExtremeRolesPlugin.Logger.LogInfo(" ----- MeetingHud Closed ----- ");
	}
}