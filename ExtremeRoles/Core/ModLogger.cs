using BepInEx.Logging;
using ExtremeRoles.Core.Abstract;

namespace ExtremeRoles.Core;

public sealed class BepInExLogger : IModLogger
{
	private readonly ManualLogSource logger = ExtremeRolesPlugin.Logger;
	private readonly bool isDebug = ExtremeRolesPlugin.DebugMode.Value;

	public void LogDebug(object data)
		=> logger.LogDebug(data);

	public void LogError(object data)
		=> logger.LogError(data);

	public void LogFatal(object data)
		=> logger.LogFatal(data);

	public void LogInfo(object data)
		=> logger.LogInfo(data);

	public void LogMessage(object data)
		=> logger.LogMessage(data);

	public void LogWarning(object data)
		=> logger.LogWarning(data);

	public void LogTrace(object data)
	{

#if DEBUG
		if (ExtremeRolesPlugin.DebugMode.Value)
		{
			logger.LogInfo(data);
		}
#endif
	}
}
