using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.FemmeFatale;

#nullable enable

public sealed class JoseStatusModel(float maxTimer) : IStatusModel
{
	private readonly float maxSelfKillTimer = maxTimer;

	public float CurrentSelfKillTimer { get; private set; } = maxTimer;
	private bool hasExploded = false;

	public void Reset()
	{
		this.CurrentSelfKillTimer = this.maxSelfKillTimer;
	}

	public bool UpdateTimer(float deltaTime)
	{
		if (!GameProgressSystem.IsTaskPhase || this.hasExploded)
		{
			return false;
		}

		this.CurrentSelfKillTimer -= deltaTime;

		if (this.CurrentSelfKillTimer <= 0.0f)
		{
			this.CurrentSelfKillTimer = 0.0f;
			this.hasExploded = true;
			return true;
		}

		return false;
	}
}
