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
	in Action<IOption, IOption> childRegister,
	in Action<OptionTab, OptionCategory> categoryRegister,
	OptionTab tab = OptionTab.GeneralTab,
	in Color? color = null) :
	OptionCategoryFactory(name, groupId, childRegister, categoryRegister, tab, color)
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

		var boolRange = ValueHolderAssembler.CreateBoolValue(defaultValue);
	
		return CreateOption(optionId, name, format, isHidden, boolRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateFloatDynamicMaxOption(
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

		var floatRange = ValueHolderAssembler.CreateDynamicFloatValue(defaultValue, min, step, tempMaxValue);

		var opt = CreateOption(optionId, name, format, isHidden, floatRange, activator);

		checkValueOption.OnValueChanged += () => {
			float newMax = checkValueOption.Value<float>();
			floatRange.InnerRange = OptionRange<float>.Create(min, newMax, step);

			// Selectionを再設定
			opt.Selection = floatRange.Selection;
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

		var intRange = ValueHolderAssembler.CreateIntValue(defaultValue, min, max, step);

		return CreateOption(optionId, name, format, isHidden, intRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateIntDynamicMaxOption(
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

		var intRange = ValueHolderAssembler.CreateDynamicIntValue(defaultValue, min, step, tempMaxValue);

		var opt = CreateOption(optionId, name, format, isHidden, intRange, activator);

		checkValueOption.OnValueChanged += () => {
			int newMax = checkValueOption.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(min, newMax, step);

			// Selectionを再設定
			opt.Selection = intRange.Selection;
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
