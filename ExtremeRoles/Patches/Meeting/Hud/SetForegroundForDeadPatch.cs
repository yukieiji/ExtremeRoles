using HarmonyLib;

using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SetForegroundForDead))]
public static class MeetingHudSetForegroundForDeadPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!(
				FastDestroyableSingleton<HudManager>.Instance != null &&
				OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
				system.IsForgeBackgroundDead(__instance)
			))
		{
			return true;
		}

		var meeting = FastDestroyableSingleton<HudManager>.Instance.MeetingPrefab;

		__instance.amDead = false;
		__instance.Glass.sprite = meeting.Glass.sprite;
		__instance.Glass.color = meeting.Glass.color;
		return false;
	}
}
