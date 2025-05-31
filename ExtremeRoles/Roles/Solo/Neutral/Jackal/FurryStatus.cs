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

	public byte? TargetJackal { get; private set; }
	public bool SeeJackal { get; private set; } = false;

	public void Update(PlayerControl furryPlayer)
	{
		if (ShipStatus.Instance == null ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			TargetJackal is not null)
		{
			return;
		}

		if (this.hasTask && !this.SeeJackal)
		{
			float taskGage = Helper.Player.GetPlayerTaskGage(furryPlayer);
			if (taskGage >= this.jackalSeeTaskRate)
			{
				this.SeeJackal = true;
			}
		}

		foreach (var player in PlayerCache.AllPlayerControl)
		{
			byte playerId = player.PlayerId;
			if (player == null ||
				!ExtremeRoleManager.TryGetSafeCastedRole<JackalRole>(playerId, out var jackalRole) ||
				player.Data == null ||
				!(player.Data.IsDead || player.Data.Disconnected))
			{

				continue;
			}

			bool allSidekicksDead = true;
			foreach (byte sidekickPlayerId in jackalRole.SidekickPlayerId)
			{
				var sidekickPlayer = GameData.Instance.GetPlayerById(sidekickPlayerId);
				if (sidekickPlayer != null && !(player.Data.IsDead || player.Data.Disconnected))
				{
					allSidekicksDead = false;
					break;
				}
			}

			if (allSidekicksDead)
			{
				TargetJackal = playerId;
				break;
			}
		}
	}
}
