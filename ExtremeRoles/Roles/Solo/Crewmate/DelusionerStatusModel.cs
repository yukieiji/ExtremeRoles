using ExtremeRoles.Module.Ability;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Delusioner
{
    public class DelusionerStatusModel : IStatusModel
    {
        public DelusionerCounterSystem? system { get; set; }
        public float range { get; }
        public bool includeSpawnPoint { get; }
        public ExtremeAbilityButton? Button { get; set; }
        public float deflectDamagePenaltyMod { get; }
        public float curCoolTime { get; set; }

        public DelusionerStatusModel(float range, bool includeSpawnPoint, float deflectDamagePenaltyMod)
        {
            this.range = range;
            this.includeSpawnPoint = includeSpawnPoint;
            this.deflectDamagePenaltyMod = deflectDamagePenaltyMod;
        }
    }
}
