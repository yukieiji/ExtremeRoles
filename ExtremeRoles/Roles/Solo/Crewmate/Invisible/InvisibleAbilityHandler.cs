using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate.Invisible;

public sealed class InvisibleAbilityHandler(
	InvisibleSystem invisibleSystem,
	PlayerSpeedSystem speedSystem,
	InvisibleStatusModel status) : IAbility, IInvincible
{
	private readonly InvisibleSystem invisibleSystem = invisibleSystem;
	private readonly PlayerSpeedSystem speedSystem = speedSystem;
	private readonly InvisibleStatusModel status = status;

	public bool IsActive => this.status.IsActiveInvisible;

	public void Update()
	{
		this.invisibleSystem.Update();
	}

	public bool Execute(byte rolePlayerId)
	{
		this.status.IsActiveInvisible = true;
		this.invisibleSystem.SetInvisible(rolePlayerId, true);
		this.speedSystem.UpdateSelfSpeed(this.status.SpeedMod);
		return true;
	}

	public void CleanUp(byte rolePlayerId)
	{
		if (!this.status.IsActiveInvisible)
		{
			return;
		}

		this.status.IsActiveInvisible = false;
		this.invisibleSystem.SetInvisible(rolePlayerId, false);
		this.speedSystem.UpdateSelfSpeed(0.0f);
	}

	public void Reset(byte rolePlayerId)
	{
		CleanUp(rolePlayerId);
	}

	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		if (!this.status.HasHitbox && this.status.IsActiveInvisible)
		{
			return true;
		}
		return false;
	}

	public bool IsValidKillFromSource(byte source)
	{
		if (!this.status.HasHitbox && this.status.IsActiveInvisible)
		{
			return false;
		}
		return true;
	}

	public bool IsValidAbilitySource(byte source)
	{
		if (!this.status.HasHitbox && this.status.IsActiveInvisible)
		{
			return false;
		}
		return true;
	}
}
