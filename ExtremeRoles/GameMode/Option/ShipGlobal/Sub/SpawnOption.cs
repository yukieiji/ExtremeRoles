
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum RandomSpawnOption : int
{
	Enable,
	Skeld,
	MiraHq,
	Polus,
	AirShip,
	Fungle,

	IsAutoSelect,
}

public readonly struct SpawnOption(in IOptionLoader loader)
{
	public readonly bool EnableSpecialSetting = loader.GetValue<bool>((int)RandomSpawnOption.Enable);

	public readonly bool Skeld = loader.GetValue<bool>((int)RandomSpawnOption.Skeld);
	public readonly bool MiraHq = loader.GetValue<bool>((int)RandomSpawnOption.MiraHq);
	public readonly bool Polus = loader.GetValue<bool>((int)RandomSpawnOption.Polus);
	public readonly bool AirShip = loader.GetValue<bool>((int)RandomSpawnOption.AirShip);
	public readonly bool Fungle = loader.GetValue<bool>((int)RandomSpawnOption.Fungle);

	public readonly bool IsAutoSelectRandom = loader.GetValue<bool>((int)RandomSpawnOption.IsAutoSelect);


	public static void Create(in DefaultBuilder factory)
	{
		var randomSpawnOpt = factory.CreateBoolOption(RandomSpawnOption.Enable, true);
		factory.CreateBoolOption(RandomSpawnOption.Skeld, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.MiraHq, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.Polus, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.AirShip, true, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.Fungle, false, randomSpawnOpt, invert: true);

		factory.CreateBoolOption(RandomSpawnOption.IsAutoSelect, false, randomSpawnOpt, invert: true);
	}
}
