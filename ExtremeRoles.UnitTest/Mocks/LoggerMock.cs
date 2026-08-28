using BepInEx.Logging;
using ExtremeRoles;
using System.Reflection;

namespace ExtremeRoles.UnitTest.Mocks;

public class LoggerMock : ISerialMockSetup
{
	private readonly string _loggerName;

	public LoggerMock(string loggerName = "UnitTest")
	{
		_loggerName = loggerName;
	}

	public void Setup()
	{
		var loggerField = typeof(ExtremeRolesPlugin).GetField("Logger", BindingFlags.NonPublic | BindingFlags.Static);
		if (loggerField != null && loggerField.GetValue(null) == null)
		{
			loggerField.SetValue(null, BepInEx.Logging.Logger.CreateLogSource(_loggerName));
		}
	}
}
