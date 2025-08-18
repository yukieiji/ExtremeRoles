using ExtremeRoles.Module.CustomOption.Factory.Old;

namespace ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

public enum OnGameStartOption : int
{
	IsKillCoolDownIsTen,
	RemoveSomeoneButton,
	ReduceNum,
	FirstButtonCoolDown
}

public readonly struct GameStartOption
{
	public readonly bool IsKillCoolDownIsTen;
	public readonly bool RemoveSomeoneButton;
	public readonly int ReduceNum;
	public readonly int FirstButtonCoolDown;

	public GameStartOption()
	{
		this.IsKillCoolDownIsTen = true;
		this.RemoveSomeoneButton = true;
		this.ReduceNum = 1;
		this.FirstButtonCoolDown = 15;
	}
	public GameStartOption(in OptionCategory category)
	{
		this.IsKillCoolDownIsTen = category.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.IsKillCoolDownIsTen);
		this.RemoveSomeoneButton = category.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.RemoveSomeoneButton);
		this.ReduceNum = category.GetValue<OnGameStartOption, int>(
			OnGameStartOption.ReduceNum);
		this.FirstButtonCoolDown = category.GetValue<OnGameStartOption, int>(
			OnGameStartOption.FirstButtonCoolDown);
	}
	public static void Create(in OptionCategoryFactory factory)
	{
		var killCoolOpt = factory.CreateBoolOption(OnGameStartOption.IsKillCoolDownIsTen, true);
		var buttonOpt = factory.CreateBoolOption(
			OnGameStartOption.RemoveSomeoneButton, true, killCoolOpt);
		factory.CreateIntOption(
			OnGameStartOption.ReduceNum, 1, 1, 5, 1, buttonOpt, invert: true);
		factory.CreateIntOption(
			OnGameStartOption.FirstButtonCoolDown, 15, 0, 60, 1, format: OptionUnit.Second);
	}
}
