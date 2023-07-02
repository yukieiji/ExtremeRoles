using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace ExtremeRoles.Compat.ModIntegrator;

#nullable enable

public abstract class ModIntegratorBase
{
    public readonly SemanticVersioning.Version Version;
    protected BasePlugin? Plugin;
    protected Assembly Dll;
    protected Type[] ClassType;

	internal ModIntegratorBase(
		string guid, PluginInfo plugin)
	{
		this.Plugin = plugin!.Instance as BasePlugin;
		this.Version = plugin.Metadata.Version;
		this.Dll = Plugin!.GetType().Assembly;
		this.ClassType = AccessTools.GetTypesFromAssembly(this.Dll);

		this.PatchAll(new Harmony($"ExR.{guid}.Patch"));
	}

	public virtual void CreateIntegrateOption(int startOption) { }

    protected abstract void PatchAll(Harmony harmony);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static SelectionCustomOption CreateSlectionOption<T, SelectionEnum>(
		int statOptionIndex,
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		where SelectionEnum : struct, IConvertible
	{
		return new SelectionCustomOption(
			Convert.ToInt32(option) + statOptionIndex,
			option.ToString(),
			Enum.GetValues(typeof(SelectionEnum))
				.Cast<SelectionEnum>()
				.Select(x => x.ToString())
				.ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			OptionTab.General);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static FloatCustomOption CreateFloatOption<T>(
		int statOptionIndex,
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null) where T : struct, IConvertible
	{
		return new FloatCustomOption(
			Convert.ToInt32(option) + statOptionIndex,
			option.ToString(),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			OptionTab.General);
	}
}
