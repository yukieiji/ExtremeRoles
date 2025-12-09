using ExtremeRoles.Module.GameEnd;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo.Neutral.Queen;
using ExtremeRoles.Roles.Solo.Neutral.Yandere;
using System.Collections.Generic;


namespace ExtremeRoles.Module.SpecialWinChecker;

internal sealed class YandereWinChecker : IWinChecker
{
	public RoleGameOverReason Reason => RoleGameOverReason.YandereShipJustForTwo;

	private List<YandereRole> aliveYandere = new List<YandereRole>();

	public YandereWinChecker()
	{
		aliveYandere.Clear();
	}

	public void AddAliveRole(
		byte playerId, SingleRoleBase role)
	{
		aliveYandere.Add((YandereRole)role);
	}

	public bool IsWin(
		PlayerStatistics statistics)
	{
		List<PlayerControl> aliveOneSideLover = [];

		int oneSidedLoverImpNum = 0;
		int oneSidedLoverNeutralNum = 0;
		int oneSidedLoverLiberalMillitant = 0;

		foreach (var role in aliveYandere)
		{
			if (role.OneSidedLover == null ||
				role.OneSidedLover.Data == null ||
				!ExtremeRoleManager.TryGetRole(
					role.OneSidedLover.Data.PlayerId,
					out var oneSidedLoverRole))
			{
				continue;
			}

			var playerInfo = role.OneSidedLover.Data;

			if (playerInfo.IsDead || playerInfo.Disconnected)
			{
				continue;
			}

			aliveOneSideLover.Add(role.OneSidedLover);

			if (oneSidedLoverRole.IsImpostor())
			{
				++oneSidedLoverImpNum;
			}
			else if (oneSidedLoverRole.IsNeutral())
			{
				switch (oneSidedLoverRole.Core.Id)
				{
					case ExtremeRoleId.Alice:
					case ExtremeRoleId.Jackal:
					case ExtremeRoleId.Sidekick:
					case ExtremeRoleId.Lover:
					case ExtremeRoleId.Missionary:
					case ExtremeRoleId.Miner:
					case ExtremeRoleId.Eater:
					case ExtremeRoleId.Traitor:
					case ExtremeRoleId.Queen:
					case ExtremeRoleId.Delinquent:
					case ExtremeRoleId.Chimera:
						++oneSidedLoverNeutralNum;
						break;

					default:
						// どっちかがサーヴァント
						if (ExtremeRoleManager.TryGetSafeCastedRole<ServantRole>(playerInfo.PlayerId, out var servant) &&
							!servant.Loader.GetValue<QueenRole.QueenOption, bool>(QueenRole.QueenOption.ServantSucideWithQueenWhenHasKill) &&
							servant.CanKill() &&
							servant.Status is ServantStatus servantStatus)
						{
							byte parent = servantStatus.Parent;
							var parentPlayer = GameData.Instance.GetPlayerById(parent);
							if (parentPlayer != null && 
								(parentPlayer.IsDead ||parentPlayer.Disconnected))
							{
								++oneSidedLoverNeutralNum;
							}
						}
						break;
				}
			}
			else if (
				oneSidedLoverRole.IsLiberal() &&
				oneSidedLoverRole.CanKill() &&
				// リーダーではないか、無敵ではないリーダーの時だけ増やす
				/// ミリタントの計算時、キル持ち無敵のリーダーはミリタントではない扱いなので
				(oneSidedLoverRole.Core.Id is not ExtremeRoleId.Leader || !statistics.LeaderIsBlockKill))
			{
				++oneSidedLoverLiberalMillitant;
			}
		}

		int aliveNum = this.aliveYandere.Count + aliveOneSideLover.Count;

		if (aliveOneSideLover.Count == 0 ||
			this.aliveYandere.Count == 0 ||
			aliveNum < statistics.TotalAlive - aliveNum ||
			statistics.TeamImpostorAlive - statistics.AssassinAlive - oneSidedLoverImpNum > 0 ||
			statistics.SeparatedNeutralAlive.Count - oneSidedLoverNeutralNum > 1 ||
			statistics.LiberalMilitantAlive - oneSidedLoverLiberalMillitant > 1)
		{
			ExtremeRolesPlugin.Logger.LogInfo($"ImpCount: {statistics.TeamImpostorAlive}");
			ExtremeRolesPlugin.Logger.LogInfo($"oneSideNeut: {oneSidedLoverNeutralNum}");
			ExtremeRolesPlugin.Logger.LogInfo($"NeutralCount: {statistics.SeparatedNeutralAlive.Count}");
			return false;
		}

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.SetWinPlayer))
		{
			caller.WriteInt(aliveOneSideLover.Count);
			foreach (var player in aliveOneSideLover)
			{
				caller.WriteByte(player.PlayerId);
				ExtremeRolesPlugin.ShipState.AddWinner(player);
			}
		}

		return true;
	}
}
