using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Core.Abstract;

public interface IGameProgress
{
	public bool IsGameNow { get; }
	public bool IsTaskPhase { get; }
	public bool IsRoleSetUpEnd { get; }

	public bool Is(GameProgressSystem.Progress target);

	public GameProgressSystem.Progress Current { set; }
}
