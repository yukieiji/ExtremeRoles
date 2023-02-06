using System.Text;
using System.Collections.Generic;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public sealed class HideNSeekModeShipGlobalOption : IShipGlobalOption
    {
        public int HeadOptionId => (int)GlobalOption.DisableVent;

        public bool IsEnableSabtage => false;
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
        };

        public void Load()
        {
            DisableVent = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.DisableVent);

            IsRandomMap = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.RandomMap);

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

            IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.IsSameNeutralSameWin);
            DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue(
                GlobalOption.DisableNeutralSpecialForceEnd);
        }

        public void BuildHudString(ref StringBuilder builder)
        {
            foreach (GlobalOption id in this.useOption)
            {
                string optionStr = OptionHolder.AllOption[(int)id].ToHudString();
                if (optionStr != string.Empty)
                {
                    builder.AppendLine(optionStr);
                }
            }
        }

        public bool IsValidOption(int id) => this.useOption.Contains((GlobalOption)id);
    }
}
