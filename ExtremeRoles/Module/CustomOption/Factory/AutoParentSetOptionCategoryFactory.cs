﻿using System;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class AutoParentSetOptionCategoryFactory(
	in OptionCategoryFactory factory,
	in IOption? parent = null) : IDisposable
{
	private IOption? parent = parent;
	private readonly OptionCategoryFactory internalFactory = factory;

	public int IdOffset
	{
		set
		{
			this.internalFactory.IdOffset = value;
		}
	}
	public string OptionPrefix
	{
		set
		{
			this.internalFactory.OptionPrefix = value;
		}
	}

	public IOption Get(int id)
		=> this.internalFactory.Get(id);
	public IValueOption<T> Get<T>(int id)
		where T :
			struct, IComparable, IConvertible,
			IComparable<T>, IEquatable<T>
		=> this.internalFactory.Get<T>(id);

	public BoolCustomOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		BoolCustomOption newOption = this.internalFactory.CreateBoolOption(
			option,
			defaultValue,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
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
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		SelectionCustomOption newOption = this.internalFactory.CreateSelectionOption(
			option,
			selections,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
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
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
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
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public SelectionMultiEnableCustomOption CreateSelectionOption<T, W>(
		T option,
		IReadOnlyList<W> anotherDefault,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		SelectionMultiEnableCustomOption newOption = this.internalFactory.CreateSelectionOption<T, W>(
			option,
			anotherDefault,
			parent is null ? this.parent : parent,
			isHidden,
			format,
			invert,
			ignorePrefix);

		if (this.parent is null)
		{
			this.parent = newOption;
		}
		return newOption;
	}

	public IntCustomOption Create0To100Percentage10StepOption<T>(
		T option,
		IOption? parent = null,
		bool isHidden = false,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
		=> CreateIntOption(
			option,
			0, 0, SingleRoleSpawnData.MaxSpawnRate, 10,
			parent,
			isHidden,
			OptionUnit.Percentage,
			invert,
			ignorePrefix);

	public void Dispose()
	{
		this.internalFactory.Dispose();
	}
}
