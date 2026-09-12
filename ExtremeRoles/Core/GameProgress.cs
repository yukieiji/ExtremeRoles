using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Module.SystemType;

namespace ExtremeRoles.Core;

public class GameProgress : IGameProgress
{
	public bool IsGameNow => GameProgressSystem.IsGameNow;

	public bool IsTaskPhase => GameProgressSystem.IsTaskPhase;

	public bool IsRoleSetUpEnd => GameProgressSystem.IsRoleSetUpEnd;

	public GameProgressSystem.Progress Current
	{
		set
		{
			GameProgressSystem.Current = value;
		}
	}

	public bool Is(GameProgressSystem.Progress target)
		=> GameProgressSystem.Is(target);
}
