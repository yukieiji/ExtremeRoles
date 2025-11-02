using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Value;
using ExtremeRoles.Module.CustomOption.Interfaces;

using ExROption = ExtremeRoles.Module.CustomOption.Implemented.CustomOption;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public class OptionCategoryFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.GeneralTab,
	in Color? color = null) : IDisposable
{
	public string Name { get; set; } = name;
	public string OptionPrefix { protected get; set; } = name;

	public OptionTab Tab { get; } = tab;
	public int IdOffset { protected get; set; } = 0;
	protected readonly Regex NameCleaner = new Regex(@"(\|)|(<.*?>)|(\\n)", RegexOptions.Compiled);

	private readonly Color? color = color;
	private readonly int groupid = groupId;
	private readonly Action<OptionTab, OptionCategory> registerOption = action;
	private readonly OptionPack optionPack = new OptionPack();

	public IOption Get(int id)
		=> this.optionPack.Get(id);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false
	) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);
		var boolRange = new BoolOptionValue(defaultValue);

		return CreateOption(optionId, name, format, isHidden, boolRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false
	) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var floatRange = new FloatOptionValue(defaultValue, min, max, step);

		return CreateOption(optionId, name, format, isHidden, floatRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOption checkValueOption,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		float max = CreateMaxValue(min, step, defaultValue, tempMaxValue);
		var floatRange = new FloatOptionValue(defaultValue, min, max, step);

		var opt = CreateOption(optionId, name, format, isHidden, floatRange, activator);

		checkValueOption.OnValueChanged += (x) => {

			int prevSelection = floatRange.Selection;
			float newMax = checkValueOption.Value<float>();
			floatRange.InnerRange = OptionRange<float>.Create(min, newMax, step);
			floatRange.Selection = prevSelection;
		};

		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false
	) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var intRange = new IntOptionValue(defaultValue, min, max, step);

		return CreateOption(optionId, name, format, isHidden, intRange, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOption checkValueOption,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		int tempMaxValue = 0,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		int max = CreateMaxValue(min, step, defaultValue, tempMaxValue);
		var intRange = new IntOptionValue(defaultValue, min, max, step);

		var opt = CreateOption(optionId, name, format, isHidden, intRange, activator);

		checkValueOption.OnValueChanged += (x) => {

			int prevSelection = intRange.Selection;
			int newMax = checkValueOption.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(min, newMax, step);
			intRange.Selection = prevSelection;

		};
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var selection = new SelectionOptionValue(selections);

		return CreateOption(optionId, name, format, isHidden, selection, activator);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IOption CreateSelectionOption<T, W>(
		T option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var selection = SelectionOptionValue.CreateFromEnum<W>();

		return CreateOption(optionId, name, format, isHidden, selection, activator);
	}

	public void AddOption(int id, IOption option)
	{
		optionPack.AddOption(id, option);
	}

	public int GetOptionId<T>(T option) where T : struct, IConvertible
	{
		enumCheck(option);
		return Convert.ToInt32(option) + IdOffset;
	}

	protected string GetOptionName<T>(T option, bool ignorePrefix = false) where T : struct, IConvertible
	{
		string cleanedName = this.NameCleaner.Replace(this.OptionPrefix, string.Empty).Trim();

		return ignorePrefix ? $"|{cleanedName}|{option}" : $"{cleanedName}{option}";
	}

	protected static string GetColoredOptionName<T>(T option, Color? color) where T : struct, IConvertible
	{
		string? optionName = option.ToString();
		if (string.IsNullOrEmpty(optionName))
		{
			throw new ArgumentException("Can't convert string");
		}
		return !color.HasValue ? optionName : Design.ColoredString(color.Value, optionName);
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

	public void Dispose()
	{
		var newGroup = new OptionCategory(this.Tab, groupid, this.Name, this.optionPack, this.color);
		this.registerOption(Tab, newGroup);
	}

	protected static int CreateMaxValue(int min, int step, int defaultValue, int tempMaxValue)
		=> tempMaxValue == 0 ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;
	protected static float CreateMaxValue(float min, float step, float defaultValue, float tempMaxValue)
		=> tempMaxValue == 0.0f ?
			min + step < defaultValue ? defaultValue : min + step :
			tempMaxValue;

	protected IOption CreateOption(
		int id,
		string name,
		OptionUnit format,
		bool isHidden,
		IValueHolder holder,
		IOptionActivator? activator = null)
	{
		var info = new OptionInfo(id, name, format, isHidden);
		var opt = new ExROption(info, holder, activator);
		this.AddOption(id, opt);

		return opt;
	}
}
