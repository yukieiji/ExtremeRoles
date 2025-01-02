using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.Module.GameResult.StatusOverrider;

public sealed class AssassinAssassinateStatusOverrider(byte marinPlayerId) : IStatusOverrider
{
	private readonly byte marinPlayerId = marinPlayerId;

	public bool TryGetOverride(
		SingleRoleBase role,
		GhostRoleBase ghostRole,
		NetworkedPlayerInfo player,
		out PlayerStatus status)
	{
		status = PlayerStatus.Surrender;

		if (player.PlayerId == marinPlayerId)
		{
			if (player.IsDead || player.Disconnected)
			{
				status = PlayerStatus.DeadAssassinate;
			}
			else
			{
				status = PlayerStatus.Assassinate;
			}
			return true;
		}

		return !(
			role.IsImpostor() ||
			player.IsDead ||
			player.Disconnected);
	}
}
