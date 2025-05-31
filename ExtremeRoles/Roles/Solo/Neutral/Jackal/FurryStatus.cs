using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Interface;
using System.Collections.Generic;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

#nullable enable

public sealed class FurryStatus : IStatusModel
{
	public JackalRole? TargetJackal { get; private set; }

	public void Update()
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				player.Data == null ||
				!ExtremeRoleManager.TryGetSafeCastedRole<SidekickRole>(player.PlayerId, out var sk))
			{
				continue;
			}
			var jkPlayer = GameData.Instance.GetPlayerById(sk.Parent);
			if (jkPlayer == null ||
				!(jkPlayer.IsDead || jkPlayer.Disconnected) ||
				!ExtremeRoleManager.TryGetSafeCastedRole<JackalRole>(jkPlayer.PlayerId, out var jk))
			{
				continue;
			}
		}
	}
}
