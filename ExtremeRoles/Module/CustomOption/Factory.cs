using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Helper;
using System.Linq;

namespace ExtremeRoles.Module.CustomOption;

#nullable enable

public sealed class ColorSyncFactory : Factory
{
	private Color color = Color.clear;

	public ColorSyncFactory(
		Color color,
		int idOffset = 0,
		string namePrefix = "",
		OptionTab tab = OptionTab.General) : base(idOffset, namePrefix, tab)
	{
		this.color = color;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		=> new FloatCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		float tempMaxValue = 0.0f)
		where T : struct, IConvertible
		=> new FloatDynamicCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		=> new IntCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		int tempMaxValue = 0)
		where T : struct, IConvertible
		=> new IntDynamicCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		=> new BoolCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		=> new SelectionCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null)
		where T : struct, IConvertible
		where W : struct, IConvertible
		=> new SelectionCustomOption(
			GetOptionId(option),
			GetOptionName(option, this.color),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

}



public class Factory
{
	protected OptionTab Tab;
	private int idOffset = 0;
	private string namePrefix;

	public Factory(
		int idOffset = 0,
		string namePrefix = "",
		OptionTab tab = OptionTab.General)
	{
		this.idOffset = idOffset;
		this.namePrefix = namePrefix;
		this.Tab = tab;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		OptionTab tab = OptionTab.General) where T : struct, IConvertible
		=> new BoolCustomOption(
			Convert.ToInt32(option),
			GetColoredOptionName(option, color),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		OptionTab tab = OptionTab.General) where T : struct, IConvertible
		=> new FloatCustomOption(
			Convert.ToInt32(option),
			GetColoredOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		OptionTab tab = OptionTab.General) where T : struct, IConvertible
		=> new IntCustomOption(
			Convert.ToInt32(option),
			GetColoredOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		OptionTab tab = OptionTab.General) where T : struct, IConvertible
		=> new SelectionCustomOption(
			Convert.ToInt32(option),
			GetColoredOptionName(option, color),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		OptionTab tab = OptionTab.General)
		where T : struct, IConvertible
		where W : struct, IConvertible
		=> new SelectionCustomOption(
			Convert.ToInt32(option),
			GetColoredOptionName(option, color),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null) where T : struct, IConvertible
		=> new FloatCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		float tempMaxValue = 0.0f) where T : struct, IConvertible
		=> new FloatDynamicCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null) where T : struct, IConvertible
		=> new IntCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		int tempMaxValue = 0) where T : struct, IConvertible
		=> new IntDynamicCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null) where T : struct, IConvertible
		=> new BoolCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null) where T : struct, IConvertible
		=> new SelectionCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null)
		where T : struct, IConvertible
		where W : struct, IConvertible
		=> new SelectionCustomOption(
			GetOptionId(option),
			GetOptionName(option, color),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

	public int GetOptionId<T>(T option) where T : struct, IConvertible
	{
		enumCheck(option);
		return GetOptionId(Convert.ToInt32(option));
	}

	public int GetOptionId(int option) => this.idOffset + option;

	protected string GetOptionName<T>(T option, Color? color) where T : struct, IConvertible
	{
		string optionName = string.Concat(this.namePrefix, option.ToString());

		return !color.HasValue ? optionName : Design.ColoedString(color.Value, optionName);
	}

	protected static string GetColoredOptionName<T>(T option, Color? color) where T : struct, IConvertible
	{
		string? optionName = option.ToString();
		if (string.IsNullOrEmpty(optionName))
		{
			throw new ArgumentException("Can't convert string");
		}
		return !color.HasValue ? optionName : Design.ColoedString(color.Value, optionName);
	}

	protected static IEnumerable<string> GetEnumString<T>()
	{
		foreach (object enumValue in Enum.GetValues(typeof(T)))
		{
			string? valuse = enumValue.ToString();
			if (string.IsNullOrEmpty(valuse)) { continue; }

			yield return valuse;
		}
	}

	private static void enumCheck<T>(T isEnum) where T : struct, IConvertible
	{
		if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(T))))
		{
			throw new ArgumentException(nameof(T));
		}
	}
}
