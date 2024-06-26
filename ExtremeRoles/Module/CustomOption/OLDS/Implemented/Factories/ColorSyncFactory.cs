using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.OLDS.Implemented.Factories;

public sealed class ColorSyncFactory : SimpleFactory
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
			GetOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

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
			GetOptionName(option, color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			Tab, tempMaxValue);

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
			GetOptionName(option, color),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

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
			GetOptionName(option, color),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			Tab, tempMaxValue);

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
			GetOptionName(option, color),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

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
			GetOptionName(option, color),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

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
			GetOptionName(option, color),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);
}
