using System.Text;
using System.Collections.Generic;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class HideNSeekModeShipGlobalOption : IShipGlobalOption
{
    public bool CanUseHorseMode => true;

    public bool IsEnableImpostorVent => false;

    public bool IsRandomMap { get; private set; }
	public bool ChangeForceWallCheck { get; private set; }

	public bool IsAllowParallelMedbayScan { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

    public AdminDeviceOption Admin { get; private set; }
    public DeviceOption Security { get; private set; }
    public DeviceOption Vital { get; private set; }
	public SpawnOption Spawn { get; private set; }

	public ConfirmExilMode ExilMode => ConfirmExilMode.Impostor;
    public bool IsConfirmRole => false;

    public bool DisableTaskWinWhenNoneTaskCrew => false;
    public bool DisableTaskWin => false;

	public bool IsBreakEmergencyButton => true;

	public VentConsoleOption Vent { get; private set; }

	public MeetingHudOption Meeting { get; } = new();
	public GhostRoleOption GhostRole { get; } = new();

	/*
	private HashSet<GlobalOption> useOption =
	[
        GlobalOption.DisableVent,
		GlobalOption.VentAnimationModeInVison,

		GlobalOption.IsFixWallHaskTask,
		GlobalOption.GarbageTask,
		GlobalOption.ShowerTask,
		GlobalOption.DevelopPhotosTask,
		GlobalOption.DivertPowerTask,

		GlobalOption.IsRemoveAdmin,
        GlobalOption.AirShipEnableAdmin,
        GlobalOption.EnableAdminLimit,
        GlobalOption.AdminLimitTime,

        GlobalOption.IsRemoveVital,
        GlobalOption.EnableVitalLimit,
        GlobalOption.VitalLimitTime,

        GlobalOption.IsRemoveSecurity,
        GlobalOption.EnableSecurityLimit,
        GlobalOption.SecurityLimitTime,

        GlobalOption.RandomMap,

        GlobalOption.IsSameNeutralSameWin,
        GlobalOption.DisableNeutralSpecialForceEnd,

        // GlobalOption.EnableHorseMode
    ];
	*/

    public void Load()
    {
		var vent = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.VentOption);
		this.Vent = new VentConsoleOption(
			vent.GetValue<bool>((int)VentOption.Disable),
			false, false,
			(VentAnimationMode)vent.GetValue<int>((int)VentOption.AnimationModeInVison));

		Spawn = new SpawnOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.RandomSpawnOption));

		Admin = new AdminDeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.AdminOption));
		Vital = new DeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.VitalOption));
		Security = new DeviceOption(
			IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.SecurityOption));

		var neutralWinCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.NeutralWinOption);
		IsSameNeutralSameWin = neutralWinCate.GetValue<bool>((int)NeutralWinOption.IsSame);
		DisableNeutralSpecialForceEnd = neutralWinCate.GetValue<bool>((int)NeutralWinOption.DisableSpecialEnd);

		var taskCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.TaskOption);
		ChangeForceWallCheck = taskCate.GetValue<bool>((int)TaskOption.IsFixWallHaskTask);
		IsAllowParallelMedbayScan = taskCate.GetValue<bool>((int)TaskOption.ParallelMedBayScans);

		var randomMapCate = IShipGlobalOption.GetOptionCategory(ShipGlobalOptionCategory.RandomMapOption);
		IsRandomMap = randomMapCate.GetValue<bool>((int)RandomMap.Enable);
	}
	/*
    public bool IsValidOption(int id) => this.useOption.Contains((GlobalOption)id);

	public IEnumerable<GlobalOption> UseOptionId()
	{
		foreach (GlobalOption id in this.useOption)
		{
			yield return id;
		}
	}
	*/
}
