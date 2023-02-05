using System.Text;

using ExtremeRoles.Module;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public enum GlobalOption : int
    {
        NumMeating = 50,
        ChangeMeetingVoteAreaSort,
        FixedMeetingPlayerLevel,
        DisableSkipInEmergencyMeeting,
        DisableSelfVote,
        DisableVent,
        EngineerUseImpostorVent,
        CanKillVentInPlayer,
        ParallelMedBayScans,
        IsAutoSelectRandomSpawn,

        IsRemoveAdmin,
        AirShipEnableAdmin,
        EnableAdminLimit,
        AdminLimitTime,

        IsRemoveSecurity,
        EnableSecurityLimit,
        SecurityLimitTime,

        IsRemoveVital,
        EnableVitalLimit,
        VitalLimitTime,

        RandomMap,

        DisableTaskWinWhenNoneTaskCrew,
        DisableTaskWin,
        IsSameNeutralSameWin,
        DisableNeutralSpecialForceEnd,

        IsAssignNeutralToVanillaCrewGhostRole,
        IsRemoveAngleIcon,
        IsBlockGAAbilityReport,
    }

    public interface IShipGlobalOption
    {
        public int HeadOptionId { get; }

        public bool IsEnableSabtage { get; }
        public bool IsEnableImpostorVent { get; }

        public bool IsRandomMap { get; }
        
        public int MaxMeetingCount { get; }

        public bool IsChangeVoteAreaButtonSortArg { get; }
        public bool IsFixedVoteAreaPlayerLevel { get; }
        public bool IsBlockSkipInMeeting { get; }
        public bool DisableSelfVote { get; }

        public bool DisableVent { get; }
        public bool EngineerUseImpostorVent { get; }
        public bool CanKillVentInPlayer { get; }
        public bool IsAllowParallelMedbayScan { get; }
        public bool IsAutoSelectRandomSpawn { get; }

        public AdminOption Admin { get; }
        public SecurityOption Security { get; }
        public VitalOption Vital { get; }

        public bool DisableTaskWinWhenNoneTaskCrew { get; }
        public bool DisableTaskWin { get; }
        public bool IsSameNeutralSameWin { get; }
        public bool DisableNeutralSpecialForceEnd { get; }

        public bool IsAssignNeutralToVanillaCrewGhostRole { get; }
        public bool IsRemoveAngleIcon { get; }
        public bool IsBlockGAAbilityReport { get; }

        public void Load();

        public bool IsValidOption(int id);
        public void BuildHudString(ref StringBuilder builder);

        public string ToHudString()
        {
            StringBuilder strBuilder = new StringBuilder();
            BuildHudString(ref strBuilder);
            return strBuilder.ToString().Trim('\r', '\n');
        }

        public static void Create()
        {
            new IntCustomOption(
                (int)GlobalOption.NumMeating,
                GlobalOption.NumMeating.ToString(),
                10, 0, 100, 1, null);
            new BoolCustomOption(
              (int)GlobalOption.ChangeMeetingVoteAreaSort,
              GlobalOption.ChangeMeetingVoteAreaSort.ToString(),
              false);
            new BoolCustomOption(
               (int)GlobalOption.FixedMeetingPlayerLevel,
               GlobalOption.FixedMeetingPlayerLevel.ToString(),
               false);
            new BoolCustomOption(
                (int)GlobalOption.DisableSkipInEmergencyMeeting,
                GlobalOption.DisableSkipInEmergencyMeeting.ToString(),
                false);
            new BoolCustomOption(
                (int)GlobalOption.DisableSelfVote,
                GlobalOption.DisableSelfVote.ToString(),
                false);

            var ventOption = new BoolCustomOption(
                (int)GlobalOption.DisableVent,
                GlobalOption.DisableVent.ToString(),
                false);
            new BoolCustomOption(
                (int)GlobalOption.CanKillVentInPlayer,
                GlobalOption.CanKillVentInPlayer.ToString(),
                false, ventOption, invert: true);
            new BoolCustomOption(
                (int)GlobalOption.EngineerUseImpostorVent,
                GlobalOption.EngineerUseImpostorVent.ToString(),
                false, ventOption, invert: true);

            new BoolCustomOption(
                (int)GlobalOption.ParallelMedBayScans,
                GlobalOption.ParallelMedBayScans.ToString(), false);

            new BoolCustomOption(
                (int)GlobalOption.IsAutoSelectRandomSpawn,
                GlobalOption.IsAutoSelectRandomSpawn.ToString(), false);

            var adminOpt = new BoolCustomOption(
                (int)GlobalOption.IsRemoveAdmin,
                GlobalOption.IsRemoveAdmin.ToString(),
                false);
            new SelectionCustomOption(
                (int)GlobalOption.AirShipEnableAdmin,
                GlobalOption.AirShipEnableAdmin.ToString(),
                new string[]
                {
                    AirShipAdminMode.ModeBoth.ToString(),
                    AirShipAdminMode.ModeCockpitOnly.ToString(),
                    AirShipAdminMode.ModeArchiveOnly.ToString(),
                },
                adminOpt,
                invert: true);
            var adminLimitOpt = new BoolCustomOption(
                (int)GlobalOption.EnableAdminLimit,
                GlobalOption.EnableAdminLimit.ToString(),
                false, adminOpt,
                invert: true);
            new FloatCustomOption(
                (int)GlobalOption.AdminLimitTime,
                GlobalOption.AdminLimitTime.ToString(),
                30.0f, 5.0f, 120.0f, 0.5f, adminLimitOpt,
                format: OptionUnit.Second,
                invert: true,
                enableCheckOption: adminLimitOpt);

            var secOpt = new BoolCustomOption(
                (int)GlobalOption.IsRemoveSecurity,
                GlobalOption.IsRemoveSecurity.ToString(),
                false);
            var secLimitOpt = new BoolCustomOption(
                (int)GlobalOption.EnableSecurityLimit,
                GlobalOption.EnableSecurityLimit.ToString(),
                false, secOpt,
                invert: true);
            new FloatCustomOption(
                (int)GlobalOption.SecurityLimitTime,
                GlobalOption.SecurityLimitTime.ToString(),
                30.0f, 5.0f, 120.0f, 0.5f, secLimitOpt,
                format: OptionUnit.Second,
                invert: true,
                enableCheckOption: secLimitOpt);

            var vitalOpt = new BoolCustomOption(
                (int)GlobalOption.IsRemoveVital,
                GlobalOption.IsRemoveVital.ToString(),
                false);
            var vitalLimitOpt = new BoolCustomOption(
                (int)GlobalOption.EnableVitalLimit,
                GlobalOption.EnableVitalLimit.ToString(),
                false, vitalOpt,
                invert: true);
            new FloatCustomOption(
                (int)GlobalOption.VitalLimitTime,
                GlobalOption.VitalLimitTime.ToString(),
                30.0f, 5.0f, 120.0f, 0.5f, vitalLimitOpt,
                format: OptionUnit.Second,
                invert: true,
                enableCheckOption: vitalLimitOpt);

            new BoolCustomOption(
                (int)GlobalOption.RandomMap,
                GlobalOption.RandomMap.ToString(), false);

            var taskDisableOpt = new BoolCustomOption(
                (int)GlobalOption.DisableTaskWinWhenNoneTaskCrew,
                GlobalOption.DisableTaskWinWhenNoneTaskCrew.ToString(),
                false);
            new BoolCustomOption(
                (int)GlobalOption.DisableTaskWin,
                GlobalOption.DisableTaskWin.ToString(),
                false, taskDisableOpt);

            new BoolCustomOption(
                (int)GlobalOption.IsSameNeutralSameWin,
                GlobalOption.IsSameNeutralSameWin.ToString(),
                true);
            new BoolCustomOption(
                (int)GlobalOption.DisableNeutralSpecialForceEnd,
                GlobalOption.DisableNeutralSpecialForceEnd.ToString(),
                false);

            new BoolCustomOption(
                (int)GlobalOption.IsAssignNeutralToVanillaCrewGhostRole, GlobalOption.IsAssignNeutralToVanillaCrewGhostRole.ToString(),
                true);
            new BoolCustomOption(
                (int)GlobalOption.IsRemoveAngleIcon,
                GlobalOption.IsRemoveAngleIcon.ToString(),
                false);
            new BoolCustomOption(
                (int)GlobalOption.IsBlockGAAbilityReport,
                GlobalOption.IsBlockGAAbilityReport.ToString(),
                false);
        }

        public static dynamic GetCommonOptionValue(GlobalOption optionKey)
        {
            return OptionHolder.AllOption[(int)optionKey].GetValue();
        }
    }
}
