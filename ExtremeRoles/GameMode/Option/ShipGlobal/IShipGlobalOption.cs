using System;
using System.Collections.Generic;
using System.Text;

using ExtremeRoles.GameMode.Option.MapModule;

using static ExtremeRoles.Module.CustomOption.Factories.SimpleFactory;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public enum GlobalOption : int
{
    NumMeating = 50,
    ChangeMeetingVoteAreaSort,
    FixedMeetingPlayerLevel,
    DisableSkipInEmergencyMeeting,
    DisableSelfVote,

    ConfirmExilMode,
    IsConfirmRole,

    DisableVent,
    EngineerUseImpostorVent,
    CanKillVentInPlayer,
	VentAnimationModeInVison,

	ParallelMedBayScans,

	EnableRandomSpawn,
	SkeldRandomSpawn,
	MiraHqRandomSpawn,
	PolusRandomSpawn,
	AirShipRandomSpawn,
	FungleRandomSpawn,

	IsAutoSelectRandomSpawn,

	IsFixWallHaskTask,
	GarbageTask,
	ShowerTask,
	DevelopPhotosTask,
	DivertPowerTask,

	IsRemoveAdmin,
    AirShipEnableAdmin,
    EnableAdminLimit,
    AdminLimitTime,

    IsRemoveSecurity,
    EnableSecurityLimit,
    SecurityLimitTime,

    IsRemoveVital,
    EnableVitalLimit,
    VitalLimitTime,

    RandomMap,

    DisableTaskWinWhenNoneTaskCrew,
    DisableTaskWin,
    IsSameNeutralSameWin,
    DisableNeutralSpecialForceEnd,

    IsAssignNeutralToVanillaCrewGhostRole,
    IsRemoveAngleIcon,
    IsBlockGAAbilityReport,

    EnableHorseMode
}

public enum ConfirmExilMode
{
    Impostor,
    Crewmate,
    Neutral,
    AllTeam
}

public enum VentAnimationMode
{
	VanillaAnimation,
	DonotWallHack,
	DonotInVison,
}

public interface IShipGlobalOption
{
    public bool IsEnableImpostorVent { get; }

	public bool IsRandomMap { get; }

	public int MaxMeetingCount { get; }

	public bool IsBreakEmergencyButton { get; }

	public bool CanUseHorseMode { get; }

	public bool IsChangeVoteAreaButtonSortArg { get; }
	public bool IsFixedVoteAreaPlayerLevel { get; }
	public bool IsBlockSkipInMeeting { get; }
	public bool DisableSelfVote { get; }

	public ConfirmExilMode ExilMode { get; }
	public bool IsConfirmRole { get; }

	public bool DisableVent { get; }
	public bool EngineerUseImpostorVent { get; }
	public bool CanKillVentInPlayer { get; }
	public VentAnimationMode VentAnimationMode { get; }

	public SpawnOption Spawn { get; }
	public bool IsAllowParallelMedbayScan { get; }
	public bool ChangeForceWallCheck { get; }

	public IReadOnlySet<TaskTypes> WallCheckTask => new HashSet<TaskTypes>()
	{
		TaskTypes.EmptyGarbage,
		TaskTypes.FixShower,
		TaskTypes.DevelopPhotos,
		TaskTypes.DivertPower,
	};

	public IReadOnlySet<TaskTypes> ChangeTask
	{
		get
		{
			var fixTask = new HashSet<TaskTypes>();
			for (int i = (int)GlobalOption.GarbageTask; i <= (int)GlobalOption.DivertPowerTask; ++i)
			{
				var opt = (GlobalOption)i;

				if (GetCommonOptionValue<bool>(opt))
				{
					var fixTaskType = opt switch
					{
						GlobalOption.GarbageTask => TaskTypes.EmptyGarbage,
						GlobalOption.ShowerTask => TaskTypes.FixShower,
						GlobalOption.DevelopPhotosTask => TaskTypes.DevelopPhotos,
						GlobalOption.DivertPowerTask => TaskTypes.DivertPower,
						_ => throw new KeyNotFoundException()
					};
					fixTask.Add(fixTaskType);
				}
			}
			return fixTask;
		}
	}

    public AdminOption Admin { get; }
    public SecurityOption Security { get; }
    public VitalOption Vital { get; }

    public bool DisableTaskWinWhenNoneTaskCrew { get; }
    public bool DisableTaskWin { get; }
    public bool IsSameNeutralSameWin { get; }
    public bool DisableNeutralSpecialForceEnd { get; }

    public bool IsAssignNeutralToVanillaCrewGhostRole { get; }
    public bool IsRemoveAngleIcon { get; }
    public bool IsBlockGAAbilityReport { get; }

	public void Load();

    public bool IsValidOption(int id);
    public void BuildHudString(ref StringBuilder builder);

    public string ToHudString()
    {
        StringBuilder strBuilder = new StringBuilder();
        BuildHudString(ref strBuilder);
        return strBuilder.ToString().Trim('\r', '\n');
    }

