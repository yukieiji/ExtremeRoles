using ExtremeRoles.GameMode.Option.MapModuleOption;
using ExtremeRoles.GameMode.Vison;

namespace ExtremeRoles.GameMode.Factory
{
    public class ClassicGameModeOptionFactory : IModeFactory
    {
        public ShipGlobalOption CreateGlobalOption()
        {
            return new ShipGlobalOption()
            {
                MaxMeetingCount = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.NumMeating),

                IsChangeVoteAreaButtonSortArg = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.ChangeMeetingVoteAreaSort),
                IsFixedVoteAreaPlayerLevel = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.FixedMeetingPlayerLevel),
                IsBlockSkipInMeeting = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableSkipInEmergencyMeeting),
                DisableSelfVote = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableSelfVote),

                DisableVent = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableVent),
                EngineerUseImpostorVent = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EngineerUseImpostorVent),
                CanKillVentInPlayer = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.CanKillVentInPlayer),
                IsAllowParallelMedbayScan = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.ParallelMedBayScans),
                IsAutoSelectRandomSpawn = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsAutoSelectRandomSpawn),

                Admin = createAdminOpt(),
                Vital = crateVitalOpt(),
                Security = crateSecurityOpt(),

                DisableTaskWinWhenNoneTaskCrew = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableTaskWinWhenNoneTaskCrew),
                DisableTaskWin = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableTaskWin),
                IsSameNeutralSameWin = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsSameNeutralSameWin),
                DisableNeutralSpecialForceEnd = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.DisableNeutralSpecialForceEnd),

                IsAssignNeutralToVanillaCrewGhostRole = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsAssignNeutralToVanillaCrewGhostRole),
                IsRemoveAngleIcon = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveAngleIcon),
                IsBlockGAAbilityReport = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsBlockGAAbilityReport),
            };   
        }

        public IVisonModifier CreateVisonModifier() => new ClassicModeVison();

        private static AdminOption createAdminOpt()
        {
            return new AdminOption()
            {
                DisableAdmin = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveAdmin),
                AirShipEnable = (AirShipAdminMode)ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.AirShipEnableAdmin),
                EnableAdminLimit = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableAdminLimit),
                AdminLimitTime = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.AdminLimitTime),
            };
        }

        private static VitalOption crateVitalOpt()
        {
            return new VitalOption()
            {
                DisableVital = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveVital),
                EnableVitalLimit = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableVitalLimit),
                VitalLimitTime = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.VitalLimitTime),
            };
        }
        private static SecurityOption crateSecurityOpt()
        {
            return new SecurityOption()
            {
                DisableSecurity = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.IsRemoveSecurity),
                EnableSecurityLimit = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.EnableSecurityLimit),
                SecurityLimitTime = ShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.SecurityLimitTime),
            };
        }
    }
}
