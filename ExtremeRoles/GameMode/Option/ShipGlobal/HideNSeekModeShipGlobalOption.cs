using System.Text;
using System.Collections.Generic;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal;

public sealed class HideNSeekModeShipGlobalOption : IShipGlobalOption
{
    public int HeadOptionId => (int)GlobalOption.DisableVent;

    public bool CanUseHorseMode => true;

    public bool IsEnableImpostorVent => false;

    public bool IsRandomMap { get; private set; }

    public bool DisableVent { get; private set; }
    public bool IsAllowParallelMedbayScan { get; private set; }
    public bool IsSameNeutralSameWin { get; private set; }
    public bool DisableNeutralSpecialForceEnd { get; private set; }

    public AdminOption Admin { get; private set; }
    public SecurityOption Security { get; private set; }
    public VitalOption Vital { get; private set; }

    public int MaxMeetingCount => 0;

    public bool IsChangeVoteAreaButtonSortArg => false;
    public bool IsFixedVoteAreaPlayerLevel => false;
    public bool IsBlockSkipInMeeting  => false;
    public bool DisableSelfVote => false;

    public ConfirmExilMode ExilMode => ConfirmExilMode.Impostor;
    public bool IsConfirmRole => false;

    public bool EngineerUseImpostorVent => false;
    public bool CanKillVentInPlayer => false;
    public bool IsAutoSelectRandomSpawn => false;

    public bool DisableTaskWinWhenNoneTaskCrew => false;
    public bool DisableTaskWin => false;

    public bool IsAssignNeutralToVanillaCrewGhostRole => false;
    public bool IsRemoveAngleIcon => false;
    public bool IsBlockGAAbilityReport => false;

    private HashSet<GlobalOption> useOption = new HashSet<GlobalOption>()
    {
        GlobalOption.DisableVent,

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

        GlobalOption.EnableHorseMode
    };

    public void Load()
    {
        DisableVent = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableVent);

        IsRandomMap = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.RandomMap);

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

        IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.IsSameNeutralSameWin);
        DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue<bool>(
            GlobalOption.DisableNeutralSpecialForceEnd);
    }

    public void BuildHudString(ref StringBuilder builder)
    {
        foreach (GlobalOption id in this.useOption)
        {
            string optionStr = OptionManager.Instance.GetHudString((int)id);
            if (optionStr != string.Empty)
            {
                builder.AppendLine(optionStr);
            }
        }
    }

    public bool IsValidOption(int id) => this.useOption.Contains((GlobalOption)id);
}
