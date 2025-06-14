
using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using ExtremeRoles.Test.Helper;

namespace ExtremeRoles.Test.Lobby;

public class LobbyTestStep(IServiceProvider provider) : TestStepBase
{
	// 順番を変えるとなぜか動くので一番最初においておく
	private readonly IEnumerable<ILobbyTestRunner> runners = provider.GetServices<ILobbyTestRunner>();

	public static void Register(
		IServiceCollection services,
		Assembly dll)
	{
		var runnerType = typeof(ILobbyTestRunner);
		foreach (var type in dll.GetTypes())
		{
			if (!runnerType.IsAssignableFrom(type) ||
				type.IsInterface || type.IsAbstract)
			{
				continue;
			}

			object? obj = Activator.CreateInstance(type);
			if (obj is not ILobbyTestRunner runner)
			{
				continue;
			}
			if (runner.IsDebugOnly)
			{
#if DEBUG
				services.AddTransient(runnerType, type);
#endif
			}
			else
			{
				services.AddTransient(runnerType, type);
			}
		}
	}

	public override IEnumerator Run()
	{
		foreach (var runner in runners)
		{
			yield return runner.Run();
			yield return GameUtility.WaitForStabilize();
		}
	}
}
