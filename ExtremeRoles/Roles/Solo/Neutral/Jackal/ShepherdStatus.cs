using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

#nullable enable

public sealed class ShepherdStatus(
	bool hasTask,
	int jackalSeeTaskRate,
	bool isSubTeam) : IStatusModel, ISubTeam
{
	private readonly bool hasTask = hasTask;
	private readonly float jackalSeeTaskRate = jackalSeeTaskRate / 100.0f;

	public byte? TargetJackal { get; private set; }
	public bool SeeJackal { get; private set; } = false;

	public NeutralSeparateTeam Main => NeutralSeparateTeam.Jackal;
	public NeutralSeparateTeam Sub => NeutralSeparateTeam.JackalSub;

	public bool IsSub { get; } = isSubTeam;

	public void Update(PlayerControl furryPlayer)
	{

		if (!this.hasTask || this.SeeJackal)
		{
			return;
		}

		float taskGage = Helper.Player.GetPlayerTaskGage(furryPlayer);
		if (taskGage >= this.jackalSeeTaskRate)
		{
			this.SeeJackal = true;
		}
	}
}
