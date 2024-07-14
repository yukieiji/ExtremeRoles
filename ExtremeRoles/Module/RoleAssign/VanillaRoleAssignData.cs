using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using System.Collections.Generic;

namespace ExtremeRoles.Module.RoleAssign;

public sealed class VanillaRoleAssignData : NullableSingleton<VanillaRoleAssignData>
{
	public IReadOnlyList<VanillaRolePlayerAssignData> Data => data;

	private readonly List<VanillaRolePlayerAssignData> data = new();

	public void Add(in PlayerControl player, RoleTypes role)
	{
		string playerName = player.Data.DefaultOutfit.PlayerName;
		Logging.Debug($"VanillaRole Assign: {playerName} to {role}");
		data.Add(
			new VanillaRolePlayerAssignData(player.PlayerId, playerName, role));
	}
}
