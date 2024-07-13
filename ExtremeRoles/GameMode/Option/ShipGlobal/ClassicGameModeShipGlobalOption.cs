using System.Collections.Generic;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class ClassicGameModeShipGlobalOption : IShipGlobalOption
{

	public bool IsEnableImpostorVent => true;
    public bool CanUseHorseMode => true;
	public bool IsBreakEmergencyButton => false;

	public bool ChangeForceWallCheck { get; private set; }

	public bool IsRandomMap { get; private set; }

    public ExileOption Exile { get; private set; }

	public bool IsAllowParallelMedbayScan { get; private set; }

	public VentConsoleOption Vent { get; private set; }

	public SpawnOption Spawn { get; private set; }

    public AdminDeviceOption Admin { get; private set; }
    public DeviceOption Security { get; private set; }
    public DeviceOption Vital { get; private set; }
	public GhostRoleOption GhostRole { get; private set; }
	public MeetingHudOption Meeting { get; private set; }

	public bool DisableTaskWinWhenNoneTaskCrew { get; private set; }
    public bool DisableTaskWin { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

	private readonly IReadOnlySet<int> cacheOptionId = OptionSplitter.AllEnable;

	public bool TryGetInvalidOption(int categoryId, out IReadOnlySet<int> useOptionId)
	{
		useOptionId = cacheOptionId;
		return true;
	}

	public void Load()
    {
		this.Meeting = new MeetingHudOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.MeetingOption));

		this.Exile = new ExileOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.ExiledOption));

		this.Vent = new VentConsoleOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.VentOption));

		Spawn = new SpawnOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.RandomSpawnOption));

		Admin = new AdminDeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.AdminOption));
		Vital = new DeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.VitalOption));
		Security = new DeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.SecurityOption));

		var taskWinCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.TaskWinOption);
		DisableTaskWinWhenNoneTaskCrew = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableWhenNoneTaskCrew);
        DisableTaskWin = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableAll);

		var neutralWinCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.NeutralWinOption);
		IsSameNeutralSameWin = neutralWinCate.GetValue<bool>((int)NeutralWinOption.IsSame);
        DisableNeutralSpecialForceEnd = neutralWinCate.GetValue<bool>((int)NeutralWinOption.DisableSpecialEnd);

		GhostRole = new GhostRoleOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.GhostRoleGlobalOption));

		var taskCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.TaskOption);
		ChangeForceWallCheck = taskCate.GetValue<bool>((int)TaskOption.IsFixWallHaskTask);
		IsAllowParallelMedbayScan = taskCate.GetValue<bool>((int)TaskOption.ParallelMedBayScans);

		var randomMapCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.RandomMapOption);
		IsRandomMap = randomMapCate.GetValue<bool>((int)RandomMap.Enable);
	}
	/*
	public IEnumerable<GlobalOption> UseOptionId()
	{
		foreach (GlobalOption id in Enum.GetValues(typeof(GlobalOption)))
		{
			yield return id;
		}
	}

	public bool IsValidOption(int id) => Enum.IsDefined(typeof(GlobalOption), id);
	*/
}
