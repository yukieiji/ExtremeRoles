using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.Module.GameResult.StatusOverrider;

public sealed class MonikaMeetingResultStatusOverrider(
	NetworkedPlayerInfo winner,
	NetworkedPlayerInfo notSeleect) : IStatusOverrider
{
	private readonly byte winnerPlayerId = winner.PlayerId;
	private readonly byte notSelectPlayerId = notSeleect.PlayerId;

	public bool TryGetOverride(
		SingleRoleBase role,
		GhostRoleBase ghostRole,
		NetworkedPlayerInfo player,
		out PlayerStatus status)
	{
		byte playerId = player.PlayerId;

		if (playerId == winnerPlayerId)
		{
			status = PlayerStatus.LoveYou;
			return true;
		}
		else if (playerId == notSelectPlayerId)
		{
			status = PlayerStatus.DeadAssassinate;
			return true;
		}
		status = PlayerStatus.Alive;
		return false;
	}
}
