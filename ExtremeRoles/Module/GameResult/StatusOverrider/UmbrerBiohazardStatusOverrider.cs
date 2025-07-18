using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.Module.GameResult.StatusOverrider;

public sealed class UmbrerBiohazardStatusOverrider : IStatusOverrider
{
	public bool TryGetOverride(
		SingleRoleBase role,
		GhostRoleBase ghostRole,
		NetworkedPlayerInfo player,
		out PlayerStatus status)
	{
		status = PlayerStatus.Zombied;
		return !(
			role.Core.Id is ExtremeRoleId.Umbrer ||
			player.IsDead ||
			player.Disconnected);
	}
}
