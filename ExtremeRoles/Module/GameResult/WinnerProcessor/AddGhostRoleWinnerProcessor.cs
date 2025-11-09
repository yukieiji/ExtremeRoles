namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public sealed class AddGhostRoleWinnerProcessor : IWinnerProcessor
{
	public void Process(WinnerContainer winner, WinnerState state)
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($"-- Start: add ghostrole win player --");

		foreach (var (playerInfo, winCheckRole) in state.GhostRoleWinCheck)
		{
			if (winCheckRole.IsWin(ExtremeRolesPlugin.ShipState.EndReason, playerInfo))
			{
				ExtremeRolesPlugin.Logger.LogInfo($"Add Winner(Reason:Ghost Role win) : {playerInfo.PlayerName}");
				winner.AddWithPlus(playerInfo);
			}
		}
		logger.LogInfo($"-- End: add ghostrole win player --");
	}
}
