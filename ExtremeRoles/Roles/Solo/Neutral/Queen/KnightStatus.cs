using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Queen;

public sealed class KnightStatus(
	bool hasTask,
	int qweenSeeTaskRate,
	bool isSubTeams) : IStatusModel, ISubTeam
{
	private readonly bool hasTask = hasTask;
	private readonly float qweenSeeTaskRate = qweenSeeTaskRate / 100.0f;

	public bool SeeQween { get; private set; } = false;

	public NeutralSeparateTeam Main => NeutralSeparateTeam.Queen;
	public NeutralSeparateTeam Sub => NeutralSeparateTeam.QueenSub;

	public bool IsSub { get; } = isSubTeams;

	public void Update(PlayerControl knightPlayer)
	{
		if (!this.hasTask || this.SeeQween)
		{
			return;
		}

		float taskGage = Helper.Player.GetPlayerTaskGage(knightPlayer);
		if (taskGage >= this.qweenSeeTaskRate)
		{
			this.SeeQween = true;
		}
	}
}
