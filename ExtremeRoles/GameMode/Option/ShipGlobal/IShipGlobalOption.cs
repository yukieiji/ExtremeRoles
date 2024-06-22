using System;
using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Extension.Strings;
using ExtremeRoles.Patches.Option;
using ExtremeRoles.GameMode.Option.MapModule;

using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public enum ShipGlobalOptionCategory : int
{
	MeetingOption = 10,
	ExiledOption,
	VentOption,
	TaskOption,
	RandomSpawnOption,
	AdminOption,
	SecurityOption,
	VitalOption,
	RandomMapOption,
	TaskWinOption,
	NeutralWinOption,
	GhostRoleGlobalOption
}

public enum MeetingOption : int
{
	UseRaiseHand,
	NumMeating,
	ChangeMeetingVoteAreaSort,
	FixedMeetingPlayerLevel,
	DisableSkipInEmergencyMeeting,
	DisableSelfVote,
}

public enum ExiledOption : int
{
	ConfirmExilMode,
	IsConfirmRole,
}

public enum VentOption : int
{
	DisableVent,
	EngineerUseImpostorVent,
	CanKillVentInPlayer,
	VentAnimationModeInVison,
}

public enum TaskOption : int
{
	ParallelMedBayScans,

	IsFixWallHaskTask,
	GarbageTask,
	ShowerTask,
	DevelopPhotosTask,
	DivertPowerTask,
}

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

public enum MapObjectOption
{
	IsRemove,
	EnableLimit,
	LimitTime,
}

public enum AdminSpecialOption : int
{
	AirShipEnableAdmin = 5
}

public enum RandomMap : int
{
	RandomMap,
}

public enum TaskWinOption : int
{
	DisableTaskWinWhenNoneTaskCrew,
	DisableTaskWin,
}

public enum NeutralWinOption : int
{
	IsSameNeutralSameWin,
	DisableNeutralSpecialForceEnd,
}

public enum GhostRoleGlobalOption : int
{
	IsAssignNeutralToVanillaCrewGhostRole,
	IsRemoveAngleIcon,
	IsBlockGAAbilityReport,
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

			if (!NewOptionManager.Instance.TryGetCategory(
					OptionTab.General,
					(int)ShipGlobalOptionCategory.TaskOption,
					out var cate))
			{
				return fixTask;
			}

