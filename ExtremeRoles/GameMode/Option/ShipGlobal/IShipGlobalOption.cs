using System;
using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Extension.Strings;
using ExtremeRoles.Patches.Option;
using ExtremeRoles.GameMode.Option.MapModule;

using ExtremeRoles.Module.NewOption;

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

	IsFixWallHaskTask,
	GarbageTask,
	ShowerTask,
	DevelopPhotosTask,
	DivertPowerTask,

	EnableSpecialSetting,
	SkeldRandomSpawn,
	MiraHqRandomSpawn,
	PolusRandomSpawn,
	AirShipRandomSpawn,
	FungleRandomSpawn,

	IsAutoSelectRandomSpawn,

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

	// ウマングアスを一時的もしくは恒久的に無効化
	// EnableHorseMode
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
	DonotOutVison,
}

public readonly record struct MapModuleDisableFlag(
	bool Admin,
	bool Security,
	bool Vital,
	AirShipAdminMode AirShipAdminMode);

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

	public MapModuleDisableFlag RemoveMapModule => new MapModuleDisableFlag(
		this.Admin.Disable,
		this.Security.Disable,
		this.Vital.Disable,
		this.Admin.AirShipEnable);

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
	public IEnumerable<GlobalOption> UseOptionId();

	public void AddHudString(in List<string> allStr)
	{
		int lineCounter = 0;
		StringBuilder builder = new StringBuilder();
		foreach (GlobalOption id in UseOptionId())
		{
			var option = OptionManager.Instance.GetIOption((int)id);

			string optionStr = option.ToHudString();
			if (string.IsNullOrEmpty(optionStr)) { continue; }

			int lineCount = optionStr.CountLine();
			if (lineCounter + lineCount > IGameOptionsExtensionsToHudStringPatch.MaxLines)
			{
				lineCounter = 0;

				allStr.Add(builder.ToString());

				builder.Clear();
			}
			lineCounter += lineCount;
			builder.AppendLine(optionStr);
		}
		allStr.Add(builder.ToString());
	}

    public static void Create()
    {
		var mng = NewOptionManager.Instance;
		using (var factory = NewOptionManager.Instance.CreateOptionGroup("MeetingOption"))
		{
			factory.CreateIntOption(GlobalOption.NumMeating, 10, 0, 100, 1);
			factory.CreateBoolOption(GlobalOption.ChangeMeetingVoteAreaSort, false);
			factory.CreateBoolOption(GlobalOption.FixedMeetingPlayerLevel, false);
			factory.CreateBoolOption(GlobalOption.DisableSkipInEmergencyMeeting, false);
			factory.CreateBoolOption(GlobalOption.DisableSelfVote, false);
		}
		using (var factory = mng.CreateOptionGroup("ExiledOption"))
		{
			var confirmOpt = factory.CreateSelectionOption<GlobalOption, ConfirmExilMode>(GlobalOption.ConfirmExilMode);
			confirmOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
			var confirmRoleOpt = factory.CreateBoolOption(GlobalOption.IsConfirmRole, false);
			confirmRoleOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
		}
		using (var factory = mng.CreateOptionGroup("VentOption"))
		{
			var ventOption = factory.CreateBoolOption(GlobalOption.DisableVent, false);
			factory.CreateBoolOption(GlobalOption.CanKillVentInPlayer, false, ventOption, invert: true);
			factory.CreateBoolOption(GlobalOption.EngineerUseImpostorVent, false, ventOption, invert: true);
			factory.CreateSelectionOption<GlobalOption, VentAnimationMode>(GlobalOption.VentAnimationModeInVison, parent: ventOption, invert: true);
		}
		using (var factory = mng.CreateOptionGroup("TaskOption"))
		{
			factory.CreateBoolOption(GlobalOption.ParallelMedBayScans, false);

			var fixTaskOpt = factory.CreateBoolOption(GlobalOption.IsFixWallHaskTask, false);
			for (int i = (int)GlobalOption.GarbageTask; i <= (int)GlobalOption.DivertPowerTask; ++i)
			{
				factory.CreateBoolOption((GlobalOption)i, false, parent: fixTaskOpt);
			}
		}

		using (var factory = mng.CreateOptionGroup("RandomSpawnOption"))
		{
			var randomSpawnOpt = factory.CreateBoolOption(GlobalOption.EnableSpecialSetting, true);
			factory.CreateBoolOption(GlobalOption.SkeldRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(GlobalOption.MiraHqRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(GlobalOption.PolusRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(GlobalOption.AirShipRandomSpawn, true, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(GlobalOption.FungleRandomSpawn, false, randomSpawnOpt, invert: true);

			factory.CreateBoolOption(GlobalOption.IsAutoSelectRandomSpawn, false, randomSpawnOpt, invert: true);
		}
		using (var factory = mng.CreateOptionGroup("AdminOption"))
		{
			var adminOpt = factory.CreateBoolOption(GlobalOption.IsRemoveAdmin, false);
			factory.CreateSelectionOption<GlobalOption, AirShipAdminMode>(
				GlobalOption.AirShipEnableAdmin, adminOpt, invert: true);
			var adminLimitOpt = factory.CreateBoolOption(GlobalOption.EnableAdminLimit, false, adminOpt, invert: true);
			factory.CreateFloatOption(
				GlobalOption.AdminLimitTime,
				30.0f, 5.0f, 120.0f, 0.5f, adminLimitOpt,
				format: OptionUnit.Second,
				invert: true,
				enableCheckOption: adminLimitOpt);
		}
		using (var factory = mng.CreateOptionGroup("SecurityOption"))
		{
			var secOpt = factory.CreateBoolOption(GlobalOption.IsRemoveSecurity, false);
			var secLimitOpt = factory.CreateBoolOption(GlobalOption.EnableSecurityLimit, false, secOpt, invert: true);
			factory.CreateFloatOption(
				GlobalOption.SecurityLimitTime,
				30.0f, 5.0f, 120.0f, 0.5f, secLimitOpt,
				format: OptionUnit.Second,
				invert: true,
				enableCheckOption: secLimitOpt);
		}
		using (var factory = mng.CreateOptionGroup("VitalOption"))
		{
			var vitalOpt = factory.CreateBoolOption(GlobalOption.IsRemoveVital, false);
			var vitalLimitOpt = factory.CreateBoolOption(GlobalOption.EnableVitalLimit, false, vitalOpt, invert: true);
			factory.CreateFloatOption(
				GlobalOption.VitalLimitTime,
				30.0f, 5.0f, 120.0f, 0.5f, vitalLimitOpt,
				format: OptionUnit.Second,
				invert: true,
				enableCheckOption: vitalLimitOpt);
		}

		using (var factory = mng.CreateOptionGroup("RandomMapOption"))
		{
			factory.CreateBoolOption(GlobalOption.RandomMap, false);
		}

		using (var factory = mng.CreateOptionGroup("TaskWinOption"))
		{
			var taskDisableOpt = factory.CreateBoolOption(GlobalOption.DisableTaskWinWhenNoneTaskCrew, false);
			factory.CreateBoolOption(GlobalOption.DisableTaskWin, false, taskDisableOpt);
		}

		using (var factory = mng.CreateOptionGroup("NeutralWinOption"))
		{
			factory.CreateBoolOption(GlobalOption.IsSameNeutralSameWin, true);
			factory.CreateBoolOption(GlobalOption.DisableNeutralSpecialForceEnd, false);
		}

		using (var factory = mng.CreateOptionGroup("GhostRoleGlobalOption"))
		{
			factory.CreateBoolOption(GlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true);
			factory.CreateBoolOption(GlobalOption.IsRemoveAngleIcon, false);
			factory.CreateBoolOption(GlobalOption.IsBlockGAAbilityReport, false);
		}

		// ウマングアスを一時的もしくは恒久的に無効化
		// CreateBoolOption(GlobalOption.EnableHorseMode, false, isHeader: true, isHidden: true);
    }

    public static T GetCommonOptionValue<T>(GlobalOption optionKey)
        where T :
            struct, IComparable, IConvertible,
            IComparable<T>, IEquatable<T>
    {
        return OptionManager.Instance.GetValue<T>((int)optionKey);
    }
}
