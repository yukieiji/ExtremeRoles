
using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

public enum PolusVitalPos
{
	DefaultKey,
	SpecimenKey,
	LaboratoryKey
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

	public VitalDeviceOption(in OptionCategory cate)
	{
		PolusPos = (PolusVitalPos)cate.GetValue<int>((int)VitalSpecialOption.PolusVitalPos);

		var device = new DeviceOption(cate);
		Disable = device.Disable;
		EnableLimit = device.EnableLimit;
		LimitTime = device.LimitTime;
	}
	public static void Create(in OptionCategoryFactory factory)
	{
		var removeOpt = IDeviceOption.Create(factory);

		factory.CreateSelectionOption<VitalSpecialOption, PolusVitalPos>(
			VitalSpecialOption.PolusVitalPos, removeOpt, invert: true);
	}
}
