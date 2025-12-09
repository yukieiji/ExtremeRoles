using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Module.GameEnd;

public sealed class NeutralSpecialWinChecker : IGameEndChecker
{
	public bool TryCheckGameEnd(out GameOverReason reason)
	{
		reason = (GameOverReason)RoleGameOverReason.UnKnown;

		foreach (var role in ExtremeRoleManager.GameRole.Values)
		{

			if (!(role.IsWin && role.IsNeutral()))
			{
				continue;
			}

			IGameEndChecker.SetWinGameContorlId(role.GameControlId);

			reason = (GameOverReason)(role.Core.Id switch
			{
				ExtremeRoleId.Alice => RoleGameOverReason.AliceKilledByImposter,
				ExtremeRoleId.TaskMaster => RoleGameOverReason.TaskMasterGoHome,
				ExtremeRoleId.Jester => RoleGameOverReason.JesterMeetingFavorite,
				ExtremeRoleId.Eater => RoleGameOverReason.EaterAllEatInTheShip,
				ExtremeRoleId.Umbrer => RoleGameOverReason.UmbrerBiohazard,
				ExtremeRoleId.Hatter => RoleGameOverReason.HatterEndlessTeaTime,
				ExtremeRoleId.Artist => RoleGameOverReason.ArtistShipToArt,
				_ => RoleGameOverReason.UnKnown,
			});
			return true;
		}

		return false;
	}
}
