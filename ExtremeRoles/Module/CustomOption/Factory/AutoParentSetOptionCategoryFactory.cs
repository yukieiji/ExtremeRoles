using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;

using System;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class AutoParentSetOptionCategoryFactory : IDisposable
{
	private readonly AutoActivatorSetFactory internalFactory;

	public IOptionActivator? Activator => this.internalFactory.Activator;

	public AutoParentSetOptionCategoryFactory(
		in OptionCategoryFactory factory,
		in IOption? parent = null)
	{
		this.internalFactory = new AutoActivatorSetFactory(factory);
		this.internalFactory.Activator = parent is null ? null : new ParentActive(parent);
	}

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

	public IOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		var newOption = this.internalFactory.CreateBoolOption(
			option,
			defaultValue,
			activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		var newOption = this.internalFactory.CreateFloatOption(
			option,
			defaultValue,
			min, max, step,
			activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateFloatDynamicMaxOption<T>(
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
		var newOption = this.internalFactory.CreateFloatDynamicMaxOption(
			option,
			defaultValue,
			min, step,
			checkValueOption,
			activator,
			isHidden,
			format,
			tempMaxValue,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		var newOption = this.internalFactory.CreateIntOption(
			option,
			defaultValue,
			min, max, step,
			activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateIntDynamicMaxOption<T>(
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
		var newOption = this.internalFactory.CreateIntDynamicMaxOption(
			option,
			defaultValue,
			min, step,
			checkValueOption,
			activator,
			isHidden,
			format,
			tempMaxValue,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false) where T : struct, IConvertible
	{
		var newOption = this.internalFactory.CreateSelectionOption(
			option,
			selections,
			activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateSelectionOption<T, W>(
		T option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		var newOption = this.internalFactory.CreateSelectionOption<T, W>(
			option,
			activator,
			isHidden,
			format,
			ignorePrefix);
		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateOption<T>(
		T option,
		IValueHolder holder,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool ignorePrefix = false)
		where T : struct, IConvertible
	{
		var newOption = this.internalFactory.CreateOption(option, holder, activator, isHidden, format, ignorePrefix);
		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public IOption Create0To100Percentage10StepOption<T>(
		T option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		bool ignorePrefix = false,
		int defaultGage = 0) where T : struct, IConvertible
	{
		var newOption = this.internalFactory.Create0To100Percentage10StepOption(option, activator, isHidden, ignorePrefix, defaultGage);
		if (this.internalFactory.Activator is null)
		{
			this.internalFactory.Activator = new ParentActive(newOption);
		}
		return newOption;
	}

	public void Dispose()
	{
		this.internalFactory.Dispose();
	}
}
