using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.Compat.Patches;

public static class CrowedModPatch
{
	public static bool IsNotMonikaMeeting()
		=>
		!(
			OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
			system.IsActiveMeeting<MonikaLoveTargetMeeting>()
		);
}
