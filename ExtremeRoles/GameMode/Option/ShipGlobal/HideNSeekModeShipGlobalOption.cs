using System.Text;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public sealed class HideNSeekModeShipGlobalOption : IShipGlobalOption
    {
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

        public void Load()
        {
            DisableVent = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableVent);

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

            IsSameNeutralSameWin = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.IsSameNeutralSameWin);
            DisableNeutralSpecialForceEnd = IShipGlobalOption.GetCommonOptionValue(
                OptionHolder.CommonOptionKey.DisableNeutralSpecialForceEnd);
        }

        public void BuildHudString(ref StringBuilder builder)
        {
            foreach (OptionHolder.CommonOptionKey id in new OptionHolder.CommonOptionKey[]
            {
                OptionHolder.CommonOptionKey.DisableVent,
                
                OptionHolder.CommonOptionKey.IsRemoveAdmin,
                OptionHolder.CommonOptionKey.AirShipEnableAdmin,
                OptionHolder.CommonOptionKey.EnableAdminLimit,
                OptionHolder.CommonOptionKey.AdminLimitTime,
                
                OptionHolder.CommonOptionKey.IsRemoveVital,
                OptionHolder.CommonOptionKey.EnableVitalLimit,
                OptionHolder.CommonOptionKey.VitalLimitTime,

                OptionHolder.CommonOptionKey.IsRemoveSecurity,
                OptionHolder.CommonOptionKey.EnableSecurityLimit,
                OptionHolder.CommonOptionKey.SecurityLimitTime,

                OptionHolder.CommonOptionKey.IsSameNeutralSameWin,
                OptionHolder.CommonOptionKey.DisableNeutralSpecialForceEnd,
            })
            {
                string optionStr = OptionHolder.AllOption[(int)id].ToHudString();
                if (optionStr != string.Empty)
                {
                    builder.AppendLine(optionStr);
                }
            }
        }
    }
}
