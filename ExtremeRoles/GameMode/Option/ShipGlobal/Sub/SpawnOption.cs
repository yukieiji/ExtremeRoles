
using ExtremeRoles.Module.NewOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum RandomSpawnOption : int
{
	EnableSpecialSetting,
	SkeldRandomSpawn,
	MiraHqRandomSpawn,
	PolusRandomSpawn,
	AirShipRandomSpawn,
	FungleRandomSpawn,

	IsAutoSelectRandomSpawn,
}

public readonly struct SpawnOption(in OptionCategory cate)
{
	public readonly bool EnableSpecialSetting = cate.GetValue<bool>((int)RandomSpawnOption.EnableSpecialSetting);

	public readonly bool Skeld = cate.GetValue<bool>((int)RandomSpawnOption.SkeldRandomSpawn);
	public readonly bool MiraHq = cate.GetValue<bool>((int)RandomSpawnOption.MiraHqRandomSpawn);
	public readonly bool Polus = cate.GetValue<bool>((int)RandomSpawnOption.PolusRandomSpawn);
	public readonly bool AirShip = cate.GetValue<bool>((int)RandomSpawnOption.AirShipRandomSpawn);
	public readonly bool Fungle = cate.GetValue<bool>((int)RandomSpawnOption.FungleRandomSpawn);

	public readonly bool IsAutoSelectRandom = cate.GetValue<bool>((int)RandomSpawnOption.IsAutoSelectRandomSpawn);


	public static void Create(in OptionCategoryFactory factory)
	{
		var randomSpawnOpt = factory.CreateBoolOption(RandomSpawnOption.EnableSpecialSetting, true);
		factory.CreateBoolOption(RandomSpawnOption.SkeldRandomSpawn, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.MiraHqRandomSpawn, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.PolusRandomSpawn, false, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.AirShipRandomSpawn, true, randomSpawnOpt, invert: true);
		factory.CreateBoolOption(RandomSpawnOption.FungleRandomSpawn, false, randomSpawnOpt, invert: true);

		factory.CreateBoolOption(RandomSpawnOption.IsAutoSelectRandomSpawn, false, randomSpawnOpt, invert: true);
	}
}
