using System.Text;
using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public interface IShipGlobalOption
    {
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
        public void BuildHudString(ref StringBuilder builder);

        public string ToHudString()
        {
            StringBuilder strBuilder = new StringBuilder();
            BuildHudString(ref strBuilder);
            return strBuilder.ToString().Trim('\r', '\n');
        }

        public static dynamic GetCommonOptionValue(OptionHolder.CommonOptionKey optionKey)
        {
            return OptionHolder.AllOption[(int)optionKey].GetValue();
        }
    }
}
