using System.Diagnostics.CodeAnalysis;

using BepInEx;
using BepInEx.Unity.IL2CPP;

using ExtremeRoles.Compat.Interface;

#nullable enable

namespace ExtremeRoles.Compat;

public class BepInExPluginLoader : IPluginLoader
{
	public bool TryGetPlugin(string guid, [NotNullWhen(true)] out PluginInfo? plugin)
	{
		plugin = null;
		return IL2CPPChainloader.Instance != null && IL2CPPChainloader.Instance.Plugins.TryGetValue(guid, out plugin);
	}
}
