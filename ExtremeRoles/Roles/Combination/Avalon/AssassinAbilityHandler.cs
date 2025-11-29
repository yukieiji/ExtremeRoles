using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Combination.Avalon;

public class AssassinAbilityHandler(AssassinStatusModel status) : IAbility, IInvincible
{
	private AssassinStatusModel status = status;

	// アサシンは能力の対象にはなるけど、キルの対象と絶対的なキルを防げる
	public bool IsBlockKillFrom(byte? fromPlayer)
	{
		if (this.status.IsBlockKill)
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
			return this.status.IsBlockKillFromNeutral;
		}
		else if (fromPlayerRole.IsCrewmate())
		{
			return this.status.IsBlockKillFromCrew;
		}
		else if (fromPlayerRole.IsLiberal())
		{
			return this.status.IsBlockKillFromLiberal;
		}
		return true;
	}

	public bool IsValidKillFromSource(byte target)
		=> !IsBlockKillFrom(target);

	public bool IsValidAbilitySource(byte target)
		=> true;
}
