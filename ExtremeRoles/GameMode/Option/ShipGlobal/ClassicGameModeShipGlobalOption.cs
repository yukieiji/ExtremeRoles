using System;
using System.Collections.Generic;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.Module.CustomOption.OLDS;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class ClassicGameModeShipGlobalOption : IShipGlobalOption
{

	public bool IsEnableImpostorVent => true;
    public bool CanUseHorseMode => true;
	public bool IsBreakEmergencyButton => false;

	public bool ChangeForceWallCheck { get; private set; }

	public bool IsRandomMap { get; private set; }


    public ConfirmExilMode ExilMode { get; private set; }
    public bool IsConfirmRole { get; private set; }

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

	public void Load()
    {
		this.Meeting = new MeetingHudOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.MeetingOption));

		var exiledCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.ExiledOption);
		ExilMode = (ConfirmExilMode)exiledCate.GetValue<int>((int)ExiledOption.ConfirmExilMode);
        IsConfirmRole = exiledCate.GetValue<bool>((int)ExiledOption.IsConfirmRole);

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
		DisableTaskWinWhenNoneTaskCrew = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableTaskWinWhenNoneTaskCrew);
        DisableTaskWin = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableTaskWin);

		var neutralWinCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.NeutralWinOption);
		IsSameNeutralSameWin = neutralWinCate.GetValue<bool>((int)NeutralWinOption.IsSameNeutralSameWin);
        DisableNeutralSpecialForceEnd = neutralWinCate.GetValue<bool>((int)NeutralWinOption.DisableNeutralSpecialForceEnd);

		GhostRole = new GhostRoleOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.GhostRoleGlobalOption));

		var taskCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.TaskOption);
		ChangeForceWallCheck = taskCate.GetValue<bool>((int)TaskOption.IsFixWallHaskTask);
		IsAllowParallelMedbayScan = taskCate.GetValue<bool>((int)TaskOption.ParallelMedBayScans);

		var randomMapCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.RandomMapOption);
		IsRandomMap = randomMapCate.GetValue<bool>((int)RandomMap.RandomMap);
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
