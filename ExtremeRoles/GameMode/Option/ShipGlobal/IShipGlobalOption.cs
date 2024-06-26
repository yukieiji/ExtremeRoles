using System;
using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Extension.Strings;
using ExtremeRoles.Patches.Option;



using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

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

public enum ExiledOption : int
{
	ConfirmExilMode,
	IsConfirmRole,
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

public enum RandomMap : int
{
	Enable,
}

public enum TaskWinOption : int
{
	DisableWhenNoneTaskCrew,
	DisableAll,
}

public enum NeutralWinOption : int
{
	IsSame,
	DisableSpecialEnd,
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

public readonly record struct MapModuleDisableFlag(
	bool Admin,
	bool Security,
	bool Vital,
	AirShipAdminMode AirShipAdminMode);

public interface IShipGlobalOption
{
	public bool IsEnableImpostorVent { get; }

	public bool IsRandomMap { get; }

	public bool IsBreakEmergencyButton { get; }

	public bool CanUseHorseMode { get; }

	public ConfirmExilMode ExilMode { get; }
	public bool IsConfirmRole { get; }

	public VentConsoleOption Vent { get; }
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

			var cate = GetOptionCategory(ShipGlobalOptionCategory.TaskOption);

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

	public MeetingHudOption Meeting { get; }
    public AdminDeviceOption Admin { get; }
    public DeviceOption Security { get; }
    public DeviceOption Vital { get; }

    public bool DisableTaskWinWhenNoneTaskCrew { get; }
    public bool DisableTaskWin { get; }
    public bool IsSameNeutralSameWin { get; }
    public bool DisableNeutralSpecialForceEnd { get; }

    public GhostRoleOption GhostRole { get; }

	public void Load();
	/*
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
		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.MeetingOption))
		{
			MeetingHudOption.Create(factory);
		}
		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.ExiledOption))
		{
			var confirmOpt = factory.CreateSelectionOption<ExiledOption, ConfirmExilMode>(ExiledOption.ConfirmExilMode);
			// confirmOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
			var confirmRoleOpt = factory.CreateBoolOption(ExiledOption.IsConfirmRole, false);
			// confirmRoleOpt.AddToggleOptionCheckHook(StringNames.GameConfirmImpostor);
		}
		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.VentOption))
		{
			VentConsoleOption.Create(factory);
		}
		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.TaskOption))
		{
			factory.CreateBoolOption(TaskOption.ParallelMedBayScans, false);

			var fixTaskOpt = factory.CreateBoolOption(TaskOption.IsFixWallHaskTask, false);
			for (int i = (int)TaskOption.GarbageTask; i <= (int)TaskOption.DivertPowerTask; ++i)
			{
				factory.CreateBoolOption((TaskOption)i, false, parent: fixTaskOpt);
			}
		}

		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.RandomSpawnOption))
		{
			SpawnOption.Create(factory);
		}
		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.AdminOption))
		{
			AdminDeviceOption.Create(factory);
		}
		createMapObjectOptions(ShipGlobalOptionCategory.SecurityOption);
		createMapObjectOptions(ShipGlobalOptionCategory.VitalOption);

		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.RandomMapOption))
		{
			factory.CreateBoolOption(RandomMap.Enable, false);
		}

		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.TaskWinOption))
		{
			var taskDisableOpt = factory.CreateBoolOption(TaskWinOption.DisableWhenNoneTaskCrew, false);
			factory.CreateBoolOption(TaskWinOption.DisableAll, false, taskDisableOpt);
		}

		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.NeutralWinOption))
		{
			factory.CreateBoolOption(NeutralWinOption.IsSame, true);
			factory.CreateBoolOption(NeutralWinOption.DisableSpecialEnd, false);
		}

		using (var factory = OptionManager.CreateOptionCategory(ShipGlobalOptionCategory.GhostRoleGlobalOption))
		{
			GhostRoleOption.Create(factory);
		}

		// ウマングアスを一時的もしくは恒久的に無効化
		// CreateBoolOption(GlobalOption.EnableHorseMode, false, isHeader: true, isHidden: true);
    }

	private static void createMapObjectOptions(ShipGlobalOptionCategory category)
	{
		using (var factory = OptionManager.CreateOptionCategory(category))
		{
			IDeviceOption.Create(factory);
		}
	}

	protected static OptionCategory GetOptionCategory(ShipGlobalOptionCategory category)
	{
		if (!OptionManager.Instance.TryGetCategory(OptionTab.General, (int)category, out var cate))
		{
			throw new ArgumentException(category.ToString());
		}
		return cate;
	}
}
