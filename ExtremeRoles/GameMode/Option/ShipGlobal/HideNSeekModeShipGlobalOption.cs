using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

#nullable enable

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
    public VitalDeviceOption Vital { get; private set; }
	public SpawnOption Spawn { get; private set; }

	public ConfirmExileMode ExilMode => ConfirmExileMode.Impostor;
    public bool IsConfirmRole => false;

    public bool DisableTaskWinWhenNoneTaskCrew => false;
    public bool DisableTaskWin => false;

	public bool IsBreakEmergencyButton => true;

	public VentConsoleOption Vent { get; private set; }

	public MeetingHudOption Meeting { get; } = new();
	public GhostRoleOption GhostRole { get; } = new();
	public ExileOption Exile { get; } = new();
	public GameStartOption GameStart { get; } = new();

	public EmergencyTaskOption Emergency
	{
		get
		{
			if (emergencyTaskOption is null)
			{
				emergencyTaskOption = new EmergencyTaskOption(
					IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.EmergencyTaskOption));
			}
			return emergencyTaskOption;
		}
	}
	private EmergencyTaskOption? emergencyTaskOption;

	private IReadOnlyDictionary<int, HashSet<int>> useOption = new Dictionary<int, HashSet<int>>()
	{
		{ (int)ShipGlobalOptionCategory.VentOption       , [(int)VentOption.Disable, (int)VentOption.AnimationModeInVison] },
		{ (int)ShipGlobalOptionCategory.RandomSpawnOption, OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.AdminOption      , OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.VitalOption      , OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.SecurityOption   , OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.NeutralWinOption , OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.TaskOption       , OptionSplitter.AllEnable },
		{ (int)ShipGlobalOptionCategory.RandomMapOption  , OptionSplitter.AllEnable },
	};

	public bool TryGetInvalidOption(int categoryId, [NotNullWhen(true)] out IReadOnlySet<int>? useOptionId)
	{
		bool result = this.useOption.TryGetValue(categoryId, out var options) && options is not null;
		useOptionId = options;
		return result;
	}

	public void Load()
    {
		var vent = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.VentOption);
		this.Vent = new VentConsoleOption(
			vent.GetValue<bool>((int)VentOption.Disable),
			false, false,
			(VentAnimationMode)vent.GetValue<int>((int)VentOption.AnimationModeInVison));

		Spawn = new SpawnOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.RandomSpawnOption));

		Admin = new AdminDeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.AdminOption));
		Vital = new VitalDeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.VitalOption));
		Security = new DeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.SecurityOption));

		var neutralWinCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.NeutralWinOption);
		IsSameNeutralSameWin = neutralWinCate.GetValue<bool>((int)NeutralWinOption.IsSame);
		DisableNeutralSpecialForceEnd = neutralWinCate.GetValue<bool>((int)NeutralWinOption.DisableSpecialEnd);

		var taskCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.TaskOption);
		ChangeForceWallCheck = taskCate.GetValue<bool>((int)TaskOption.IsFixWallHaskTask);
		IsAllowParallelMedbayScan = taskCate.GetValue<bool>((int)TaskOption.ParallelMedBayScans);

		var randomMapCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.RandomMapOption);
		IsRandomMap = randomMapCate.GetValue<bool>((int)RandomMap.Enable);
	}
}
