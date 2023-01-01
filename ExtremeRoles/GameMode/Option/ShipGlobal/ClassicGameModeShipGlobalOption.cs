using ExtremeRoles.GameMode.Option.MapModule;

namespace ExtremeRoles.GameMode.Option.ShipGlobal
{
    public class ClassicGameModeShipGlobalOption : IShipGlobalOption
    {
        public int MaxMeetingCount
        {
            get
            {
                if (!this.maxMeetingCount.HasValue)
                {
                    this.maxMeetingCount = IShipGlobalOption.GetCommonOptionValue(
                    OptionHolder.CommonOptionKey.NumMeating);
                }
                return this.maxMeetingCount.Value;
            }
        }
        private int? maxMeetingCount; 

        public bool IsChangeVoteAreaButtonSortArg => throw new System.NotImplementedException();

        public bool IsFixedVoteAreaPlayerLevel => throw new System.NotImplementedException();

        public bool IsBlockSkipInMeeting => throw new System.NotImplementedException();

        public bool DisableSelfVote => throw new System.NotImplementedException();

        public bool DisableVent => throw new System.NotImplementedException();

        public bool EngineerUseImpostorVent => throw new System.NotImplementedException();

        public bool CanKillVentInPlayer => throw new System.NotImplementedException();

        public bool IsAllowParallelMedbayScan => throw new System.NotImplementedException();

        public bool IsAutoSelectRandomSpawn => throw new System.NotImplementedException();

        public AdminOption Admin => throw new System.NotImplementedException();

        public SecurityOption Security => throw new System.NotImplementedException();

        public VitalOption Vital => throw new System.NotImplementedException();

        public bool DisableTaskWinWhenNoneTaskCrew => throw new System.NotImplementedException();

        public bool DisableTaskWin => throw new System.NotImplementedException();

        public bool IsSameNeutralSameWin => throw new System.NotImplementedException();

        public bool DisableNeutralSpecialForceEnd => throw new System.NotImplementedException();

        public bool IsAssignNeutralToVanillaCrewGhostRole => throw new System.NotImplementedException();

        public bool IsRemoveAngleIcon => throw new System.NotImplementedException();

        public bool IsBlockGAAbilityReport => throw new System.NotImplementedException();

        private static T getOrInsert<T>(ref T? value, OptionHolder.CommonOptionKey optKey)
        {
            if (!value.HasValue)
            {
                value = IShipGlobalOption.GetCommonOptionValue(optKey);
            }
            return value.Value;
        }
    }
}
