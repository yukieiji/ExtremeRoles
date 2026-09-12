using System.Diagnostics.CodeAnalysis;
using BepInEx;

namespace ExtremeRoles.Compat.Interface;

#nullable enable

public interface IPluginLoader
{
	public bool TryGetPlugin(string guid, [NotNullWhen(true)] out PluginInfo? plugin);
}