using ExtremeRoles.Helper;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

#nullable enable

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Module.NewOption.Factory;

public sealed class SequentialOptionGroupFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.General) :
	OptionGroupFactory(name, groupId, action, tab)
{
	public int StartId => 0;
	public int EndId => this.Offset - 1;

	public int Offset { private get; set; } = 0;

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
	{
		int optionId = getOptionIdAndUpdate();
		var opt = new FloatCustomOption(
				optionId,
				getOptionName(option, color, ignorePrefix),
				defaultValue,
				min, max, step,
				parent, isHeader, isHidden,
				format, invert, enableCheckOption, this.Tab);
		this.AddOption(optionId, opt);
		return opt;
	}

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

	{
		int optionId = getOptionIdAndUpdate();
		var opt = new FloatDynamicCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

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
	{
		int optionId = getOptionIdAndUpdate();
		var opt = new IntCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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
	{
		int optionId = getOptionIdAndUpdate();
		var opt = new IntDynamicCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

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
	{
		int optionId = getOptionIdAndUpdate();
		var opt = new BoolCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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
	{
		int optionId = getOptionIdAndUpdate();
		var opt = new SelectionCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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

	{
		int optionId = getOptionIdAndUpdate();
		var opt = new SelectionCustomOption(
			optionId,
			getOptionName(option, color, ignorePrefix),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

	private string getOptionName(object option, Color? color, bool ignorePrefix = false)
	{
		string optionName = ignorePrefix ? $"|{this.Name}|{option}" : $"{this.Name}{option}";

		return !color.HasValue ? optionName : Design.ColoedString(color.Value, optionName);
	}
	private int getOptionIdAndUpdate()
	{
		int optionId = this.Offset;
		this.Offset++;
		return optionId;
	}
}
