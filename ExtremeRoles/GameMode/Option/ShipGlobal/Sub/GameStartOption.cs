using ExtremeRoles.Module.CustomOption.Factory.OptionBuilder;
using ExtremeRoles.Module.CustomOption.Interfaces;

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
	public GameStartOption(in IOptionLoader loader)
	{
		this.IsKillCoolDownIsTen = loader.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.IsKillCoolDownIsTen);
		this.RemoveSomeoneButton = loader.GetValue<OnGameStartOption, bool>(
			OnGameStartOption.RemoveSomeoneButton);
		this.ReduceNum = loader.GetValue<OnGameStartOption, int>(
			OnGameStartOption.ReduceNum);
		this.FirstButtonCoolDown = loader.GetValue<OnGameStartOption, int>(
			OnGameStartOption.FirstButtonCoolDown);
	}
	public static void Create(in DefaultBuilder factory)
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
