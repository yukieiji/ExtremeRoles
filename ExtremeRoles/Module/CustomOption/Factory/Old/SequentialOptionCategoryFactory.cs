using ExtremeRoles.Helper;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;




using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Implemented.Old;
using ExtremeRoles.Module.CustomOption.OLDS;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory.Old;

public sealed class SequentialOptionCategoryFactory(
	string name,
	int groupId,
	in Action<OptionTab, OptionCategory> action,
	OptionTab tab = OptionTab.GeneralTab,
	in Color? color = null) :
	OptionCategoryFactory(name, groupId, action, tab, color)
{
	public int StartId => 0;
	public int EndId => Offset - 1;

	public int Offset { private get; set; } = 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BoolCustomOption CreateBoolOption(
		object option,
		bool defaultValue,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = new BoolCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue,
			OptionRelationFactory.Create(parent, invert));

		AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FloatDynamicCustomOption CreateFloatDynamicOption(
		object option,
		float defaultValue,
		float min, float step,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = new FloatDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntCustomOption CreateIntOption(
		object option,
		int defaultValue,
		int min, int max, int step,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = new IntCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, max, step,
			OptionRelationFactory.Create(parent, invert));

		AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IntDynamicCustomOption CreateIntDynamicOption(
		object option,
		int defaultValue,
		int min, int step,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		int tempMaxValue = 0,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = new IntDynamicCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			defaultValue, min, step,
			OptionRelationFactory.Create(parent, invert),
			tempMaxValue);

		AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption(
		object option,
		string[] selections,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = new SelectionCustomOption(
			new OptionInfo(optionId, name, format, isHidden),
			selections,
			OptionRelationFactory.Create(parent, invert));

		AddOption(optionId, opt);
		return opt;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SelectionCustomOption CreateSelectionOption<W>(
		object option,
		IOldOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where W : struct, Enum
	{
		int optionId = getOptionIdAndUpdate();
		string name = getOptionName(option, ignorePrefix);

		var opt = SelectionCustomOption.CreateFromEnum<W>(
			new OptionInfo(optionId, name, format, isHidden),
			OptionRelationFactory.Create(parent, invert));

		AddOption(optionId, opt);
		return opt;
	}

	private string getOptionName(object option, bool ignorePrefix = false)
	{
		string cleanedName = NameCleaner.Replace(Name, string.Empty).Trim();

		return ignorePrefix ? $"|{cleanedName}|{option}" : $"{cleanedName}{option}";
	}

	private int getOptionIdAndUpdate()
	{
		int optionId = Offset + IdOffset;
		Offset++;
		return optionId;
	}
}
