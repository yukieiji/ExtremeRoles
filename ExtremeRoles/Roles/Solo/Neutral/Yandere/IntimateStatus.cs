using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Status;
using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Neutral.Yandere;

public sealed class IntimateStatus(
	bool hasTask,
	int intimateSeeTaskRate,
	bool isSubTeams) : IStatusModel, ISubTeam
{
	private readonly bool hasTask = hasTask;
	private readonly float intimateSeeTaskRate = intimateSeeTaskRate / 100.0f;
	private readonly HashSet<byte> oneSideedLover = new HashSet<byte>();

	public bool SeeYandere { get; private set; } = false;

	public NeutralSeparateTeam Main => NeutralSeparateTeam.Yandere;
	public NeutralSeparateTeam Sub => NeutralSeparateTeam.YandereSub;

	public bool IsSub { get; } = isSubTeams;

	private bool init = false;

	public void Update(PlayerControl intimatePlayer)
	{
		if (!this.init)
		{
			this.init = true;
			foreach (var player in PlayerCache.AllPlayerControl)
			{
				if (player == null ||
					!ExtremeRoleManager.TryGetSafeCastedRole<YandereRole>(player.PlayerId, out var yandere))
				{
					continue;
				}
				bool isNotNull = yandere.OneSidedLover != null;
				this.init &= isNotNull;
				if (isNotNull)
				{
					this.oneSideedLover.Add(yandere.OneSidedLover!.PlayerId);
				}
			}
		}

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
	public bool IsOneSideLover(SingleRoleBase target)
	{
		foreach (var (id, role) in ExtremeRoleManager.GameRole)
		{
			if (this.oneSideedLover.Contains(id) &&
				role == target)
			{
				return true;
			}
		}
		return false;
	}
}
