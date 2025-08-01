using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Fencer;

public class FencerStatusModel(float maxTime) : IStatusModel
{
    public bool IsCounter { get; set; } = false;
    public float Timer { get; set; } = 0.0f;
	public float MaxTime { get; } = maxTime;
    public bool CanKill { get; set; } = false;
}
