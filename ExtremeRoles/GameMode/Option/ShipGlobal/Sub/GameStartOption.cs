using ExtremeRoles.Module.CustomOption.Factory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum OnGameStartOption : int
{
	IsKillCoolDownIsTen,
	RemoveSomeoneButton,
	ReduceNum,
	IsEmergencyButtonCoolDownIsFifteen
}

public readonly struct GameStartOption
{
	public readonly bool IsKillCoolDownIsTen;
	public readonly bool RemoveSomeoneButton;
	public readonly int ReduceNum;
	public readonly bool IsEmergencyButtonCoolDownIsFifteen;

	public GameStartOption()
	{
		this.IsKillCoolDownIsTen = true;
		this.RemoveSomeoneButton = true;
		this.ReduceNum = 1;
		this.IsEmergencyButtonCoolDownIsFifteen = true;
	}
	public GameStartOption(in OptionCategory category)
	{
		this.IsKillCoolDownIsTen = category.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.IsKillCoolDownIsTen);
		this.RemoveSomeoneButton = category.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.RemoveSomeoneButton);
		this.ReduceNum = category.GetValue<OnGameStartOption, int>(
			OnGameStartOption.ReduceNum);
		this.IsEmergencyButtonCoolDownIsFifteen = category.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.IsEmergencyButtonCoolDownIsFifteen);
	}
	public static void Create(in OptionCategoryFactory factory)
	{
		var opt = factory.CreateBoolOption(OnGameStartOption.IsKillCoolDownIsTen, true);
		var buttonOpt = factory.CreateBoolOption(
			OnGameStartOption.RemoveSomeoneButton, true, opt);
		factory.CreateIntOption(
			OnGameStartOption.ReduceNum, 1, 1, 1, 5, buttonOpt, invert: true);
		factory.CreateBoolOption(
			OnGameStartOption.IsEmergencyButtonCoolDownIsFifteen, true);
	}
}
