using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Helper;



#nullable enable

namespace ExtremeRoles.Module.CustomOption.OLDS.Implemented.Factories;


public class SequentialOptionFactory : SimpleFactory
{
	public int StartId => GetOptionId(0);
	public int EndId => GetOptionId(Offset - 1);

	public int Offset { private get; set; }

	public SequentialOptionFactory(
		int idOffset = 0,
		string namePrefix = "",
		OptionTab tab = OptionTab.General) : base(idOffset, namePrefix, tab)
	{
		Offset = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption(
		object option,
		float defaultValue,
		float min, float max, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		=> new FloatCustomOption(
				getOptionIdAndUpdate(),
				getOptionName(option, color, ignorePrefix),
				defaultValue,
				min, max, step,
				parent, isHeader, isHidden,
				format, invert, enableCheckOption, Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption(
		object option,
		float defaultValue,
		float min, float step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
		=> new FloatDynamicCustomOption(
			getOptionIdAndUpdate(),
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption(
		object option,
		int defaultValue,
		int min, int max, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		=> new IntCustomOption(
			getOptionIdAndUpdate(),
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption(
		object option,
		int defaultValue,
		int min, int step,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
		=> new IntDynamicCustomOption(
			getOptionIdAndUpdate(),
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			Tab, tempMaxValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption(
		object option,
		bool defaultValue,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		=> new BoolCustomOption(
			getOptionIdAndUpdate(),
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption(
		object option,
		string[] selections,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		=> new SelectionCustomOption(
			getOptionIdAndUpdate(),
			getOptionName(option, color, ignorePrefix),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, Tab);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<W>(
		object option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		where W : struct, IConvertible
		=> new SelectionCustomOption(
				getOptionIdAndUpdate(),
				getOptionName(option, color, ignorePrefix),
				GetEnumString<W>().ToArray(),
				parent, isHeader, isHidden,
				format, invert, enableCheckOption, Tab);

	private string getOptionName(object option, Color? color, bool ignorePrefix = false)
	{
		string optionName = ignorePrefix ? $"|{NamePrefix}|{option}" : $"{NamePrefix}{option}";

		return !color.HasValue ? optionName : Design.ColoedString(color.Value, optionName);
	}
	private int getOptionIdAndUpdate()
	{
		int optionId = GetOptionId(Offset);
		Offset++;
		return optionId;
	}
}
