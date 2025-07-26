using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Combination
{
    public class AssassinStatusModel : IStatusModel
    {
        public bool CanKilled { get; }
        public bool CanKilledFromCrew { get; }
        public bool CanKilledFromNeutral { get; }

        public AssassinStatusModel(bool canKilled, bool canKilledFromCrew, bool canKilledFromNeutral)
        {
            CanKilled = canKilled;
            CanKilledFromCrew = canKilledFromCrew;
            CanKilledFromNeutral = canKilledFromNeutral;
        }
    }
}
