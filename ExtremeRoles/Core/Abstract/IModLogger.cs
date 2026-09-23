namespace ExtremeRoles.Core.Abstract;

public interface IModLogger
{
	public void LogFatal(object data);

	public void LogError(object data);

	public void LogWarning(object data);

	public void LogMessage(object data);


	public void LogInfo(object data);

	public void LogDebug(object data);

	public void LogTrace(object data);
}
