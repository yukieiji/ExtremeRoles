using System;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factorys;

public sealed class AutoParentSetFactory
{
	private IOptionInfo? parent;
	private readonly SimpleFactory internalFactory;

	public int IdOffset
	{
		set
		{
			this.internalFactory.IdOffset = value;
		}
	}

	public string NamePrefix
	{
		get => this.internalFactory.NamePrefix;
		set
		{
			this.internalFactory.NamePrefix = value;
		}
	}

	public AutoParentSetFactory(
		int idOffset = 0,
		string namePrefix = "",
		OptionTab tab = OptionTab.General,
		IOptionInfo? parent = null)
	{
		this.parent = parent;
		this.internalFactory = new SimpleFactory(idOffset, namePrefix, tab);
	}

	public FloatCustomOption CreateBoolOption<T>(
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
		FloatCustomOption newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		FloatCustomOption newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		FloatDynamicCustomOption newOption = this.internalFactory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			tempMaxValue,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		IntCustomOption newOption = this.internalFactory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		IntDynamicCustomOption newOption = this.internalFactory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			tempMaxValue,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		BoolCustomOption newOption = this.internalFactory.CreateBoolOption(
			option,
			defaultValue,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption(
			option,
			selections,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}

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
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption<T, W>(
			option,
			parent is null ? this.parent : parent,
			isHeader,
			isHidden,
			format,
			invert,
			enableCheckOption,
			color,
			ignorePrefix);

		if (isHeader)
		{
			this.parent = newOption;
		}
		return newOption;
	}
}
