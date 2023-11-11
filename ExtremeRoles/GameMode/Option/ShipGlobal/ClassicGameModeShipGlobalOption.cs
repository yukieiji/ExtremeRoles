using System;
using System.Text;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class ClassicGameModeShipGlobalOption : IShipGlobalOption
{
    public bool IsEnableImpostorVent => true;
    public bool CanUseHorseMode => true;
	public bool IsBreakEmergencyButton => false;

	public bool ChangeForceWallCheck { get; private set; }

	public bool IsRandomMap { get; private set; }

    public int MaxMeetingCount { get; private set; }

    public bool IsChangeVoteAreaButtonSortArg { get; private set; }
    public bool IsFixedVoteAreaPlayerLevel { get; private set; }
    public bool IsBlockSkipInMeeting { get; private set; }
    public bool DisableSelfVote { get; private set; }

    public ConfirmExilMode ExilMode { get; private set; }
    public bool IsConfirmRole { get; private set; }

    public bool DisableVent { get; private set; }
    public bool EngineerUseImpostorVent { get; private set; }
    public bool CanKillVentInPlayer { get; private set; }
	public VentAnimationMode VentAnimationMode { get; private set; }

	public bool IsAllowParallelMedbayScan { get; private set; }

	public SpawnOption Spawn { get; private set; }

    public AdminOption Admin { get; private set; }
    public SecurityOption Security { get; private set; }
    public VitalOption Vital { get; private set; }

    public bool DisableTaskWinWhenNoneTaskCrew { get; private set; }
    public bool DisableTaskWin { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

    public bool IsAssignNeutralToVanillaCrewGhostRole { get; private set; }
    public bool IsRemoveAngleIcon { get; private set; }
    public bool IsBlockGAAbilityReport { get; private set; }

	public void Load()
    {
        MaxMeetingCount = IShipGlobalOption.GetCommonOptionValue<int>(
            GlobalOption.NumMeating);

        IsRandomMap = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.RandomMap);

        IsChangeVoteAreaButtonSortArg = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.ChangeMeetingVoteAreaSort);
        IsFixedVoteAreaPlayerLevel = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.FixedMeetingPlayerLevel);
        IsBlockSkipInMeeting = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableSkipInEmergencyMeeting);
        DisableSelfVote = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableSelfVote);
        ExilMode = (ConfirmExilMode)IShipGlobalOption.GetCommonOptionValue<int>(
            GlobalOption.ConfirmExilMode);
        IsConfirmRole = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsConfirmRole);

        DisableVent = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableVent);
        EngineerUseImpostorVent = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.EngineerUseImpostorVent);
        CanKillVentInPlayer = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.CanKillVentInPlayer);
		this.VentAnimationMode = (VentAnimationMode)IShipGlobalOption.GetCommonOptionValue<int>(
			GlobalOption.VentAnimationModeInVison);

		IsAllowParallelMedbayScan = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.ParallelMedBayScans);

		Spawn = new SpawnOption()
		{
			AirShip = IShipGlobalOption.GetCommonOptionValue<bool>(
				GlobalOption.AirShipRandomSpawn),
			IsAutoSelectRandom = IShipGlobalOption.GetCommonOptionValue<bool>(
				GlobalOption.IsAutoSelectRandomSpawn),
		};

		ChangeForceWallCheck = IShipGlobalOption.GetCommonOptionValue<bool>(
			GlobalOption.IsFixWallHaskTask);

		Admin = new AdminOption()
        {
            DisableAdmin = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.IsRemoveAdmin),
            AirShipEnable = (AirShipAdminMode)IShipGlobalOption.GetCommonOptionValue<int>(
                GlobalOption.AirShipEnableAdmin),
            EnableAdminLimit = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.EnableAdminLimit),
            AdminLimitTime = IShipGlobalOption.GetCommonOptionValue<float>(
                GlobalOption.AdminLimitTime),
        };
        Vital = new VitalOption()
        {
            DisableVital = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.IsRemoveVital),
            EnableVitalLimit = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.EnableVitalLimit),
            VitalLimitTime = IShipGlobalOption.GetCommonOptionValue<float>(
                GlobalOption.VitalLimitTime),
        };
        Security = new SecurityOption()
        {
            DisableSecurity = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.IsRemoveSecurity),
            EnableSecurityLimit = IShipGlobalOption.GetCommonOptionValue<bool>(
                GlobalOption.EnableSecurityLimit),
            SecurityLimitTime = IShipGlobalOption.GetCommonOptionValue<float>(
                GlobalOption.SecurityLimitTime),
        };

        DisableTaskWinWhenNoneTaskCrew = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableTaskWinWhenNoneTaskCrew);
        DisableTaskWin = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableTaskWin);
        IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsSameNeutralSameWin);
        DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableNeutralSpecialForceEnd);

        IsAssignNeutralToVanillaCrewGhostRole = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsAssignNeutralToVanillaCrewGhostRole);
        IsRemoveAngleIcon = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsRemoveAngleIcon);
        IsBlockGAAbilityReport = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsBlockGAAbilityReport);
    }

    public void BuildHudString(ref StringBuilder builder)
    {
        foreach (GlobalOption id in Enum.GetValues(typeof(GlobalOption)))
        {
            var option = OptionManager.Instance.GetIOption((int)id);

			string optionStr = option.ToHudString();
            if (optionStr != string.Empty)
            {
                builder.AppendLine(optionStr);
            }
        }
    }

    public bool IsValidOption(int id) => Enum.IsDefined(typeof(GlobalOption), id);
}
