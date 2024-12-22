using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;

using PlayerStatus = ExtremeRoles.Module.ExtremeShipStatus.ExtremeShipStatus.PlayerStatus;

namespace ExtremeRoles.Module.GameResult.StatusOverrider;

public interface IStatusOverrider
{
	public bool TryGetOverride(
		SingleRoleBase role,
		GhostRoleBase ghostRole,
		NetworkedPlayerInfo player,
		out PlayerStatus status);
}
