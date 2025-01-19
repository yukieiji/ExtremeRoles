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
	public Harmony Patch { get; }
	public BasePlugin Plugin { get; }

	public SemanticVersioning.Version Version { get; }
	public string Name => MetadataHelper.GetMetadata(Plugin).GUID;


	private readonly Type[] classType;

	public InitializerBase(PluginInfo plugin)
	{
		this.Plugin = (BasePlugin)plugin.Instance;
		this.Version = plugin.Metadata.Version;
		this.Dll = Plugin!.GetType().Assembly;
		this.classType = AccessTools.GetTypesFromAssembly(this.Dll);
		this.Patch = new Harmony($"ExR.{plugin.Metadata.GUID}.Patch");
	}

	public ModIntegratorBase Initialize()
	{
		this.PatchAll(this.Patch);
		object? integrator = Activator.CreateInstance(typeof(T), [this]);
		if (integrator is T mod)
		{
			return mod;
		}
		throw new Exception($"Failed to create instance of {typeof(T).Name}");
	}

	public Type GetClass(string name)
		=> this.classType.First(t => t.Name == name);

	public MethodInfo　GetMethod(string className, string methodName, Type[]? param = null)
	{
		Type classType = this.classType.First(t => t.Name == className);
		return GetMethod(classType, methodName, param);
	}

	public MethodInfo GetMethod(Type fromType, string methodName, Type[]? param = null)
		=> AccessTools.Method(fromType, methodName, param);

	protected MethodInfo CreatePatchMethod(Action func)
		=> SymbolExtensions.GetMethodInfo(() => func.Invoke());

	protected MethodInfo CreatePatchMethod<W>(Action<W> func)
	{
		W? instance = default;
#pragma warning disable CS8604
		return SymbolExtensions.GetMethodInfo(() => func.Invoke(instance));
#pragma warning restore CS8604
	}

	protected MethodInfo CreatePatchMethod<W>(Func<W, bool> func)
	{
		W? instance = default;
#pragma warning disable CS8604
		return SymbolExtensions.GetMethodInfo(() => func.Invoke(instance));
#pragma warning restore CS8604
	}

	protected abstract void PatchAll(Harmony harmony);
}
