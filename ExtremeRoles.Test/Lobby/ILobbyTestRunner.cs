namespace ExtremeRoles.Test.Lobby;

public interface ILobbyTestRunner
{
	public bool IsDebugOnly { get; set; }

	public void Run();
}
