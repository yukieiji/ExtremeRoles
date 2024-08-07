﻿
using ExtremeRoles.Module.CustomOption.Factory;


using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum DeviceOptionType
{
	IsRemove,
	EnableLimit,
	LimitTime,
}

public interface IDeviceOption
{
	public bool Disable { get; }
	public bool EnableLimit { get; }
	public float LimitTime { get; }

	public static IOption Create(in OptionCategoryFactory factory)
	{
		var removeOpt = factory.CreateBoolOption(DeviceOptionType.IsRemove, false);
		var enableLimit = factory.CreateBoolOption(
			DeviceOptionType.EnableLimit, false,
			removeOpt, invert: true);
		factory.CreateFloatOption(
			DeviceOptionType.LimitTime,
			30.0f, 5.0f, 120.0f, 0.5f,
			enableLimit,
			format: OptionUnit.Second);

		return removeOpt;
	}
}

public readonly struct DeviceOption(in OptionCategory category) : IDeviceOption
{
	public bool Disable { get; } = category.GetValue<bool>((int)DeviceOptionType.IsRemove);
	public bool EnableLimit { get; } = category.GetValue<bool>((int)DeviceOptionType.EnableLimit);
	public float LimitTime { get; } = category.GetValue<float>((int)DeviceOptionType.LimitTime);
}
