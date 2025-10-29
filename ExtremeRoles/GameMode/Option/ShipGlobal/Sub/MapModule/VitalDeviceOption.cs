using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum PolusVitalPos
{
	DefaultKey,
	Specimens,
	Laboratory
}

public enum VitalSpecialOption : int
{
	PolusVitalPos = 5
}

public readonly struct VitalDeviceOption : IDeviceOption
{
	public readonly PolusVitalPos PolusPos;

	public bool Disable { get; }
	public bool EnableLimit { get; }
	public float LimitTime { get; }

	public VitalDeviceOption(in IOptionLoader loader)
	{
		PolusPos = (PolusVitalPos)loader.GetValue<int>((int)VitalSpecialOption.PolusVitalPos);

		var device = new DeviceOption(loader);
		Disable = device.Disable;
		EnableLimit = device.EnableLimit;
		LimitTime = device.LimitTime;
	}
	public static void Create(in DefaultBuilder factory)
	{
		var removeOpt = IDeviceOption.Create(factory);

		factory.CreateSelectionOption<VitalSpecialOption, PolusVitalPos>(
			VitalSpecialOption.PolusVitalPos, removeOpt, invert: true);
	}
}
