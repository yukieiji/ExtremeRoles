namespace ExtremeRoles.Module.Interface;

public interface IGameEndChecker
{
	public bool TryCheckGameEnd(out GameOverReason reason);

	public void CleanUp()
	{

	}

	protected static void SetWinGameContorlId(int id)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.SetWinGameControlId))
		{
			caller.WriteInt(id);
		}
		RPCOperator.SetWinGameControlId(id);
	}
}
