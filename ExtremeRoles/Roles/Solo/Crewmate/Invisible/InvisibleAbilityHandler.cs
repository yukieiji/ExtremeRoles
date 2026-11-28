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
	private bool isActive = false;

	public bool Execute(byte rolePlayerId)
	{
		this.isActive = true;
		this.invisibleSystem.SetInvisible(rolePlayerId, true);
		this.speedSystem.UpdateSelfSpeed(this.status.SpeedMod);
		return true;
	}

	public void CleanUp(byte rolePlayerId)
	{
		if (!this.isActive)
		{
			return;
		}

		this.isActive = false;
		this.invisibleSystem.SetInvisible(rolePlayerId, false);
		this.speedSystem.UpdateSelfSpeed(0.0f);
	}

	public bool IsBlockKillFrom(byte? fromPlayer)
		=> !this.status.HasHitbox && this.isActive;

	public bool IsValidKillFromSource(byte source)
		=> !IsBlockKillFrom(source);

	public bool IsValidAbilitySource(byte source)
		=> !IsBlockKillFrom(source);
}