			for (int i = (int)TaskOption.GarbageTask; i <= (int)TaskOption.DivertPowerTask; ++i)
			{
				if (cate.GetValue<bool>(i))
				{
					var fixTaskType = (TaskOption)i switch
					{
						TaskOption.GarbageTask => TaskTypes.EmptyGarbage,
						TaskOption.ShowerTask => TaskTypes.FixShower,
						TaskOption.DevelopPhotosTask => TaskTypes.DevelopPhotos,
						TaskOption.DivertPowerTask => TaskTypes.DivertPower,
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
	// public IEnumerable<GlobalOption> UseOptionId();
	/*
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
	*/

    public static void Create()
    {
		var mng = NewOptionManager.Instance;
		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.MeetingOption))
		{
			factory.CreateBoolOption(MeetingOption.UseRaiseHand, false);
			factory.CreateIntOption(MeetingOption.NumMeating, 10, 0, 100, 1);
			factory.CreateBoolOption(MeetingOption.ChangeMeetingVoteAreaSort, false);
			factory.CreateBoolOption(MeetingOption.FixedMeetingPlayerLevel, false);
			factory.CreateBoolOption(MeetingOption.DisableSkipInEmergencyMeeting, false);
			factory.CreateBoolOption(MeetingOption.DisableSelfVote, false);
		}
		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.ExiledOption))
		{
			var confirmOpt = factory.CreateSelectionOption<ExiledOption, ConfirmExilMode>(ExiledOption.ConfirmExilMode);
			// confirmOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
			var confirmRoleOpt = factory.CreateBoolOption(ExiledOption.IsConfirmRole, false);
			// confirmRoleOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
		}
		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.VentOption))
		{
			var ventOption = factory.CreateBoolOption(VentOption.DisableVent, false);
			factory.CreateBoolOption(VentOption.CanKillVentInPlayer, false, ventOption, invert: true);
			factory.CreateBoolOption(VentOption.EngineerUseImpostorVent, false, ventOption, invert: true);
			factory.CreateSelectionOption<VentOption, VentAnimationMode>(VentOption.VentAnimationModeInVison, parent: ventOption, invert: true);
		}
		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.TaskOption))
		{
			factory.CreateBoolOption(TaskOption.ParallelMedBayScans, false);

			var fixTaskOpt = factory.CreateBoolOption(TaskOption.IsFixWallHaskTask, false);
			for (int i = (int)TaskOption.GarbageTask; i <= (int)TaskOption.DivertPowerTask; ++i)
			{
				factory.CreateBoolOption((TaskOption)i, false, parent: fixTaskOpt);
			}
		}

		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.RandomSpawnOption))
		{
			var randomSpawnOpt = factory.CreateBoolOption(RandomSpawnOption.EnableSpecialSetting, true);
			factory.CreateBoolOption(RandomSpawnOption.SkeldRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(RandomSpawnOption.MiraHqRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(RandomSpawnOption.PolusRandomSpawn, false, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(RandomSpawnOption.AirShipRandomSpawn, true, randomSpawnOpt, invert: true);
			factory.CreateBoolOption(RandomSpawnOption.FungleRandomSpawn, false, randomSpawnOpt, invert: true);

			factory.CreateBoolOption(RandomSpawnOption.IsAutoSelectRandomSpawn, false, randomSpawnOpt, invert: true);
		}
		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.AdminOption))
		{
			var adminOpt = factory.CreateBoolOption(MapObjectOption.IsRemove, false);
			factory.CreateSelectionOption<AdminSpecialOption, AirShipAdminMode>(
				AdminSpecialOption.AirShipEnableAdmin, adminOpt, invert: true);
			var adminLimitOpt = factory.CreateBoolOption(MapObjectOption.EnableLimit, false, adminOpt, invert: true);
			factory.CreateFloatOption(
				MapObjectOption.LimitTime,
				30.0f, 5.0f, 120.0f, 0.5f, adminLimitOpt,
				format: OptionUnit.Second,
				invert: true);
		}
		createMapObjectOptions(ShipGlobalOptionCategory.SecurityOption);
		createMapObjectOptions(ShipGlobalOptionCategory.VitalOption);

		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.RandomMapOption))
		{
			factory.CreateBoolOption(RandomMap.RandomMap, false);
		}

		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.TaskWinOption))
		{
			var taskDisableOpt = factory.CreateBoolOption(TaskWinOption.DisableTaskWinWhenNoneTaskCrew, false);
			factory.CreateBoolOption(TaskWinOption.DisableTaskWin, false, taskDisableOpt);
		}

		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.NeutralWinOption))
		{
			factory.CreateBoolOption(NeutralWinOption.IsSameNeutralSameWin, true);
			factory.CreateBoolOption(NeutralWinOption.DisableNeutralSpecialForceEnd, false);
		}

		using (var factory = mng.CreateOptionCategory(ShipGlobalOptionCategory.GhostRoleGlobalOption))
		{
			factory.CreateBoolOption(GhostRoleGlobalOption.IsAssignNeutralToVanillaCrewGhostRole, true);
			factory.CreateBoolOption(GhostRoleGlobalOption.IsRemoveAngleIcon, false);
			factory.CreateBoolOption(GhostRoleGlobalOption.IsBlockGAAbilityReport, false);
		}

		// ウマングアスを一時的もしくは恒久的に無効化
		// CreateBoolOption(GlobalOption.EnableHorseMode, false, isHeader: true, isHidden: true);
    }

	private static void createMapObjectOptions(ShipGlobalOptionCategory category)
	{
		using (var factory = NewOptionManager.Instance.CreateOptionCategory(category))
		{
			factory.CreateBoolOption(MapObjectOption.IsRemove, false);
			factory.CreateBoolOption(MapObjectOption.EnableLimit, false);
			factory.CreateFloatOption(
				MapObjectOption.LimitTime,
				30.0f, 5.0f, 120.0f, 0.5f,
				format: OptionUnit.Second);
		}
	}
}
