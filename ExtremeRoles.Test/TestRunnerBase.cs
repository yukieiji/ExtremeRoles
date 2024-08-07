﻿using System.IO;

using BepInEx;
using BepInEx.Logging;

using BepInExLogger = BepInEx.Logging.Logger;

namespace ExtremeRoles.Test;

public abstract class TestRunnerBase
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

	public static void Run<T>() where T : TestRunnerBase, new()
	{
		T runner = new T();
		runner.Run();
	}

	public abstract void Run();
}
