using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate.Invisible;

public sealed class InvisibleStatusModel(float speedMod, bool hasHitbox) : IStatusModel
{
	public float SpeedMod { get; } = speedMod;
	public bool HasHitbox { get; } = hasHitbox;
	public bool IsActiveInvisible { get; set; } = false;
}
