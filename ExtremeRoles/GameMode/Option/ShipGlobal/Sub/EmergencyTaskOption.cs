using Il2CppSystem;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum EmergencyTaskTimeOption : int
{
	SkeldReactor,
	SkeldOxygen,
	MiraHqReactor,
	MiraHqOxygen,
	PolusReactor,
	AirshipHeli,
	FungleReactor
}

public sealed class SystemGetter(ShipStatus instance)
{
	private readonly ShipStatus instance = instance;


	public ShipStatus.MapType Type => instance.Type;

	public bool TryGet<T>(SystemTypes system, out T value) where T : Object
	{
		value = null;
		return
			instance.Systems.TryGetValue(system, out var interVal) &&
			interVal.IsTryCast<T>(out value);
	}
}

public sealed class EmergencyTaskOption(in IOptionLoader loader)
{
	private readonly int skeldReactorTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.SkeldReactor);
	private readonly int skeldOxygenTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.SkeldOxygen);

	private readonly int miraReactorTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.MiraHqReactor);
	private readonly int miraOxygenTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.MiraHqOxygen);

	private readonly int polusReactorTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.PolusReactor);

	public readonly int AirshipHeliTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.AirshipHeli);

	private readonly int fungleReactorTime = loader.GetValue<EmergencyTaskTimeOption, int>(EmergencyTaskTimeOption.FungleReactor);


	public void ChangeTime(ShipStatus instance)
	{
		var getter = new SystemGetter(instance);
		switch (getter.Type)
		{
			case ShipStatus.MapType.Ship:
				changeSkeld(getter);
				break;
			case ShipStatus.MapType.Hq:
				changeMira(getter);
				break;
			case ShipStatus.MapType.Pb:
				changePolus(getter);
				break;
			case ShipStatus.MapType.Fungle:
				changeFungle(getter);
				break;
			default:
				break;
		}
	}

	private void changeSkeld(SystemGetter getter)
	{
		if (getter.TryGet<ReactorSystemType>(SystemTypes.Reactor, out var reactor))
		{
			reactor.ReactorDuration = skeldReactorTime;
		}
		if (getter.TryGet<LifeSuppSystemType>(SystemTypes.LifeSupp, out var oxygen))
		{
			oxygen.LifeSuppDuration = skeldOxygenTime;
		}
	}

	private void changeMira(SystemGetter getter)
	{
		if (getter.TryGet<ReactorSystemType>(SystemTypes.Reactor, out var reactor))
		{
			reactor.ReactorDuration = miraReactorTime;
		}
		if (getter.TryGet<LifeSuppSystemType>(SystemTypes.LifeSupp, out var oxygen))
		{
			oxygen.LifeSuppDuration = miraOxygenTime;
		}
	}

	private void changePolus(SystemGetter getter)
	{
		if (getter.TryGet<ReactorSystemType>(SystemTypes.Reactor, out var reactor))
		{
			reactor.ReactorDuration = polusReactorTime;
		}
	}

	private void changeFungle(SystemGetter getter)
	{
		if (getter.TryGet<ReactorSystemType>(SystemTypes.Reactor, out var reactor))
		{
			reactor.ReactorDuration = fungleReactorTime;
		}
	}

	public static void Create(in DefaultBuilder factory)
	{
		factory.CreateIntOption(
			EmergencyTaskTimeOption.SkeldReactor,
			30, 5, 120, 1, format: OptionUnit.Second);
		factory.CreateIntOption(
			EmergencyTaskTimeOption.SkeldOxygen,
			30, 5, 120, 1, format: OptionUnit.Second);

		factory.CreateIntOption(
			EmergencyTaskTimeOption.MiraHqReactor,
			45, 5, 120, 1, format: OptionUnit.Second);
		factory.CreateIntOption(
			EmergencyTaskTimeOption.MiraHqOxygen,
			45, 5, 120, 1, format: OptionUnit.Second);

		factory.CreateIntOption(
			EmergencyTaskTimeOption.PolusReactor,
			60, 5, 120, 1, format: OptionUnit.Second);

		factory.CreateIntOption(
			EmergencyTaskTimeOption.AirshipHeli,
			90, 5, 120, 1, format: OptionUnit.Second);

		factory.CreateIntOption(
			EmergencyTaskTimeOption.FungleReactor,
			60, 5, 120, 1, format: OptionUnit.Second);
	}
}
