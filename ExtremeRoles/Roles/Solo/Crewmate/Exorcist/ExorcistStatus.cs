using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Crewmate.Exorcist;

public sealed class ExorcistStatus : IStatusModel
{
	private readonly ExorcistRole exorcist;
	private readonly float range;
	private float awakeFakeImpTaskGage;

	public NetworkedPlayerInfo CurTarget => Player.GetDeadBodyInfo(this.range);

	public ExorcistStatus(
		ExorcistRole exorcist,
		float awakeFakeImpTaskGage,
		float range)
	{
		this.awakeFakeImpTaskGage = awakeFakeImpTaskGage;
		this.exorcist = exorcist;
		this.exorcist.FakeImposter = awakeFakeImpTaskGage <= 0.0f;
		this.range = range;
	}

	public void FrameUpdate(PlayerControl player)
	{
		if (this.awakeFakeImpTaskGage <= 0.0f)
		{
			return;
		}
		if (Player.GetPlayerTaskGage(player) >= this.awakeFakeImpTaskGage)
		{
			this.exorcist.FakeImposter = true;
			this.awakeFakeImpTaskGage = -1.0f;
		}
	}
}
