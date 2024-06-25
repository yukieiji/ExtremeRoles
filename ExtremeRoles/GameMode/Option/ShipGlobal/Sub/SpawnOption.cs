
using ExtremeRoles.Module.CustomOption.Factory;

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

public readonly struct SpawnOption(in OptionCategory cate)
{
	public readonly bool EnableSpecialSetting = cate.GetValue<bool>((int)RandomSpawnOption.Enable);

	public readonly bool Skeld = cate.GetValue<bool>((int)RandomSpawnOption.Skeld);
	public readonly bool MiraHq = cate.GetValue<bool>((int)RandomSpawnOption.MiraHq);
	public readonly bool Polus = cate.GetValue<bool>((int)RandomSpawnOption.Polus);
	public readonly bool AirShip = cate.GetValue<bool>((int)RandomSpawnOption.AirShip);
	public readonly bool Fungle = cate.GetValue<bool>((int)RandomSpawnOption.Fungle);

	public readonly bool IsAutoSelectRandom = cate.GetValue<bool>((int)RandomSpawnOption.IsAutoSelect);


	public static void Create(in OptionCategoryFactory factory)
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
