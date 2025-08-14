using ExtremeRoles.Roles.API.Interface.Ability;

namespace ExtremeRoles.Roles.Combination.HeroAcademia;

public sealed class VigilanteAbilityHandler(VigilanteStatusModel status) : IAbility, IKilledFrom
{
	private readonly VigilanteStatusModel status = status;

	public bool TryKilledFrom(
		PlayerControl rolePlayer, PlayerControl fromPlayer)
		=> !(ExtremeRoleManager.TryGetRole(fromPlayer.PlayerId, out var fromRole) &&
			fromRole.Core.Id is ExtremeRoleId.Hero &&
			this.status.Condition is not Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip);
}
