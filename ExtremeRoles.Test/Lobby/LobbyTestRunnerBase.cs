using System.IO;
using System.Collections;

using BepInEx;
using BepInEx.Logging;
using BepInExLogger = BepInEx.Logging.Logger;

namespace ExtremeRoles.Test.Lobby;

public abstract class LobbyTestRunnerBase : ILobbyTestRunner
{
	protected ManualLogSource Log
	{
		get
		{
			if (logListener == null)
			{
				string dirPath = Path.Combine(Paths.BepInExRootPath, "ExtremeRoles.Test");
				if (!Directory.Exists(dirPath))
				{
					Directory.CreateDirectory(dirPath);
				}
				logListener = new DiskLogListener(
					Path.Combine(dirPath, $"{GetType().Name}.log"),
					LogLevel.All);
				BepInExLogger.Listeners.Add(logListener);
			}
			return ExtremeRolesTestPlugin.Instance.Log;
		}
	}

	public bool IsDebugOnly { get; set; } = false;

	private DiskLogListener? logListener;

	~LobbyTestRunnerBase()
	{
		if (logListener != null)
		{
			BepInExLogger.Listeners.Remove(logListener);
		}
	}

	public abstract IEnumerator Run();
}
