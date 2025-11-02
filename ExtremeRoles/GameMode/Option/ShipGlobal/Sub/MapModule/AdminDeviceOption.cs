using ExtremeRoles.Module.CustomOption.Factory.Old;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum AirShipAdminMode
{
	ModeBoth,
	ModeCockpitOnly,
	ModeArchiveOnly
}

public enum AdminSpecialOption : int
{
	AirShipEnable = 5
}

public readonly struct AdminDeviceOption : IDeviceOption
{
	public readonly AirShipAdminMode AirShipEnable;

	public bool Disable { get; }
	public bool EnableLimit { get; }
	public float LimitTime { get; }

	public AdminDeviceOption(in OptionCategory cate)
	{
		AirShipEnable = (AirShipAdminMode)cate.GetValue<int>((int)AdminSpecialOption.AirShipEnable);

		var device = new DeviceOption(cate);
		Disable = device.Disable;
		EnableLimit = device.EnableLimit;
		LimitTime = device.LimitTime;
	}
	public static void Create(in OldOptionCategoryFactory factory)
	{
		var removeOpt = IDeviceOption.Create(factory);

		factory.CreateSelectionOption<AdminSpecialOption, AirShipAdminMode>(
			AdminSpecialOption.AirShipEnable, removeOpt, invert: true);
	}
}
