using ExtremeRoles.Roles.API.Interface;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class PawnStatus(
	bool hasTask,
	int qweenSeeTaskRate) : IStatusModel
{
	private readonly bool hasTask = hasTask;
	private readonly float qweenSeeTaskRate = qweenSeeTaskRate / 100.0f;

	public bool SeeQween { get; private set; } = false;

	public void Update(PlayerControl pawnPlayer)
	{
		if (!this.hasTask)
		{
			return;
		}

		float taskGage = Helper.Player.GetPlayerTaskGage(pawnPlayer);
		if (taskGage >= this.qweenSeeTaskRate && !this.SeeQween)
		{
			this.SeeQween = true;
		}
	}
}
