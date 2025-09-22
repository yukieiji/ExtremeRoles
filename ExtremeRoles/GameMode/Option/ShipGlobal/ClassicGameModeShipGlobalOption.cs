using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;


#nullable enable

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class ClassicGameModeShipGlobalOption : IShipGlobalOption
{

	public bool IsEnableImpostorVent => true;
    public bool CanUseHorseMode => true;
	public bool IsBreakEmergencyButton => false;

	public bool ChangeForceWallCheck { get; private set; }

	public bool IsRandomMap { get; private set; }

	public GameStartOption GameStart { get; private set; }
	public ExileOption Exile { get; private set; }

	public bool IsAllowParallelMedbayScan { get; private set; }

	public VentConsoleOption Vent { get; private set; }

	public SpawnOption Spawn { get; private set; }

    public AdminDeviceOption Admin { get; private set; }
    public DeviceOption Security { get; private set; }
    public VitalDeviceOption Vital { get; private set; }
	public GhostRoleOption GhostRole { get; private set; }
	public MeetingHudOption Meeting { get; private set; }

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

	public bool DisableTaskWinWhenNoneTaskCrew { get; private set; }
    public bool DisableTaskWin { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

	private readonly IReadOnlySet<int> cacheOptionId = OptionSplitter.AllEnable;

	public bool TryGetInvalidOption(int categoryId, [NotNullWhen(true)] out IReadOnlySet<int>? useOptionId)
	{
		useOptionId = cacheOptionId;
		return true;
	}

	public void Load()
    {
		this.GameStart = new GameStartOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.OnGameStartOption));

		this.Meeting = new MeetingHudOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.MeetingOption));

		this.Exile = new ExileOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.ExiledOption));

		this.Vent = new VentConsoleOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.VentOption));

		Spawn = new SpawnOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.RandomSpawnOption));

		Admin = new AdminDeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.AdminOption));
		Vital = new VitalDeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.VitalOption));
		Security = new DeviceOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.SecurityOption));

		var taskWinCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.TaskWinOption);
		DisableTaskWinWhenNoneTaskCrew = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableWhenNoneTaskCrew);
        DisableTaskWin = taskWinCate.GetValue<bool>((int)TaskWinOption.DisableAll);

		var neutralWinCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.NeutralWinOption);
		IsSameNeutralSameWin = neutralWinCate.GetValue<bool>((int)NeutralWinOption.IsSame);
        DisableNeutralSpecialForceEnd = neutralWinCate.GetValue<bool>((int)NeutralWinOption.DisableSpecialEnd);

		GhostRole = new GhostRoleOption(
			IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.GhostRoleGlobalOption));

		var taskCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.TaskOption);
		ChangeForceWallCheck = taskCate.GetValue<bool>((int)TaskOption.IsFixWallHaskTask);
		IsAllowParallelMedbayScan = taskCate.GetValue<bool>((int)TaskOption.ParallelMedBayScans);

		var randomMapCate = IShipGlobalOption.GetOptionLoader(ShipGlobalOptionCategory.RandomMapOption);
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
