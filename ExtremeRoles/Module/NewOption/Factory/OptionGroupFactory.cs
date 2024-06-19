using ExtremeRoles.Helper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public class OptionGroupFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionGroup> action,
	OptionTab tab = OptionTab.General) : IDisposable
{
	public string Name { get; set; } = name;

	public OptionTab Tab { get; } = tab;

	private readonly int groupid = groupId;
	private readonly Action<OptionTab, OptionGroup> registerOption = action;
	private readonly OptionPack optionPack = new OptionPack();

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
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new FloatCustomOption(
			optionId,
			GetOptionName(option, color, ignorePrefix),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new FloatDynamicCustomOption(
			optionId,
			GetOptionName(option, color, ignorePrefix),
			defaultValue,
			min, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption,
			this.Tab, tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

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
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new IntCustomOption(
			optionId,
			GetOptionName(option, color, ignorePrefix),
			defaultValue,
			min, max, step,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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
		int tempMaxValue = 0,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new IntDynamicCustomOption(
		   optionId,
		   GetOptionName(option, color, ignorePrefix),
		   defaultValue,
		   min, step,
		   parent, isHeader, isHidden,
		   format, invert, enableCheckOption,
		   this.Tab, tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}


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
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new BoolCustomOption(
			GetOptionId(option),
			GetOptionName(option, color, ignorePrefix),
			defaultValue,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

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
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new SelectionCustomOption(
			GetOptionId(option),
			GetOptionName(option, color, ignorePrefix),
			selections,
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOptionInfo? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		IOptionInfo? enableCheckOption = null,
		Color? color = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		var opt = new SelectionCustomOption(
			optionId,
			GetOptionName(option, color, ignorePrefix),
			GetEnumString<W>().ToArray(),
			parent, isHeader, isHidden,
			format, invert, enableCheckOption, this.Tab);
		this.AddOption(optionId, opt);
		return opt;
	}

	protected void AddOption<SelectionType>(
		int id,
		IValueOption<SelectionType> option) where SelectionType :
		struct, IComparable, IConvertible,
		IComparable<SelectionType>, IEquatable<SelectionType>
	{
		optionPack.AddOption(id, option);
	}

	public int GetOptionId<T>(T option) where T : struct, IConvertible
	{
		enumCheck(option);
		return Convert.ToInt32(option);
	}

	protected string GetOptionName<T>(T option, Color? color, bool ignorePrefix = false) where T : struct, IConvertible
	{
		string optionName = ignorePrefix ? $"|{this.Name}|{option}" : $"{this.Name}{option}";

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

	public void Dispose()
	{
		var newGroup = new OptionGroup(groupid, this.Name, optionPack);
		this.registerOption(Tab, newGroup);
	}
}
