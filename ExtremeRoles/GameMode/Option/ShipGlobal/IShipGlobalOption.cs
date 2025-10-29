using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Rewired.Controller;




#nullable enable

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public enum ShipGlobalOptionCategory : int
{
	OnGameStartOption = 10,
	MeetingOption,
	ExiledOption,
	VentOption,
	TaskOption,
	RandomSpawnOption,
	EmergencyTaskOption,
	AdminOption,
	SecurityOption,
	VitalOption,
	RandomMapOption,
	TaskWinOption,
	NeutralWinOption,
	GhostRoleGlobalOption
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
	HauntMinigameMaxSpeed,
	IsAssignNeutralToVanillaCrewGhostRole,
	IsRemoveAngleIcon,
	IsBlockGAAbilityReport,
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

	public GameStartOption GameStart { get; }
	public ExileOption Exile { get; }

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

	public IReadOnlySet<TaskTypes> ChangeTask
	{
		get
		{
			var fixTask = new HashSet<TaskTypes>();

			var cate = GetOptionLoader(ShipGlobalOptionCategory.TaskOption);

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
    public VitalDeviceOption Vital { get; }

	public EmergencyTaskOption Emergency { get; }

    public bool DisableTaskWinWhenNoneTaskCrew { get; }
    public bool DisableTaskWin { get; }
    public bool IsSameNeutralSameWin { get; }
    public bool DisableNeutralSpecialForceEnd { get; }

    public GhostRoleOption GhostRole { get; }

	public void Load();
	public bool TryGetInvalidOption(int categoryId, [NotNullWhen(true)] out IReadOnlySet<int>? useOptionId);

	public static void Create(OptionCategoryAssembler assembler)
    {
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.OnGameStartOption))
		{
			GameStartOption.Create(cate.Builder);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.MeetingOption))
		{
			MeetingHudOption.Create(cate.Builder);
		}
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.ExiledOption))
		{
			ExileOption.Create(cate.Builder);
		}
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.VentOption))
		{
			VentConsoleOption.Create(cate.Builder);
		}
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.TaskOption))
		{
			var builder = cate.Builder;
			builder.CreateBoolOption(TaskOption.ParallelMedBayScans, false);

			var fixTaskOpt = builder.CreateBoolOption(TaskOption.IsFixWallHaskTask, false);
			for (int i = (int)TaskOption.GarbageTask; i <= (int)TaskOption.DivertPowerTask; ++i)
			{
				builder.CreateBoolOption((TaskOption)i, false, parent: fixTaskOpt);
			}
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.RandomSpawnOption))
		{
			SpawnOption.Create(cate.Builder);
		}
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.EmergencyTaskOption))
		{
			EmergencyTaskOption.Create(cate.Builder);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.AdminOption))
		{
			AdminDeviceOption.Create(cate.Builder);
		}
		createMapObjectOptions(assembler, ShipGlobalOptionCategory.SecurityOption);
		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.VitalOption))
		{
			VitalDeviceOption.Create(cate.Builder);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.RandomMapOption))
		{
			cate.Builder.CreateBoolOption(RandomMap.Enable, false);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.TaskWinOption))
		{
			var builder = cate.Builder;
			var taskDisableOpt = builder.CreateBoolOption(TaskWinOption.DisableWhenNoneTaskCrew, false);
			builder.CreateBoolOption(TaskWinOption.DisableAll, false, taskDisableOpt);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.NeutralWinOption))
		{
			var builder = cate.Builder;
			builder.CreateBoolOption(NeutralWinOption.IsSame, true);
			builder.CreateBoolOption(NeutralWinOption.DisableSpecialEnd, false);
		}

		using (var cate = assembler.CreateOptionCategory(ShipGlobalOptionCategory.GhostRoleGlobalOption))
		{
			GhostRoleOption.Create(cate.Builder);
		}

		// ウマングアスを一時的もしくは恒久的に無効化
		// CreateBoolOption(GlobalOption.EnableHorseMode, false, isHeader: true, isHidden: true);
    }

	private static void createMapObjectOptions(OptionCategoryAssembler assembler, ShipGlobalOptionCategory category)
	{
		using (var cate = assembler.CreateOptionCategory(category))
		{
			IDeviceOption.Create(cate.Builder);
		}
	}

	protected static IOptionLoader GetOptionLoader(ShipGlobalOptionCategory category)
	{
		if (!OptionManager.Instance.TryGetCategory(OptionTab.GeneralTab, (int)category, out var cate))
		{
			throw new ArgumentException(category.ToString());
		}
		return cate.Loader;
	}
}
