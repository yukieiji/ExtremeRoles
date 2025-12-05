using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.CustomOption.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;



#nullable enable

namespace ExtremeRoles.GameMode.RoleSelector;

public enum LiberalGlobalSetting
{
	WinMoney,
	TaskCompletedMoney,
	KillMoney,

	LiberalVison,
	UseVent,

	CanKilledLeader,
	LeaderKilledBoost,
	IsAutoRevive,

	IsAutoExitWhenLeaderSolo,
	CanKilledWhenLeaderSolo,

	LeaderHasOtherVisonSize,
	LeaderVison,

	CanHasTaskLeader,
	LeaderTaskBoost,
	LeaderTaskCompletedMoney,

	CanKillLeader,
	LeaderKillBoost,
	LeaderKillMoney,

	LeaderHasOtherKillCool,
	LeaderKillCool,
	LeaderHasOtherKillRange,
	LeaderKillRange,

	LiberalMilitantMini,
	LiberalMilitantMax,

	MilitantHasOtherKillCool,
	MilitantKillCool,
	MilitantHasOtherKillRange,
	MilitantKillRange,
}

public class LiberalDefaultOptipnLoader : IOptionLoader
{
	public IReadOnlyList<IOption> GlobalOption { get; }
	public IReadOnlyList<IOption> LeaderOption { get; }
	public IReadOnlyList<IOption> MilitantOption { get; }

	private readonly OptionCategory category;

	public LiberalDefaultOptipnLoader()
	{
		if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)SpawnOptionCategory.LiberalSetting, out var category))
		{
			throw new ArgumentException("Cannot find liberal setting");
		}
		this.category = category;

		this.GlobalOption = [
			this.category.Get(LiberalGlobalSetting.WinMoney),
			this.category.Get(LiberalGlobalSetting.TaskCompletedMoney),
			this.category.Get(LiberalGlobalSetting.KillMoney),
			this.category.Get(LiberalGlobalSetting.LiberalVison),
			this.category.Get(LiberalGlobalSetting.CanKilledLeader),
		];

		this.LeaderOption = [
			this.category.Get(LiberalGlobalSetting.LeaderHasOtherVisonSize),
			this.category.Get(LiberalGlobalSetting.CanHasTaskLeader),
			this.category.Get(LiberalGlobalSetting.CanKillLeader),
		];


		this.MilitantOption = [
			this.category.Get(LiberalGlobalSetting.LiberalMilitantMini),
			this.category.Get(LiberalGlobalSetting.LiberalMilitantMax),
			this.category.Get(LiberalGlobalSetting.MilitantHasOtherKillCool),
			this.category.Get(LiberalGlobalSetting.MilitantKillRange),
		];
	}

	public IOption Get(int id)
		=> this.category.Get(id);

	public IOption Get<T>(T id) where T : Enum
		=> this.category.Get(id);

	public T GetValue<W, T>(W id)
		where W : Enum
		where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
		=> this.category.GetValue<W, T>(id);

	public T GetValue<T>(int id) where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
		=> this.category.GetValue<T>(id);

	public bool TryGet(int id, [NotNullWhen(true)] out IOption? option)
		=> this.category.TryGet(id, out option);

	public bool TryGet<T>(T id, [NotNullWhen(true)] out IOption? option) where T : Enum
		=> this.category.TryGet(id, out option);

	public bool TryGetValue<T>(int id, [NotNullWhen(true)] out T value) where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
		=> this.category.TryGetValue(id, out value);

	public bool TryGetValue<W, T>(W id, [NotNullWhen(true)] out T value)
		where W : Enum
		where T : struct, IComparable, IConvertible, IComparable<T>, IEquatable<T>
		=> this.category.TryGetValue(id, out value);
}

public sealed class LiberalSettingCheck(IOption maxLiberalOption, int num) : IOptionActivator
{
	public IOption? Parent => null;

	private readonly IOption liberalMaxOption = maxLiberalOption;
	private readonly int num = num;

	public bool IsActive => liberalMaxOption.Value<int>() >= num;
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

		factory.CreateFloatOption(LiberalGlobalSetting.LiberalVison,
			2f, 0.25f, 5.0f, 0.25f, format: OptionUnit.Multiplier);
		factory.CreateBoolOption(LiberalGlobalSetting.UseVent, true);

