using ExtremeRoles.GameMode.Option.MapModuleOption;

namespace ExtremeRoles.GameMode
{
    public class ShipGlobalOption
    {
        public const int SameNeutralGameControlId = int.MaxValue;

        public int MaxMeetingCount                        { get; set; } = 100;

        public bool IsChangeVoteAreaButtonSortArg         { get; set; } = true;
        public bool IsFixedVoteAreaPlayerLevel            { get; set; } = false;
        public bool IsBlockSkipInMeeting                  { get; set; } = false;
        public bool DisableSelfVote                       { get; set; } = false;

        public bool DisableVent                           { get; set; } = false;
        public bool EngineerUseImpostorVent               { get; set; } = false;
        public bool CanKillVentInPlayer                   { get; set; } = false;
        public bool IsAllowParallelMedbayScan             { get; set; } = false;
        public bool IsAutoSelectRandomSpawn               { get; set; } = false;

        public AdminOption Admin                          { get; set; } = new ();
        public SecurityOption Security                    { get; set; } = new ();
        public VitalOption Vital                          { get; set; } = new ();

        public bool DisableTaskWinWhenNoneTaskCrew        { get; set; } = false;
        public bool DisableTaskWin                        { get; set; } = false;
        public bool IsSameNeutralSameWin                  { get; set; } = true;
        public bool DisableNeutralSpecialForceEnd         { get; set; } = false;

        public bool IsAssignNeutralToVanillaCrewGhostRole { get; set; } = true;
        public bool IsRemoveAngleIcon                     { get; set; } = false;
        public bool IsBlockGAAbilityReport                { get; set; } = false;

        public static dynamic GetCommonOptionValue(OptionHolder.CommonOptionKey optionKey)
        {
            return OptionHolder.AllOption[(int)optionKey].GetValue();
        }
    }
}
