using System;

using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Module.RoleAssign;


#nullable enable

namespace ExtremeRoles.Module.CustomOption.Factory;

public sealed class AutoParentSetOptionCategoryFactory(
	in OptionCategoryFactory factory,
	in IOption? parent = null) : IDisposable
{
	public IOptionActivator? Activator { get; private set; } = parent is null ? null : new DefaultParentActive(parent);
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

	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption CreateBoolOption<T>(
		T option,
		bool defaultValue,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
		=> CreateBoolOption(option, defaultValue, OptionActivatorFactory.Create(parent, invert), isHidden, format, ignorePrefix);
	
	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption CreateFloatOption<T>(
		T option,
		float defaultValue,
		float min, float max, float step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
		=> CreateFloatOption<T>(option, defaultValue, min, max, step, OptionActivatorFactory.Create(parent, invert), isHidden, format, ignorePrefix);

	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption CreateIntOption<T>(
		T option,
		int defaultValue,
		int min, int max, int step,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
		=> CreateIntOption(option, defaultValue, min, max, step, OptionActivatorFactory.Create(parent, invert), isHidden, format, ignorePrefix);


	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption CreateSelectionOption<T>(
		T option,
		string[] selections,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false) where T : struct, IConvertible
		=> CreateSelectionOption(option, selections, OptionActivatorFactory.Create(parent, invert), isHidden, format, ignorePrefix);

	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption CreateSelectionOption<T, W>(
		T option,
		IOption? parent = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
		=> CreateSelectionOption<T, W>(option, OptionActivatorFactory.Create(parent, invert), isHidden, format, ignorePrefix);

	[Obsolete("parentやignorePrefixを使わず、OptionActivatorを使用するように調整してください")]
	public IOption Create0To100Percentage10StepOption<T>(
		T option,
		IOption? parent = null,
		bool isHidden = false,
		bool invert = false,
		bool ignorePrefix = false,
		int defaultGage = 0)
		where T : struct, IConvertible
		=> Create0To100Percentage10StepOption(option, OptionActivatorFactory.Create(parent, invert), isHidden, ignorePrefix, defaultGage);

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
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
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
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
		}
		return newOption;
	}

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
		var newOption = this.internalFactory.CreateFloatDynamicOption(
			option,
			defaultValue,
			min, step,
			checkValueOption,
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			tempMaxValue,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
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
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
		}
		return newOption;
	}

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
		var newOption = this.internalFactory.CreateIntDynamicOption(
			option,
			defaultValue,
			min, step,
			checkValueOption,
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			tempMaxValue,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
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
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
		}
		return newOption;
	}

	public IOption CreateSelectionOption<T, W>(
		T option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		OptionUnit format = OptionUnit.None,
		bool invert = false,
		bool ignorePrefix = false)
		where T : struct, IConvertible
		where W : struct, Enum
	{
		var newOption = this.internalFactory.CreateSelectionOption<T, W>(
			option,
			activator is null ? this.Activator : activator,
			isHidden,
			format,
			ignorePrefix);

		if (this.Activator is null)
		{
			this.Activator = new DefaultParentActive(newOption);
		}
		return newOption;
	}

	public IOption Create0To100Percentage10StepOption<T>(
		T option,
		IOptionActivator? activator = null,
		bool isHidden = false,
		bool ignorePrefix = false,
		int defaultGage = 0) where T : struct, IConvertible
		=> CreateIntOption(
			option,
			defaultGage, 0, SingleRoleSpawnData.MaxSpawnRate, 10,
			activator,
			isHidden,
			OptionUnit.Percentage,
			ignorePrefix);

	public void Dispose()
	{
		this.internalFactory.Dispose();
	}
}
