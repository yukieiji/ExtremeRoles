
using ExtremeRoles.Test.InGame.GameLoop;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace ExtremeRoles.Test.Lobby;

public class LobbyTestStep(IServiceProvider provider) : TestStepBase
{
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
		var waitor = new WaitForSeconds(2.5f);
		foreach (var runner in runners)
		{
			runner.Run();
			yield return waitor;
		}
	}
}
