using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtremeRoles.Module.RoleAssign;

public readonly struct VanillaRolePlayerAssignData(byte playerId, string playerName, RoleTypes role)
{
	public readonly byte PlayerId = playerId;
	public readonly string PlayerName = playerName;
	public readonly RoleTypes Role = role;

	public override bool Equals(object obj)
		=> obj is VanillaRolePlayerAssignData data &&
			data.PlayerName == PlayerName &&
			data.PlayerId == PlayerId &&
			data.Role == Role;

	public override int GetHashCode()
		=> PlayerId.GetHashCode() ^ Role.GetHashCode() ^ PlayerName.GetHashCode();
}