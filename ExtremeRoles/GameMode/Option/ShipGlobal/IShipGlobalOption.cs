using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public interface IShipGlobalOption
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

        public void Load();

        public static dynamic GetCommonOptionValue(OptionHolder.CommonOptionKey optionKey)
        {
            return OptionHolder.AllOption[(int)optionKey].GetValue();
        }
    }
}
