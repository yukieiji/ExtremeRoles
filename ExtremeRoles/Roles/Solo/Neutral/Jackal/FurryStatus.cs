using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Interface.Status;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

#nullable enable

public sealed class FurryStatus(
	bool hasTask,
	int jackalSeeTaskRate) : IStatusModel
{
	private readonly bool hasTask = hasTask;
	private readonly float jackalSeeTaskRate = jackalSeeTaskRate / 100.0f;
	public bool SeeJackal { get; private set; } = false;

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
