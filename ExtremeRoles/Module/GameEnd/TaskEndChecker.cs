using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.GameEnd;

public sealed class TaskEndChecker : IGameEndChecker
{
	private readonly GameData data = GameData.Instance;

	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = GameOverReason.CrewmatesByTask;

		this.data.RecomputeTaskCounts();

		return
			this.data.TotalTasks > 0 &&
			this.data.CompletedTasks >= this.data.TotalTasks;
	}
}
