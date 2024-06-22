using System;

using UnityEngine;

using OptionTab = ExtremeRoles.Module.CustomOption.OptionTab;
using OptionUnit = ExtremeRoles.Module.CustomOption.OptionUnit;

using ExtremeRoles.Module.NewOption.Interfaces;
using ExtremeRoles.Module.NewOption.Implemented;

#nullable enable

namespace ExtremeRoles.Module.NewOption.Factory;

public sealed class AutoParentSetOptionCategoryFactory(
	in OptionCategoryFactory factory,
	in IOption? parent = null) : IDisposable
{
	private IOption? parent = parent;
	private readonly OptionCategoryFactory internalFactory = factory;

	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		BoolCustomOption newOption = this.internalFactory.CreateBoolOption(
			option,
			defaultValue,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public FloatCustomOption CreateBoolOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		FloatCustomOption newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public FloatCustomOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		FloatCustomOption newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public FloatDynamicCustomOption CreateFloatDynamicOption<T>(
		T option,
		float defaultValue,
		float min, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		float tempMaxValue = 0.0f,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		FloatDynamicCustomOption newOption = this.internalFactory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			tempMaxValue,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public IntCustomOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		IntCustomOption newOption = this.internalFactory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public IntDynamicCustomOption CreateIntDynamicOption<T>(
		T option,
		int defaultValue,
		int min, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		int tempMaxValue = 0,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		IntDynamicCustomOption newOption = this.internalFactory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			tempMaxValue,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public SelectionCustomOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption(
			option,
			selections,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public SelectionCustomOption CreateSelectionOption<T, W>(
		T option,
		IOption? parent = null,
		bool isHeader = false,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		Color? color = null,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption<T, W>(
			option,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			color,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public void Dispose()
	{
		this.internalFactory.Dispose();
	}
}
