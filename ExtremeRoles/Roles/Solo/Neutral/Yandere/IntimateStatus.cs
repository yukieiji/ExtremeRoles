using ExtremeRoles.Roles.API.Interface.Status;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

public sealed class IntimateStatus(
	bool hasTask,
	int intimateSeeTaskRate,
	bool isSubTeams) : IStatusModel, ISubTeam
{
	private readonly bool hasTask = hasTask;
	private readonly float intimateSeeTaskRate = intimateSeeTaskRate / 100.0f;

	public bool SeeYandere { get; private set; } = false;

	public NeutralSeparateTeam Main => NeutralSeparateTeam.Yandere;
	public NeutralSeparateTeam Sub => NeutralSeparateTeam.YandereSub;

	public bool IsSub { get; } = isSubTeams;

	public void Update(PlayerControl intimatePlayer)
	{
		if (!this.hasTask || this.SeeYandere)
		{
			return;
		}

		float taskGage = Helper.Player.GetPlayerTaskGage(intimatePlayer);
		if (taskGage >= this.intimateSeeTaskRate)
		{
			this.SeeYandere = true;
		}
	}
}
