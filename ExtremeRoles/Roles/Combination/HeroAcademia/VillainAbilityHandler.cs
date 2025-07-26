using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Combination.HeroAcademia;

public class VillainAbilityHandler : IAbility, IKilledFrom
{
	public bool TryKilledFrom(PlayerControl rolePlayer, PlayerControl fromPlayer)
	{
		if (ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromRole) &&
			fromRole.Id is ExtremeRoleId.Hero)
		{
			HeroAcademiaRole.RpcDrawHeroAndVillan(fromPlayer, rolePlayer);
			return false;
		}
		return !fromRole.IsCrewmate();
	}
}