    public static void Create()
    {
		CreateIntOption(GlobalOption.NumMeating, 10, 0, 100, 1, isHeader: true);
		CreateBoolOption(GlobalOption.ChangeMeetingVoteAreaSort, false);
		CreateBoolOption(GlobalOption.FixedMeetingPlayerLevel, false);
		CreateBoolOption(GlobalOption.DisableSkipInEmergencyMeeting, false);
		CreateBoolOption(GlobalOption.DisableSelfVote, false);

		var confirmOpt = CreateSelectionOption<GlobalOption, ConfirmExilMode>(
			GlobalOption.ConfirmExilMode, isHeader: true);
        confirmOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
		var confirmRoleOpt = CreateBoolOption(GlobalOption.IsConfirmRole, false);
        confirmRoleOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);

        var ventOption = CreateBoolOption(GlobalOption.DisableVent, false, isHeader: true);
		CreateBoolOption(GlobalOption.CanKillVentInPlayer, false, ventOption, invert: true);
		CreateBoolOption(GlobalOption.EngineerUseImpostorVent, false, ventOption, invert: true);
		CreateSelectionOption<GlobalOption, VentAnimationMode>(GlobalOption.VentAnimationModeInVison, parent: ventOption, invert: true);

		CreateBoolOption(GlobalOption.ParallelMedBayScans, false, isHeader: true);
		CreateBoolOption(GlobalOption.IsAutoSelectRandomSpawn, false);
		var fixTaskOpt = CreateBoolOption(GlobalOption.IsFixWallHaskTask, false);

		for (int i = (int)GlobalOption.GarbageTask; i < (int)GlobalOption.IsRemoveAdmin; ++i)
		{
			CreateBoolOption((GlobalOption)i, false, parent: fixTaskOpt);
		}


		var randomSpawnOpt = CreateBoolOption(GlobalOption.EnableRandomSpawn , true);
		CreateBoolOption(GlobalOption.SkeldRandomSpawn  , false, randomSpawnOpt, invert: true);
		CreateBoolOption(GlobalOption.MiraHqRandomSpawn , false, randomSpawnOpt, invert: true);
		CreateBoolOption(GlobalOption.PolusRandomSpawn  , false, randomSpawnOpt, invert: true);
		CreateBoolOption(GlobalOption.AirShipRandomSpawn, true , randomSpawnOpt, invert: true);
		CreateBoolOption(GlobalOption.FungleRandomSpawn , false, randomSpawnOpt, invert: true);

		CreateBoolOption(GlobalOption.IsAutoSelectRandomSpawn, false, randomSpawnOpt, invert: true);

		var adminOpt = CreateBoolOption(GlobalOption.IsRemoveAdmin, false, isHeader: true);
		CreateSelectionOption<GlobalOption, AirShipAdminMode>(
			GlobalOption.AirShipEnableAdmin, adminOpt, invert: true);
		var adminLimitOpt = CreateBoolOption(GlobalOption.EnableAdminLimit, false, adminOpt, invert: true);
		CreateFloatOption(
			GlobalOption.AdminLimitTime,
			30.0f, 5.0f, 120.0f, 0.5f, adminLimitOpt,
			format: OptionUnit.Second,
			invert: true,
			enableCheckOption: adminLimitOpt);

        var secOpt = CreateBoolOption(GlobalOption.IsRemoveSecurity, false, isHeader: true);
		var secLimitOpt = CreateBoolOption(GlobalOption.EnableSecurityLimit, false, secOpt, invert: true);
		CreateFloatOption(
			GlobalOption.SecurityLimitTime,
			30.0f, 5.0f, 120.0f, 0.5f, secLimitOpt,
			format: OptionUnit.Second,
			invert: true,
			enableCheckOption: secLimitOpt);

        var vitalOpt = CreateBoolOption(GlobalOption.IsRemoveVital, false, isHeader: true);
        var vitalLimitOpt = CreateBoolOption(GlobalOption.EnableVitalLimit, false, vitalOpt, invert: true);
		CreateFloatOption(
			GlobalOption.VitalLimitTime,
			30.0f, 5.0f, 120.0f, 0.5f, vitalLimitOpt,
			format: OptionUnit.Second,
			invert: true,
			enableCheckOption: vitalLimitOpt);

		CreateBoolOption(GlobalOption.RandomMap, false, isHeader: true);

        var taskDisableOpt = CreateBoolOption(GlobalOption.DisableTaskWinWhenNoneTaskCrew, false, isHeader: true);
		CreateBoolOption(GlobalOption.DisableTaskWin, false, taskDisableOpt);

		CreateBoolOption(GlobalOption.IsSameNeutralSameWin, true, isHeader: true);
		CreateBoolOption(GlobalOption.DisableNeutralSpecialForceEnd, false);

		CreateBoolOption(GlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true, isHeader: true);
		CreateBoolOption(GlobalOption.IsRemoveAngleIcon, false);
		CreateBoolOption(GlobalOption.IsBlockGAAbilityReport, false);

		CreateBoolOption(GlobalOption.EnableHorseMode, false, isHeader: true);
    }

    public static T GetCommonOptionValue<T>(GlobalOption optionKey)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        return OptionManager.Instance.GetValue<T>((int)optionKey);
    }
}
