using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public class ClassicGameModeShipGlobalOption : IShipGlobalOption
    {
        public int MaxMeetingCount { get; protected set; }

        public bool IsChangeVoteAreaButtonSortArg { get; protected set; }
        public bool IsFixedVoteAreaPlayerLevel { get; protected set; }
        public bool IsBlockSkipInMeeting { get; protected set; }
        public bool DisableSelfVote { get; protected set; }

        public bool DisableVent { get; protected set; }
        public bool EngineerUseImpostorVent { get; protected set; }
        public bool CanKillVentInPlayer { get; protected set; }
        public bool IsAllowParallelMedbayScan { get; protected set; }
        public bool IsAutoSelectRandomSpawn { get; protected set; }

        public AdminOption Admin { get; protected set; }
        public SecurityOption Security { get; protected set; }
        public VitalOption Vital { get; protected set; }

        public bool DisableTaskWinWhenNoneTaskCrew { get; protected set; }
        public bool DisableTaskWin { get; protected set; }
        public bool IsSameNeutralSameWin { get; protected set; }
        public bool DisableNeutralSpecialForceEnd { get; protected set; }

        public bool IsAssignNeutralToVanillaCrewGhostRole { get; protected set; }
        public bool IsRemoveAngleIcon { get; protected set; }
        public bool IsBlockGAAbilityReport { get; protected set; }

        public void Load()
        {
            MaxMeetingCount = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.NumMeating);

            IsChangeVoteAreaButtonSortArg = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.ChangeMeetingVoteAreaSort);
            IsFixedVoteAreaPlayerLevel = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.FixedMeetingPlayerLevel);
            IsBlockSkipInMeeting = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableSkipInEmergencyMeeting);
            DisableSelfVote = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableSelfVote);

            DisableVent = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableVent);
            EngineerUseImpostorVent = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.EngineerUseImpostorVent);
            CanKillVentInPlayer = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.CanKillVentInPlayer);
            IsAllowParallelMedbayScan = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.ParallelMedBayScans);
            IsAutoSelectRandomSpawn = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsAutoSelectRandomSpawn);

            Admin = new AdminOption()
            {
                DisableAdmin = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveAdmin),
                AirShipEnable = (AirShipAdminMode)IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.AirShipEnableAdmin),
                EnableAdminLimit = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableAdminLimit),
                AdminLimitTime = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.AdminLimitTime),
            };
            Vital = new VitalOption()
            {
                DisableVital = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveVital),
                EnableVitalLimit = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableVitalLimit),
                VitalLimitTime = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.VitalLimitTime),
            };
            Security = new SecurityOption()
            {
                DisableSecurity = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveSecurity),
                EnableSecurityLimit = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableSecurityLimit),
                SecurityLimitTime = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.SecurityLimitTime),
            };

            DisableTaskWinWhenNoneTaskCrew = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableTaskWinWhenNoneTaskCrew);
            DisableTaskWin = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableTaskWin);
            IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsSameNeutralSameWin);
            DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableNeutralSpecialForceEnd);

            IsAssignNeutralToVanillaCrewGhostRole = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsAssignNeutralToVanillaCrewGhostRole);
            IsRemoveAngleIcon = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsRemoveAngleIcon);
            IsBlockGAAbilityReport = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsBlockGAAbilityReport);
        }
    }
}
