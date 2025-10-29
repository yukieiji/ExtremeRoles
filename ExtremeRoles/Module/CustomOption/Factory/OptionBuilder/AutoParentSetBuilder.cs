using System;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;

#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;

public sealed class AutoParentSetBuilder(
	in DefaultBuilder builder,
	in IOption? parent = null) : IOptionBuilder
{
	private IOption? parent = parent;
	private readonly DefaultBuilder builder = builder;

	public int IdOffset
	{
		set
		{
			this.builder.IdOffset = value;
		}
	}
	public string OptionPrefix
	{
		set
		{
			this.builder.OptionPrefix = value;
		}
	}

	public OptionPack Option => this.builder.Option;

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
		var newOption = this.builder.CreateBoolOption(
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
		var newOption = this.builder.CreateFloatOption(
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
		var newOption = this.builder.CreateFloatOption(
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
		var newOption = this.builder.CreateFloatDynamicOption(
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
		var newOption = this.builder.CreateIntOption(
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
		var newOption = this.builder.CreateIntDynamicOption(
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
		var newOption = this.builder.CreateSelectionOption(
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
		var newOption = this.builder.CreateSelectionOption<T, W>(
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
		var newOption = this.builder.CreateSelectionOption<T, W>(
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
		bool ignorePrefix = false,
		int defaultGage = 0) where T : struct, IConvertible
		=> CreateIntOption(
			option,
			defaultGage, 0, SingleRoleSpawnData.MaxSpawnRate, 10,
			parent,
			isHidden,
			OptionUnit.Percentage,
			invert,
			ignorePrefix);
}
