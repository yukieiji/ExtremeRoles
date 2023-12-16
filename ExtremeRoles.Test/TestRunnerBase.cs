using System.IO;

using BepInEx;
using BepInEx.Logging;

using BepInExLogger = BepInEx.Logging.Logger;

namespace ExtremeRoles.Test;

internal abstract class TestRunnerBase
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
					Path.Combine(dirPath, $"{this.GetType().Name}.log"),
					LogLevel.All);
				BepInExLogger.Listeners.Add(logListener);
			}
			return ExtremeRolesTestPlugin.Instance.Log;
		}
	}
	private DiskLogListener? logListener;

	~TestRunnerBase()
	{
		if (logListener != null)
		{
			BepInExLogger.Listeners.Remove(logListener);
		}
	}

	public abstract void Run();

	public abstract void Export();
}
