using System.Collections.Generic;
using System.Text;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;

using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.GameMode;

using NeutralMad = ExtremeRoles.Roles.Solo.Neutral.Madmate;

#nullable enable

namespace ExtremeRoles.Module;

public sealed record PlayerStatistics(
	int AllTeamCrewmate,
	int TeamImpostorAlive,
	int TeamCrewmateAlive,
	int TeamNeutralAlive,
	int TotalAlive,
	int AssassinAlive,
	IReadOnlyDictionary<int, IWinChecker> SpecialWinCheckRoleAlive,
	IReadOnlyDictionary<(NeutralSeparateTeam, int), int> SeparatedNeutralAlive)
{
	public const int SameNeutralGameControlId = int.MaxValue;

	public override string ToString()
	{
		var builder = new StringBuilder();

		builder.AppendLine("------------ Current Player Statistics ------------");
		builder.AppendLine($"Total Player Alive : {this.TotalAlive}");
		builder.AppendLine($"Crewmate Alive : {this.TeamCrewmateAlive} / {this.AllTeamCrewmate}");
		builder.AppendLine($"Impostor Alive :{this.TeamImpostorAlive}");
		builder.AppendLine($"Assassin Alive : {this.AssassinAlive}");
		builder.AppendLine($"Neutral Alive : {this.TeamNeutralAlive}");

		builder.AppendLine("------ Neutral Win Special Checker ------");
		foreach (var (id, winChecker) in this.SpecialWinCheckRoleAlive)
		{
			builder.AppendLine(
				$"{winChecker} --- GameControlId:{id} --- IsWin:{winChecker.IsWin(this)}");
		}

		builder.AppendLine(
			$"------ Neutral Separate Teams --- CurrentAliveTeams:{this.SeparatedNeutralAlive.Count} ------");
		foreach (var ((team, id), aliveNum) in this.SeparatedNeutralAlive)
		{
			builder.AppendLine(
				$"{team} --- GameControlId:{id} --- AliveNum: {aliveNum}");
		}
		return builder.ToString();
	}

	public static PlayerStatistics Create()
	{
		int numTotalAlive = 0;

		int numCrew = 0;
		int numCrewAlive = 0;

		int numImpostorAlive = 0;

		int numNeutralAlive = 0;

		int numAssassinAlive = 0;
		Dictionary<(NeutralSeparateTeam, int), int> neutralTeam = new Dictionary<
			(NeutralSeparateTeam, int), int>();
		Dictionary<int, IWinChecker> specialWinCheckRoleAlive = new Dictionary<
			int, IWinChecker>();

		foreach (NetworkedPlayerInfo playerInfo in
			GameData.Instance.AllPlayers.GetFastEnumerator())
		{
			if (playerInfo == null ||
				playerInfo.Disconnected)
			{
				continue;
			}

			SingleRoleBase role = ExtremeRoleManager.GameRole[playerInfo.PlayerId];
			ExtremeRoleType team = role.Team;
			ExtremeRoleId roleId = role.Id;

			// クルーのカウントを数える
			if (role.IsCrewmate())
			{
				++numCrew;
			}

			// 死んでたら次のプレイヤーへ
			if (playerInfo.IsDead)
			{
				continue;
			}

			// マッドメイトの生存をカウントしないオプション
			if (roleId == ExtremeRoleId.Madmate &&
				role is NeutralMad madmate &&
				madmate.IsDontCountAliveCrew)
			{
				continue;
			}

			++numTotalAlive;
			int gameControlId = role.GameControlId;

			if (ExtremeRoleManager.SpecialWinCheckRole.Contains(roleId))
			{
				addSpecialWinCheckRole(
					in specialWinCheckRoleAlive,
					gameControlId,
					roleId, role,
					playerInfo.PlayerId);
			}

			// 生きてる
			switch (team)
			{
				case ExtremeRoleType.Crewmate:
					++numCrewAlive;
					break;
				case ExtremeRoleType.Impostor:

					// アサシンがニュートラルを切れない時
					if (roleId == ExtremeRoleId.Assassin &&
						role is Assassin assassin)
					{
						bool canNeutralKill = assassin.CanKilledFromNeutral;
						if (!canNeutralKill || (canNeutralKill && !assassin.CanKilled))
						{
							++numAssassinAlive;
						}
					}

					++numImpostorAlive;
					break;
				case ExtremeRoleType.Neutral:

					if (ExtremeGameModeManager.Instance.ShipOption.IsSameNeutralSameWin)
					{
						gameControlId = SameNeutralGameControlId;
					}

					++numNeutralAlive;
					neutalCondition(role, roleId, gameControlId, in neutralTeam);
					break;

				default:
					break;
			}
		}

		return new PlayerStatistics(

			AllTeamCrewmate: numCrew,

			TeamImpostorAlive: numImpostorAlive,
			TeamCrewmateAlive: numCrewAlive,
			TeamNeutralAlive: numNeutralAlive,
			TotalAlive: numTotalAlive,
			AssassinAlive: numAssassinAlive,

			SpecialWinCheckRoleAlive: specialWinCheckRoleAlive,
			SeparatedNeutralAlive: neutralTeam
		);
	}

	private static void neutalCondition(
		in SingleRoleBase role,
		in ExtremeRoleId roleId,
		in int gameControlId,
		in Dictionary<(NeutralSeparateTeam, int), int> neutralTeam)
	{
		switch (roleId)
		{
			case ExtremeRoleId.Alice:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Alice);
				break;
			case ExtremeRoleId.Jackal:
			case ExtremeRoleId.Sidekick:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Jackal);
				break;
			case ExtremeRoleId.Lover:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Lover);
				break;
			case ExtremeRoleId.Missionary:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Missionary);
				break;
			case ExtremeRoleId.Yandere:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Yandere);
				break;
			case ExtremeRoleId.Vigilante:
				if (((Vigilante)role).Condition ==
					Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
				{
					addNeutralTeams(
						neutralTeam,
						gameControlId,
						NeutralSeparateTeam.Vigilante);
				}
				break;
			case ExtremeRoleId.Miner:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Miner);
				break;
			case ExtremeRoleId.Eater:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Eater);
				break;
			case ExtremeRoleId.Traitor:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Traitor);
				break;
			case ExtremeRoleId.Queen:
			case ExtremeRoleId.Servant:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Queen);
				break;
			case ExtremeRoleId.Delinquent:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Kids);
				break;
			case ExtremeRoleId.Tucker:
			case ExtremeRoleId.Chimera:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Tucker);
				break;
			case ExtremeRoleId.Monika:
				addNeutralTeams(
					neutralTeam,
					gameControlId,
					NeutralSeparateTeam.Monika);
				break;
			default:
				checkMultiAssignedServant(
					in neutralTeam,
					gameControlId, role);
				break;
		}
	}

	private static void checkMultiAssignedServant(
		in Dictionary<(NeutralSeparateTeam, int), int> neutralTeam,
		int gameControlId,
		SingleRoleBase role)
	{
		if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole?.Id == ExtremeRoleId.Servant)
		{
			addNeutralTeams(
				neutralTeam,
				gameControlId,
				NeutralSeparateTeam.Queen);
		}
	}

	private static void addNeutralTeams(
		in Dictionary<(NeutralSeparateTeam, int), int> neutralTeam,
		int gameControlId,
		NeutralSeparateTeam team)
	{
		var key = (team, gameControlId);

		if (neutralTeam.ContainsKey(key))
		{
			neutralTeam[key] = neutralTeam[key] + 1;
		}
		else
		{
			neutralTeam.Add(key, 1);
		}
	}

	private static void addSpecialWinCheckRole(
		in Dictionary<int, IWinChecker> roleData,
		int gameControlId,
		ExtremeRoleId roleId,
		SingleRoleBase role,
		byte playerId)
	{

		if (roleData.TryGetValue(gameControlId, out var winChecker))
		{
			winChecker.AddAliveRole(playerId, role);
		}
		else
		{
			winChecker = roleId switch
			{
				ExtremeRoleId.Lover => new LoverWinChecker(),
				ExtremeRoleId.Vigilante => new VigilanteWinChecker(),
				ExtremeRoleId.Delinquent => new KidsWinChecker(),
				ExtremeRoleId.Yandere => new YandereWinChecker(),
				ExtremeRoleId.Hatter => new HatterWinChecker(),
				ExtremeRoleId.Monika => new MonikaAliveWinChecker(),
				_ => null,
			};
			if (winChecker is not null)
			{
				winChecker.AddAliveRole(playerId, role);
				roleData.Add(gameControlId, winChecker);
			}
		}
	}
}
