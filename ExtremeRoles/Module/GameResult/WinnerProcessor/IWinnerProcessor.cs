namespace ExtremeRoles.Module.GameResult.WinnerProcessor;

public interface IWinnerProcessor
{
	public void Process(WinnerContainer winner, WinnerState state);
}
