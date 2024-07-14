using AmongUs.GameOptions;

namespace ExtremeRoles.Module.RoleAssign;

public readonly struct VanillaRolePlayerAssignData(byte playerId, string playerName, RoleTypes role)
{
	public readonly byte PlayerId = playerId;
	public readonly string PlayerName = playerName;
	public readonly RoleTypes Role = role;

	public VanillaRolePlayerAssignData(in NetworkedPlayerInfo pc) :
		this(pc.PlayerId, pc.DefaultOutfit.PlayerName, pc.Role.Role)
	{ }

	public static bool operator ==(VanillaRolePlayerAssignData d1, VanillaRolePlayerAssignData d2)
		=> d1.Equals(d2);

	public static bool operator !=(VanillaRolePlayerAssignData d1, VanillaRolePlayerAssignData d2)
		=> !d1.Equals(d2);

	public override bool Equals(object obj)
		=> obj is VanillaRolePlayerAssignData data &&
			data.PlayerName == PlayerName &&
			data.PlayerId == PlayerId &&
			data.Role == Role;

	public override int GetHashCode()
		=> PlayerId.GetHashCode() ^ Role.GetHashCode() ^ PlayerName.GetHashCode();
}