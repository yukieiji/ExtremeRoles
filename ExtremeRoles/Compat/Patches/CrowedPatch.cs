using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Patches.Meeting.Hud;

namespace ExtremeRoles.Compat.Patches;

public static class CrowedModPatch
{
	public static bool IsNotMonikaMeeting()
		=>
		!(
			OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
			system.IsActiveMeeting<MonikaLoveTargetMeeting>()
		);

	public static void SortMonikaTrash(ref IEnumerable<PlayerVoteArea> __result)
	{
		if (MonikaTrashSystem.TryGet(out var system))
		{
			__result
				.OrderBy(MeetingHudSortButtonsPatch.DefaultSort)
				.ThenBy(system.GetVoteAreaOrder);
		}
	}
}
