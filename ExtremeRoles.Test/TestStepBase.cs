using BepInEx;
using BepInEx.Logging;

using System.Collections;
using System.IO;

using BepInExLogger = BepInEx.Logging.Logger;

namespace ExtremeRoles.Test;

public abstract class TestStepBase : ITestStep
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

	~TestStepBase()
	{
		if (logListener != null)
		{
			BepInExLogger.Listeners.Remove(logListener);
		}
	}

	public abstract IEnumerator Run();
}
