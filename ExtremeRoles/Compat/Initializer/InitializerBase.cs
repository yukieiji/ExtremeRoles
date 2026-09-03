using System;
using System.Linq;
using System.Reflection;

using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;



using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Compat.ModIntegrator;

namespace ExtremeRoles.Compat.Initializer;

#nullable enable

public abstract class InitializerBase<T> : IInitializer
	where T : ModIntegratorBase
{
	public Assembly Dll { get; }
	public IHarmonyPatch Patch { get; }
	public BasePlugin Plugin { get; }

	public SemanticVersioning.Version Version { get; }
	public string Name => MetadataHelper.GetMetadata(Plugin).GUID;

	private readonly IAccessTool tool;
	private readonly Type[] classType;

	public InitializerBase(PluginInfo plugin, IAccessTool accessTool, IHarmonyPatch patch)
	{
		this.Plugin = (BasePlugin)plugin.Instance;
		this.Version = plugin.Metadata.Version;
		this.Dll = Plugin!.GetType().Assembly;
		this.tool = accessTool;
		this.Patch = patch;
		this.classType = accessTool.GetTypesFromAssembly(this.Dll);
	}

	public Type GetClass(string name)
		=> this.classType.First(t => t.Name == name);

	public MethodInfo GetMethod(string className, string methodName, Type[]? param = null)
	{
		Type classType = this.classType.First(t => t.Name == className);
		return GetMethod(classType, methodName, param);
	}

	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null)
		=> this.tool.GetMethod(fromType, methodName, param);
	public PropertyInfo GetProperty(Type fromType, string fieldName)
		=> this.tool.GetProperty(fromType, fieldName);

	public FieldInfo GetField(Type fromType, string fieldName)
		=> this.tool.GetField(fromType, fieldName);

	public ModIntegratorBase Initialize()
	{
		this.PatchAll(this.tool, this.Patch);
		object? integrator = Activator.CreateInstance(typeof(T), [this, this.tool]);
		if (integrator is T mod)
		{
			return mod;
		}
		throw new Exception($"Failed to create instance of {typeof(T).Name}");
	}

	protected abstract void PatchAll(IAccessTool accessTool, IHarmonyPatch patch);
}
