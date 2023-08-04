using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Module.RoleAssign;

public readonly struct PlayerToSingleRoleAssignData : IPlayerToExRoleAssignData
{
	public byte PlayerId { get; init; }
	public int RoleId { get; init; }
	public int ControlId { get; init; }

	public byte RoleType => (byte)IPlayerToExRoleAssignData.ExRoleType.Single;

	public PlayerToSingleRoleAssignData(
		byte playerId, int roleId, int controlId)
	{
		this.PlayerId = playerId;
		this.RoleId = roleId;
		this.ControlId = controlId;
	}
}

public readonly struct PlayerToCombRoleAssignData : IPlayerToExRoleAssignData
{
	public byte PlayerId { get; init; }
	public int RoleId { get; init; }
	public int ControlId { get; init; }

	public byte RoleType => (byte)IPlayerToExRoleAssignData.ExRoleType.Comb;

	public byte CombTypeId { get; init; }
	public byte AmongUsRoleId { get; init; }

	public PlayerToCombRoleAssignData(
		byte playerId, int roleId,
		byte combType, int gameContId,
		byte amongUsRoleId)
	{
		this.PlayerId = playerId;
		this.RoleId = roleId;
		this.CombTypeId = combType;
		this.ControlId = gameContId;
		this.AmongUsRoleId = amongUsRoleId;
	}
}
