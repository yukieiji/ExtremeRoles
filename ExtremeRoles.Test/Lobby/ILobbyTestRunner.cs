using System.Collections;

namespace ExtremeRoles.Test.Lobby;

public interface ILobbyTestRunner
{
	public bool IsDebugOnly { get; set; }

	public IEnumerator Run();
}
