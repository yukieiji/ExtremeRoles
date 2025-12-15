namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public sealed class RemoveAddPlusWinnerProcessor : IWinnerProcessor
{
	public void Process(WinnerContainer winner, WinnerState _)
	{
		foreach (var player in winner.PlusedWinner)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"Remove Winner(Dupe Player) : {player.PlayerName}");
			winner.Remove(player);
		}
	}
}
