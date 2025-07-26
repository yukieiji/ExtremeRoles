using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Module.SpecialWinChecker
{
	internal sealed class VigilanteWinChecker : IWinChecker
	{
		public RoleGameOverReason Reason => RoleGameOverReason.VigilanteNewIdealWorld;

		public void AddAliveRole(
			byte playerId, SingleRoleBase role)
		{ }

		public bool IsWin(
			PlayerStatistics statistics)
		{
			int heroNum = 0;
			int villanNum = 0;
			int vigilanteNum = 0;


			foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
			{
				var playerInfo = GameData.Instance.GetPlayerById(playerId);
				if (!playerInfo.IsDead)
				{
					var id = role.Core.Id;
					if (id == ExtremeRoleId.Hero)
					{
						++heroNum;
					}
					else if (id == ExtremeRoleId.Villain)
					{
						++villanNum;
					}
					else if (id == ExtremeRoleId.Vigilante)
					{
						++vigilanteNum;
					}
					else
					{
						return false;
					}
				}
			}

			if (heroNum > 0 && villanNum > 0 && vigilanteNum > 0)
			{
				return true;
			}
			return false;
		}
	}
}
