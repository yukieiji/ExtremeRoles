using AmongUs.GameOptions;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRoleAssignData : NullableSingleton<VanillaRoleAssignData>
{
	public IReadOnlyList<VanillaRolePlayerAssignData> Data => data;

	private readonly List<VanillaRolePlayerAssignData> data = new();

	public void Add(in PlayerControl player, RoleTypes role)
	{
		data.Add(
			new VanillaRolePlayerAssignData(player.PlayerId, player.Data.DefaultOutfit.PlayerName, role));
	}
}