		var leaderKilledSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanKilledLeader, false);
		var killedActive = new ParentActive(leaderKilledSetting);
		factory.CreateFloatOption(LiberalGlobalSetting.LeaderKilledBoost, 10, 0, 1000, 5, killedActive, format: OptionUnit.Percentage);

		var isLiberalMoreTwo = new LiberalSettingCheck(liberalMaxNumSetting, 2);

		// リベラルが一人になったときに無敵が剥がれるように => 条件: リベラル2人 and 死なない設定
		var leaderCanNotKill = new InvertActive(leaderKilledSetting);
		var autoCanKilled = factory.CreateBoolOption(LiberalGlobalSetting.CanKilledWhenLeaderSolo,
			false, new MultiActive(isLiberalMoreTwo, leaderCanNotKill));

		// 死んだときに自動的に復活する => 条件: 無敵ではない or 無敵が剥がれたとき
		var autoRevive = factory.CreateBoolOption(LiberalGlobalSetting.IsAutoRevive, true, new OrActive(killedActive, new ParentActive(autoCanKilled)));

		// リベラルが一人になったときに強制的に退場 => リベラルの勝利を完全に消す　条件: リベラルが2人以上 and 無敵設定 and 無敵が剥がれない設定時)
		var isAutoDead = factory.CreateBoolOption(
			LiberalGlobalSetting.IsAutoExitWhenLeaderSolo,
			false, new MultiActive(isLiberalMoreTwo, leaderCanNotKill, new InvertActive(autoCanKilled)));

		var visionOption = factory.CreateBoolOption(LiberalGlobalSetting.LeaderHasOtherVisonSize, false);
		factory.CreateFloatOption(LiberalGlobalSetting.LeaderVison,
			2f, 0.25f, 5.0f, 0.25f, new ParentActive(visionOption), format: OptionUnit.Multiplier);

		var leaderTaskSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanHasTaskLeader, false);
		var leaderTaskActive = new ParentActive(leaderTaskSetting);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderTaskBoost, 0, 0, 1000, 5, leaderTaskActive, format: OptionUnit.Percentage);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderTaskCompletedMoney, 5, 1, 1000, 1, leaderTaskActive);


		var leaderKillSetting = factory.CreateBoolOption(LiberalGlobalSetting.CanKillLeader, false);
		var leaderKillActive = new ParentActive(leaderKillSetting);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderKillBoost, 0, 0, 1000, 5, leaderKillActive, format: OptionUnit.Percentage);
		factory.CreateIntOption(LiberalGlobalSetting.LeaderKillMoney, 10, 1, 1000, 1, leaderKillActive);

		var leaderKillCoolOption = factory.CreateBoolOption(
			LiberalGlobalSetting.LeaderHasOtherKillCool,
			false, leaderKillActive);
		factory.CreateFloatOption(
			LiberalGlobalSetting.LeaderKillCool,
			30f, 1.0f, 120f, 0.5f,
			new ParentActive(leaderKillCoolOption),
			format: OptionUnit.Second);

		var leaderKillRangeOption = factory.CreateBoolOption(
			LiberalGlobalSetting.LeaderHasOtherKillRange,
			false, leaderKillActive);
		factory.CreateSelectionOption(
			LiberalGlobalSetting.LeaderKillRange,
			OptionCreator.Range,
			new ParentActive(leaderKillRangeOption));


		var liberalMilitantMini = factory.CreateIntDynamicMaxOption(LiberalGlobalSetting.LiberalMilitantMini, 0, 0, 1, liberalMaxNumSetting, isLiberalMoreTwo);

		int curMini = liberalMilitantMini.Value<int>();
		var intRange = ValueHolderAssembler.CreateIntValue(curMini, curMini, liberalMaxNumSetting.Value<int>(), 1);
		var liberalMilitantMax = factory.CreateOption(LiberalGlobalSetting.LiberalMilitantMax, intRange, isLiberalMoreTwo);

		var valueChangedEvent = () => {
			int newMini = liberalMilitantMini.Value<int>();
			int newMaxNum = liberalMaxNumSetting.Value<int>();
			intRange.InnerRange = OptionRange<int>.Create(newMini, newMaxNum, 1);
			
			// Selectionを再設定
			liberalMilitantMax.Selection = intRange.Selection;
		};

		liberalMilitantMini.OnValueChanged += valueChangedEvent;
		liberalMaxNumSetting.OnValueChanged += valueChangedEvent;


		var MilitantKillCoolOption = factory.CreateBoolOption(
			LiberalGlobalSetting.MilitantHasOtherKillCool,
			false, isLiberalMoreTwo);
		factory.CreateFloatOption(
			LiberalGlobalSetting.MilitantKillCool,
			30f, 1.0f, 120f, 0.5f,
			new ParentActive(MilitantKillCoolOption),
			format: OptionUnit.Second);

		var MilitantKillRangeOption = factory.CreateBoolOption(
			LiberalGlobalSetting.MilitantHasOtherKillRange,
			false, isLiberalMoreTwo);
		factory.CreateSelectionOption(
			LiberalGlobalSetting.MilitantKillRange,
			OptionCreator.Range,
			new ParentActive(MilitantKillRangeOption));
	}
}
