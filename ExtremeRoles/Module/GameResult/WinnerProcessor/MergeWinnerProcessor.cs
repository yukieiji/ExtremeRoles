namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public sealed class MergeWinnerProcessor : IWinnerProcessor
{
	public void Process(WinnerContainer winner, WinnerState state)
	{
		var logger = ExtremeRolesPlugin.Logger;

		logger.LogInfo($"-- Start: merge plused win player --");
		foreach (var player in winner.PlusedWinner)
		{
			logger.LogInfo($"marge to winner:{player.PlayerName}");
			winner.Add(player);
		}
		logger.LogInfo($"-- End: merge plused win player --");
	}
}
