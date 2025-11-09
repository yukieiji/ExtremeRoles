
namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public sealed class ModifiedWinnerProcessor : IWinnerProcessor
{
	public void Process(WinnerContainer winner, WinnerState state)
	{
		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo($"-- Start: modified win player --");
		foreach (var (playerInfo, winModRole) in state.ModRole)
		{
			winModRole.ModifiedWinPlayer(
				playerInfo,
				ExtremeRolesPlugin.ShipState.EndReason, // 更新され続けるため、新しいのを常に渡す
				in winner);
		}
		logger.LogInfo($"-- End: modified win player --");
	}
}
