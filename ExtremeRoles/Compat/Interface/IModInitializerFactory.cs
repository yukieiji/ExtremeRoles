using System;
using BepInEx;

namespace ExtremeRoles.Compat.Interface;

#nullable enable

public interface IModInitializerFactory
{
	public IInitializer? Create(Type initializerType, PluginInfo plugin);
}