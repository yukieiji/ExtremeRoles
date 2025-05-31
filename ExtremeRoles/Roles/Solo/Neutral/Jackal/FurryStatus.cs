using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Neutral.Jackal;

#nullable enable

public sealed class FurryStatus : IStatusModel
{
	public byte? TargetJackal { get; private set; }

	public void Update()
	{
		if (ShipStatus.Instance == null ||
			!RoleAssignState.Instance.IsRoleSetUpEnd ||
			TargetJackal is not null)
		{
			return;
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
