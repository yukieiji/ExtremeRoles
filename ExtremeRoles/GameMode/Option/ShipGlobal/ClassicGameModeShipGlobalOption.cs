using System;
using System.Text;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class ClassicGameModeShipGlobalOption : IShipGlobalOption
{
    public int HeadOptionId => (int)GlobalOption.NumMeating; 

    public bool IsEnableImpostorVent => true;
    public bool CanUseHorseMode => true;

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
    public bool IsAllowParallelMedbayScan { get; private set; }
    public bool IsAutoSelectRandomSpawn { get; private set; }

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
        MaxMeetingCount = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.NumMeating);

        IsRandomMap = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.RandomMap);

        IsChangeVoteAreaButtonSortArg = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.ChangeMeetingVoteAreaSort);
        IsFixedVoteAreaPlayerLevel = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.FixedMeetingPlayerLevel);
        IsBlockSkipInMeeting = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableSkipInEmergencyMeeting);
        DisableSelfVote = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableSelfVote);
        ExilMode = (ConfirmExilMode)IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.ConfirmExilMode);
        IsConfirmRole = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsConfirmRole);

        DisableVent = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableVent);
        EngineerUseImpostorVent = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.EngineerUseImpostorVent);
        CanKillVentInPlayer = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.CanKillVentInPlayer);
        IsAllowParallelMedbayScan = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.ParallelMedBayScans);
        IsAutoSelectRandomSpawn = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsAutoSelectRandomSpawn);

        Admin = new AdminOption()
        {
            DisableAdmin = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.IsRemoveAdmin),
            AirShipEnable = (AirShipAdminMode)IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.AirShipEnableAdmin),
            EnableAdminLimit = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.EnableAdminLimit),
            AdminLimitTime = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.AdminLimitTime),
        };
        Vital = new VitalOption()
        {
            DisableVital = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.IsRemoveVital),
            EnableVitalLimit = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.EnableVitalLimit),
            VitalLimitTime = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.VitalLimitTime),
        };
        Security = new SecurityOption()
        {
            DisableSecurity = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.IsRemoveSecurity),
            EnableSecurityLimit = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.EnableSecurityLimit),
            SecurityLimitTime = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.SecurityLimitTime),
        };

        DisableTaskWinWhenNoneTaskCrew = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableTaskWinWhenNoneTaskCrew);
        DisableTaskWin = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableTaskWin);
        IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsSameNeutralSameWin);
        DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.DisableNeutralSpecialForceEnd);

        IsAssignNeutralToVanillaCrewGhostRole = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsAssignNeutralToVanillaCrewGhostRole);
        IsRemoveAngleIcon = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsRemoveAngleIcon);
        IsBlockGAAbilityReport = IShipGlobalOption.GetCommonOptionValue(
            GlobalOption.IsBlockGAAbilityReport);
    }

    public void BuildHudString(ref StringBuilder builder)
    {
        foreach (GlobalOption id in Enum.GetValues(typeof(GlobalOption)))
        {
            string optionStr = OptionHolder.AllOption[(int)id].ToHudString();
            if (optionStr != string.Empty)
            {
                builder.AppendLine(optionStr);
            }
        }
    }

    public bool IsValidOption(int id) => Enum.IsDefined(typeof(GlobalOption), id);
}
