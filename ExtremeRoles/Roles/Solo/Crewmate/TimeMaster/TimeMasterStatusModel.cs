using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.TimeMaster;

public sealed class TimeMasterStatusModel(float rewindSecond) : IStatusModel
{
	public float RewindSecond { get; } = rewindSecond;
	public bool IsShieldOn { get; set; } = false;
}
