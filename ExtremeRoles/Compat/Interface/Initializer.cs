using System;
using HarmonyLib;

using System.Reflection;

using BepInEx.Unity.IL2CPP;
using ExtremeRoles.Compat.ModIntegrator;

#nullable enable

namespace ExtremeRoles.Compat.Interface;

public interface IInitializer
{
	public Assembly Dll { get; }
	public Harmony Patch { get; }
	public BasePlugin Plugin { get; }

	public SemanticVersioning.Version Version { get; }
	public string Name { get; }

	public ModIntegratorBase Initialize();

	public Type GetClass(string name);
	public MethodInfo GetMethod(string className, string methodName, Type[]? param = null);
	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null);
}
