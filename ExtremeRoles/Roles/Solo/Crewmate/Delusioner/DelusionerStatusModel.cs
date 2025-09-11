using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Delusioner;

public class DelusionerStatusModel(float range, bool includeSpawnPoint, float deflectDamagePenaltyMod) : IStatusModel
{
	public float Range { get; } = range;
	public bool IncludeSpawnPoint { get; } = includeSpawnPoint;
    public float DeflectDamagePenaltyMod { get; } = deflectDamagePenaltyMod;
    public float CurCoolTime { get; set; }
}
