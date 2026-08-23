
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

// インライン化されてためパッチ使わず処理する
public static class MeetingHudUpdateForegroundPatchHelper
{
	public static void Patch(MeetingHud __instance)
	{
		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system) ||
			!system.IsDefaultForegroundForDead(__instance))
		{
			return;
		}
		var meeting = HudManager.Instance.MeetingPrefab;
		__instance.hasForegroundForDeadBeenSet = false;
		__instance.Glass.sprite = meeting.Glass.sprite;
		__instance.Glass.color = meeting.Glass.color;
	}
}