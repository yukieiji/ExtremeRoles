using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.OLDS;

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
		var invertActive = new InvertActive(randomSpawnOpt);
		factory.CreateBoolOption(RandomSpawnOption.Skeld, false, invertActive);
		factory.CreateBoolOption(RandomSpawnOption.MiraHq, false, invertActive);
		factory.CreateBoolOption(RandomSpawnOption.Polus, false, invertActive);
		factory.CreateBoolOption(RandomSpawnOption.AirShip, true, invertActive);
		factory.CreateBoolOption(RandomSpawnOption.Fungle, false, invertActive);

		factory.CreateBoolOption(RandomSpawnOption.IsAutoSelect, false, invertActive);
	}
}
