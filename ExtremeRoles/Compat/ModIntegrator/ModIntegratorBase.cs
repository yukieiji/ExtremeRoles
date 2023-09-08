using System;
using System.Reflection;

using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

using OptionFactory = ExtremeRoles.Module.CustomOption.Factorys.SequentialOptionFactory;

namespace ExtremeRoles.Compat.ModIntegrator;

#nullable enable

public abstract class ModIntegratorBase
{
    public readonly SemanticVersioning.Version Version;
	public string Name => MetadataHelper.GetMetadata(Plugin).GUID;

    protected BasePlugin? Plugin;
    protected Assembly Dll;
    protected Type[] ClassType;

	internal ModIntegratorBase(
		string guid, PluginInfo plugin)
	{
		this.Plugin = plugin.Instance as BasePlugin;
		this.Version = plugin.Metadata.Version;
		this.Dll = Plugin!.GetType().Assembly;
		this.ClassType = AccessTools.GetTypesFromAssembly(this.Dll);

		this.PatchAll(new Harmony($"ExR.{guid}.Patch"));
	}

	public virtual void CreateIntegrateOption(OptionFactory factory) { }

    protected abstract void PatchAll(Harmony harmony);
}
