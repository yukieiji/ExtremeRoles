using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;



#nullable enable

namespace ExtremeRoles.GameMode.RoleSelector;

public enum LiberalGlobalSetting
{
	WinMoney,
	TaskCompletedMoney,
	KillMoney,

	CanKillLeader,
	LeaderKillBoost,
	LeaderTaskCompletedMoney,

	CanHasTaskLeader,
	LeaderTaskBoost,
	LeaderKillMoney,

	CanKilledLeader,
	LeaderKilledBoost,

	CanExiledLeader,
	LeaderExiledBoost,

	LiberalMilitantMini,
	LiberalMilitantMax,
}

public sealed class LiberalSettingCheck(IOption maxLiberalOption, int num) : IOptionActivator
{
	public IOption? Parent => null;

	private readonly IOption liberalMaxOption = maxLiberalOption;
	private readonly int num = num - 1;

	public bool IsActive => liberalMaxOption.Value<int>() > num;
}

public sealed class LiberalOption
{
	public static void Create(OptionCategoryFactory globalFactory, IOption liberalMaxNumSetting)
	{
		var factory = new AutoActivatorSetFactory(globalFactory);
		factory.Activator = new LiberalSettingCheck(liberalMaxNumSetting, 1);

		factory.CreateIntOption(LiberalGlobalSetting.WinMoney, 100, 1, 1000, 1);
		factory.CreateIntOption(LiberalGlobalSetting.TaskCompletedMoney, 5, 1, 1000, 1);
		factory.CreateIntOption(LiberalGlobalSetting.KillMoney, 10, 1, 1000, 1);

		var leaderTaskSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanHasTaskLeader, false);
		var leaderTaskActive = new ParentActive(leaderTaskSetting);
		factory.CreateFloatOption(LiberalGlobalSetting.LeaderTaskBoost, 1.0f, 1.0f, 10.0f, 0.25f, leaderTaskActive);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderTaskCompletedMoney, 5, 1, 1000, 1, leaderTaskActive);

		var leaderKillSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanKillLeader, false);
		var leaderKillActive = new ParentActive(leaderKillSetting);
		factory.CreateFloatOption(LiberalGlobalSetting.LeaderKillBoost, 1.0f, 1.0f, 10.0f, 0.25f, leaderKillActive);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderKillMoney, 10, 1, 1000, 1, leaderKillActive);

		var leaderKilledSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanKilledLeader, false);
		factory.CreateFloatOption(LiberalGlobalSetting.LeaderExiledBoost, 1.0f, 1.0f, 10.0f, 0.25f, new ParentActive(leaderKilledSetting));

		var isMilitantActive = new LiberalSettingCheck(liberalMaxNumSetting, 2);
		var liberalMilitantMini = factory.CreateIntDynamicMaxOption(LiberalGlobalSetting.LiberalMilitantMini, 0, 0, 1, liberalMaxNumSetting, isMilitantActive);

		int curMini = liberalMilitantMini.Value<int>();
		var intRange = ValueHolderAssembler.CreateIntValue(curMini, curMini, liberalMaxNumSetting.Value<int>(), 1);
		var liberalMilitantMax = factory.CreateOption(LiberalGlobalSetting.LiberalMilitantMax, intRange, isMilitantActive);

		var valueCangeEvent = () => {
			int newMini = liberalMilitantMini.Value<int>();
			int newMaxNum = liberalMilitantMax.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(newMini, newMaxNum, 1);
			
			// Selectionを再設定
			liberalMilitantMax.Selection = intRange.Selection;
		};

		liberalMilitantMini.OnValueChanged += valueCangeEvent;
		liberalMaxNumSetting.OnValueChanged += valueCangeEvent;
	}
}
