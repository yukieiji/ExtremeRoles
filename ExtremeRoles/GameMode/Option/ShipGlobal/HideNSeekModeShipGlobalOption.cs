using System.Collections.Generic;

using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

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

	private IReadOnlyDictionary<int, HashSet<int>> useOption = new Dictionary<int, HashSet<int>>()
	{
		{ (int)ShipGlobalOptionCategory.VentOption       , [(int)VentOption.Disable, (int)VentOption.AnimationModeInVison] },
		{ (int)ShipGlobalOptionCategory.RandomSpawnOption, [] },
		{ (int)ShipGlobalOptionCategory.AdminOption      , [] },
		{ (int)ShipGlobalOptionCategory.VitalOption      , [] },
		{ (int)ShipGlobalOptionCategory.SecurityOption   , [] },
		{ (int)ShipGlobalOptionCategory.NeutralWinOption , [] },
		{ (int)ShipGlobalOptionCategory.TaskOption       , [] },
		{ (int)ShipGlobalOptionCategory.RandomMapOption  , [] },
	};

	public bool TryGetInvalidOption(int categoryId, out IReadOnlySet<int> useOptionId)
	{
		bool result = this.useOption.TryGetValue(categoryId, out var options);
		useOptionId = options;
		return result;
	}

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
}
