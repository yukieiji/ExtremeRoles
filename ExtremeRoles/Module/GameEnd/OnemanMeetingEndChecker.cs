using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

namespace ExtremeRoles.Module.GameEnd;

public sealed class OnemanMeetingEndChecker : IGameEndChecker
{
	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.ImpostorsByVote;

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system) ||
			MeetingHud.Instance != null ||
			ExileController.Instance != null ||
			!system.TryGetGameEndReason(out var reson))
		{
			return false;
		}

		reason = (GameOverReason)reson;
		return true;
	}
}
