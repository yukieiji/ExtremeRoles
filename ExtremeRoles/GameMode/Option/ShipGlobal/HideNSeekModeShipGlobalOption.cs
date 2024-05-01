using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class HideNSeekModeShipGlobalOption : IShipGlobalOption
{
    public bool CanUseHorseMode => true;

    public bool IsEnableImpostorVent => false;

    public bool IsRandomMap { get; private set; }
	public bool ChangeForceWallCheck { get; private set; }

	public bool DisableVent { get; private set; }
	public VentAnimationMode VentAnimationMode { get; private set; }

	public bool IsAllowParallelMedbayScan { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

    public AdminOption Admin { get; private set; }
    public SecurityOption Security { get; private set; }
    public VitalOption Vital { get; private set; }

	public SpawnOption Spawn { get; private set; }

	public int MaxMeetingCount => 0;

    public bool IsChangeVoteAreaButtonSortArg => false;
    public bool IsFixedVoteAreaPlayerLevel => false;
    public bool IsBlockSkipInMeeting  => false;
    public bool DisableSelfVote => false;

    public ConfirmExilMode ExilMode => ConfirmExilMode.Impostor;
    public bool IsConfirmRole => false;

    public bool EngineerUseImpostorVent => false;
    public bool CanKillVentInPlayer => false;

    public bool DisableTaskWinWhenNoneTaskCrew => false;
    public bool DisableTaskWin => false;

    public bool IsAssignNeutralToVanillaCrewGhostRole => false;
    public bool IsRemoveAngleIcon => false;
    public bool IsBlockGAAbilityReport => false;

	public bool IsBreakEmergencyButton => true;

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

    public void Load()
    {
        DisableVent = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableVent);
		this.VentAnimationMode = (VentAnimationMode)IShipGlobalOption.GetCommonOptionValue<int>(
			GlobalOption.VentAnimationModeInVison);

		ChangeForceWallCheck = IShipGlobalOption.GetCommonOptionValue<bool>(
			GlobalOption.IsFixWallHaskTask);

		Spawn = new SpawnOption(
			EnableSpecialSetting:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.EnableSpecialSetting),
			Skeld:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.SkeldRandomSpawn),
			MiraHq:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.MiraHqRandomSpawn),
			Polus:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.PolusRandomSpawn),
			AirShip:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.AirShipRandomSpawn),
			Fungle:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.FungleRandomSpawn),
			IsAutoSelectRandom:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.IsAutoSelectRandomSpawn));


		IsRandomMap = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.RandomMap);

        Admin = new AdminOption(
			Disable:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.IsRemoveAdmin),
			AirShipEnable:
				(AirShipAdminMode)IShipGlobalOption.GetCommonOptionValue<int>(
					GlobalOption.AirShipEnableAdmin),
			EnableAdminLimit:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.EnableAdminLimit),
			AdminLimitTime:
				IShipGlobalOption.GetCommonOptionValue<float>(
					GlobalOption.AdminLimitTime));
		Vital = new VitalOption(
			Disable:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.IsRemoveVital),
			EnableVitalLimit:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.EnableVitalLimit),
			VitalLimitTime:
				IShipGlobalOption.GetCommonOptionValue<float>(
					GlobalOption.VitalLimitTime),
			PolusPos:
				(PolusVitalPos)IShipGlobalOption.GetCommonOptionValue<int>(
					GlobalOption.PolusVitalPos));
		Security = new SecurityOption(
			Disable:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.IsRemoveSecurity),
			EnableSecurityLimit:
				IShipGlobalOption.GetCommonOptionValue<bool>(
					GlobalOption.EnableSecurityLimit),
			SecurityLimitTime:
				IShipGlobalOption.GetCommonOptionValue<float>(
					GlobalOption.SecurityLimitTime));

        IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsSameNeutralSameWin);
        DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableNeutralSpecialForceEnd);
    }

    public bool IsValidOption(int id) => this.useOption.Contains((GlobalOption)id);

	public IEnumerable<GlobalOption> UseOptionId()
	{
		foreach (GlobalOption id in this.useOption)
		{
			yield return id;
		}
	}
}
