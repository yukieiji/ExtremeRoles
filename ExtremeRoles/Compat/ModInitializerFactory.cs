using System;

using BepInEx;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Core.Abstract;


#nullable enable

namespace ExtremeRoles.Compat;

public class ModInitializerFactory(IAccessTool accessTool, IModLogger logger, IHarmonyPatchProvider provider) : IModInitializerFactory
{
	private readonly IModLogger modLogger = logger;
	private readonly IHarmonyPatchProvider harmonyPatchProvider = provider;
	private readonly IAccessTool accessTool = accessTool;

	public IInitializer? Create(Type initializerType, PluginInfo plugin)
	{
		var patch = harmonyPatchProvider.Get(plugin);
		object? instance = Activator.CreateInstance(initializerType, [plugin, patch, accessTool]);
		if (instance == null)
		{
			modLogger.LogError($"{initializerType.FullName} can't create instance");
			return null;
		}

		if (instance is not IInitializer initializer)
		{
			modLogger.LogError(
				$"ModIntegratorType '{initializerType.FullName}' : NOT IMP IInitializer!!");
			return null;
		}
		return initializer;
	}
}
