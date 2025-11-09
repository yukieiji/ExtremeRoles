using System.Collections.Generic;

using ExtremeRoles.GameMode;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public sealed class AddNeutralWinerProcessor : IWinnerProcessor
{
	private readonly HashSet<(ExtremeRoleId, int)> winRoleCache = new HashSet<(ExtremeRoleId, int)>();

	public void Process(WinnerContainer winner, WinnerState state)
	{
		if (!ExtremeGameModeManager.Instance.ShipOption.DisableNeutralSpecialForceEnd)
		{
			return;
		}

		this.winRoleCache.Clear();

		foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (!ExtremeRoleManager.TryGetRole(playerInfo.PlayerId, out var role))
			{
				continue;
			}

			if (role is MultiAssignRoleBase multiAssignRole &&
				multiAssignRole.AnotherRole is not null &&
				tryAddWinRole(
					winner, multiAssignRole.AnotherRole, playerInfo))
			{
				continue;
			}
			tryAddWinRole(winner, role, playerInfo);
		}
	}
	private bool tryAddWinRole(
		WinnerContainer winner,
		in SingleRoleBase role,
		in NetworkedPlayerInfo playerInfo)
	{
		int gameControlId = role.GameControlId;

		if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
		{
			gameControlId = PlayerStatistics.SameNeutralGameControlId;
		}

		var logger = ExtremeRolesPlugin.Logger;
		var item = (role.Core.Id, gameControlId);

		if (winRoleCache.Contains(item))
		{
			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			winner.Add(playerInfo);
			return true;
		}
		else if (role.IsNeutral() && role.IsWin)
		{
			winRoleCache.Add(item);

			logger.LogInfo($"Add Winner(Reason:Additional Neutral Win) : {playerInfo.PlayerName}");
			winner.Add(playerInfo);
			return true;
		}
		return false;
	}
}
