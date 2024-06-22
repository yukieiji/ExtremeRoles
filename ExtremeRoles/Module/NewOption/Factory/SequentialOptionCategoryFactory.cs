using ExtremeRoles.Helper;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;
using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public sealed class SequentialOptionCategoryFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.General) :
	OptionCategoryFactory(name, groupId, action, tab)
{
	public int StartId => 0;
	public int EndId => this.Offset - 1;

	public int Offset { private get; set; } = 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption(
		object option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = new BoolCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption(
		object option,
		float defaultValue,
		float min, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = new FloatDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption(
		object option,
		int defaultValue,
		int min, int max, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = new IntCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, max, step,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption(
		object option,
		int defaultValue,
		int min, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = new IntDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption(
		object option,
		string[] selections,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = new SelectionCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			selections,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<W>(
		object option,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false)
		where W : struct, Enum
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, color, ignorePrefix);

		var opt = SelectionCustomOption.CreateFromEnum<W>(
			new OptionInfo(optionId, name, format, isHidden),
			OptionRelationFactory.Create(parent, invert));

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
