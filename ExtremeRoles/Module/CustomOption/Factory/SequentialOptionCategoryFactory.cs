using System;
using System.Runtime.CompilerServices;

using UnityEngine;

using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class SequentialOptionCategoryFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.GeneralTab,
	in Color? color = null) :
	OptionCategoryFactory(name, groupId, action, tab, color)
{
	public int StartId => 0;
	public int EndId => this.Offset - 1;

	public int Offset { private get; set; } = 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateBoolOption(
		object option,
		bool defaultValue,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var boolRange = new BoolOptionValue(defaultValue);
	
		return CreateOption(optionId, name, format, isHidden, boolRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateFloatDynamicOption(
		object option,
		float defaultValue,
		float min, float step,
		IOption checkValueOption,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		float max = CreateMaxValue(min, step, defaultValue, tempMaxValue);
		var floatRange = new FloatOptionValue(defaultValue, min, max, step);

		var opt = CreateOption(optionId, name, format, isHidden, floatRange, activator);

		checkValueOption.OnValueChanged += (x) => {

			int prevSelection = floatRange.Selection;
			float newMax = checkValueOption.GetValue<float>();
			floatRange.InnerRange = OptionRange<float>.Create(min, newMax, step);
			floatRange.Selection = prevSelection;
		};

		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateIntOption(
		object option,
		int defaultValue,
		int min, int max, int step,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var intRange = new IntOptionValue(defaultValue, min, max, step);

		return CreateOption(optionId, name, format, isHidden, intRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateIntDynamicOption(
		object option,
		int defaultValue,
		int min, int step,
		IOption checkValueOption,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		int max = CreateMaxValue(min, step, defaultValue, tempMaxValue);
		var intRange = new IntOptionValue(defaultValue, min, max, step);

		var opt = CreateOption(optionId, name, format, isHidden, intRange, activator);

		checkValueOption.OnValueChanged += (x) => {

			int prevSelection = intRange.Selection;
			int newMax = checkValueOption.GetValue<int>();
			intRange.InnerRange = OptionRange<int>.Create(min, newMax, step);
			intRange.Selection = prevSelection;

		};
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateSelectionOption(
		object option,
		string[] selections,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var selection = new SelectionOptionValue(selections);

		return CreateOption(optionId, name, format, isHidden, selection, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateSelectionOption<W>(
		object option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
		where W : struct, Enum
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var selection = SelectionOptionValue.CreateFromEnum<W>();

		return CreateOption(optionId, name, format, isHidden, selection, activator);
	}

	private string getOptionName(object option, bool ignorePrefix = false)
	{
		string cleanedName = this.NameCleaner.Replace(this.Name, string.Empty).Trim();

		return ignorePrefix ? $"|{cleanedName}|{option}" : $"{cleanedName}{option}";
	}

	private int getOptionIdAndUpdate()
	{
		int optionId = this.Offset + this.IdOffset;
		this.Offset++;
		return optionId;
	}
}
