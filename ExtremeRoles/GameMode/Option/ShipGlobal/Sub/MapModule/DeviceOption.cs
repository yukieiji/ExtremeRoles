using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
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

	public static IOption Create(in DefaultBuilder factory)
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

public readonly struct DeviceOption(in IOptionLoader loader) : IDeviceOption
{
	public bool Disable { get; } = loader.GetValue<bool>((int)DeviceOptionType.IsRemove);
	public bool EnableLimit { get; } = loader.GetValue<bool>((int)DeviceOptionType.EnableLimit);
	public float LimitTime { get; } = loader.GetValue<float>((int)DeviceOptionType.LimitTime);
}
