using ExtremeRoles.Helper;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using UnityEngine;

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;
using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;


#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public class OptionCategoryFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.General,
	in Color? color = null) : IDisposable
{
	public string Name { get; set; } = name;

	public OptionTab Tab { get; } = tab;
	public int IdOffset { protected get; set; } = 0;
	protected readonly Regex NameCleaner = new Regex(@"(\|)|(<.*?>)|(\\n)", RegexOptions.Compiled);

	private readonly Color? color = color;
	private readonly int groupid = groupId;
	private readonly Action<OptionTab, OptionCategory> registerOption = action;
	private readonly OptionPack optionPack = new OptionPack();

	public IOption Get(int id)
		=> this.optionPack.Get(id);
	public IValueOption<T> Get<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.optionPack.Get<T>(id);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new BoolCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new FloatCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, max, step,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new FloatDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new IntCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, max, step,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		int tempMaxValue = 0,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new IntDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = new SelectionCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			selections,
			OptionRelationFactory.Create(parent, invert));

		this.AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		int optionId = GetOptionId(option);
		string name = GetOptionName(option, ignorePrefix);

		var opt = SelectionCustomOption.CreateFromEnum<W>(
			new OptionInfo(optionId, name, format, isHidden),
			OptionRelationFactory.Create(parent, invert));

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
		return Convert.ToInt32(option) + IdOffset;
	}

	protected string GetOptionName<T>(T option, bool ignorePrefix = false) where T : struct, IConvertible
	{
		string cleanedName = this.NameCleaner.Replace(this.Name, string.Empty).Trim();

		return ignorePrefix ? $"|{cleanedName}|{option}" : $"{cleanedName}{option}";
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
		var newGroup = new OptionCategory(this.Tab, groupid, this.Name, this.optionPack, this.color);
		this.registerOption(Tab, newGroup);
	}
}
