using ExtremeRoles.GameMode.MapModuleOption;

namespace ExtremeRoles.GameMode
{
    public class ShipGlobalOption
    {
        public const int SameNeutralGameControlId = int.MaxValue;

        public int MaxNumberOfMeeting                     { get; private set; } = 100;

        public bool ChangeMeetingVoteAreaSort             { get; private set; } = true;
        public bool FixedMeetingPlayerLevel               { get; private set; } = false;
        public bool AllowParallelMedBayScan               { get; private set; } = false;
        public bool BlockSkippingInEmergencyMeeting       { get; private set; } = false;

        public bool DisableVent                           { get; private set; } = false;
        public bool EngineerUseImpostorVent               { get; private set; } = false;
        public bool CanKillVentInPlayer                   { get; private set; } = false;
        public bool IsAutoSelectRandomSpawn               { get; private set; } = false;

        public AdminOption Admin                          { get; private set; } = new ();
        public SecurityOption Security                    { get; private set; } = new ();
        public VitalOption Vital                          { get; private set; } = new ();

        public bool DisableSelfVote                       { get; private set; } = false;

        public bool DisableTaskWinWhenNoneTaskCrew        { get; private set; } = false;
        public bool DisableTaskWin                        { get; private set; } = false;
        public bool IsSameNeutralSameWin                  { get; private set; } = true;
        public bool DisableNeutralSpecialForceEnd         { get; private set; } = false;

        public bool IsAssignNeutralToVanillaCrewGhostRole { get; private set; } = true;
        public bool IsRemoveAngleIcon                     { get; private set; } = false;
        public bool IsBlockGAAbilityReport                { get; private set; } = false;
    }
}
