using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinAbilityHandler : IAbility, IInvincible
{
	private AssassinStatusModel status;

	public AssassinAbilityHandler(AssassinStatusModel status)
	{
		this.status = status;
	}

	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		if (status.IsBlockKill)
		{
			return true;
		}
		if (!fromPlayer.HasValue)
		{
			return false;
		}

		if (!ExtremeRoleManager.TryGetRole(fromPlayer.Value, out var fromPlayerRole))
		{
			return true;
		}

		if (fromPlayerRole.IsNeutral())
		{
			return status.IsBlockKillFromNeutral;
		}
		else if (fromPlayerRole.IsCrewmate())
		{
			return status.IsBlockKillFromCrew;
		}
		else if (fromPlayerRole.IsLiberal())
		{
			return status.IsBlockKillFromLiberal;
		}
		return true;
	}

	public bool IsValidTarget(byte target)
		=> true;
}
