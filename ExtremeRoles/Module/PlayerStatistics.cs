using System.Text;
using System.Collections.Generic;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Status;

using Monika = ExtremeRoles.Roles.Solo.Neutral.Monika;
using NeutralMad = ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateRole;
using ExtremeRoles.Roles.Combination.Avalon;
using ExtremeRoles.Roles.Combination.HeroAcademia;


#nullable enable

namespace ExtremeRoles.Module;

public sealed class NeutralSeparateTeamContainer()
{
	public readonly record struct NeutralTeam(NeutralSeparateTeam Team, int Id);
	public readonly record struct NeutralSubTeam(NeutralTeam Main, NeutralSeparateTeam Sub);

	public IReadOnlyDictionary<NeutralTeam, int> Team
	{
		get
		{
			foreach (var (sub, num) in this.subTeam)
			{
				if (this.neutralTeam.ContainsKey(sub.Main))
				{
					continue;
				}
				this.neutralTeam[new NeutralTeam(sub.Sub, sub.Main.Id)] = num;
			}
			return this.neutralTeam;
		}
	}
	private readonly Dictionary<NeutralTeam, int> neutralTeam = [];
	private readonly Dictionary<NeutralSubTeam, int> subTeam = [];

	public void Add(NeutralSeparateTeam team, int id)
	{
		var key = new NeutralTeam(team, id);
		if (this.neutralTeam.TryGetValue(key, out int num))
		{
			this.neutralTeam[key] = num + 1;
		}
		else
		{
			this.neutralTeam.Add(key, 1);
		}
	}
	public void AddSubTeam(NeutralSeparateTeam main, NeutralSeparateTeam sub, int id)
	{
		var key = new NeutralSubTeam(new(main, id), sub);
		if (this.subTeam.TryGetValue(key, out int num))
		{
			this.subTeam[key] = num + 1;
		}
		else
		{
			this.subTeam.Add(key, 1);
		}
	}
}

file sealed class NeutralSeparateTeamBuilder()
{
	public IReadOnlyDictionary<NeutralSeparateTeamContainer.NeutralTeam, int> Team => neutralTeam.Team;
	private readonly NeutralSeparateTeamContainer neutralTeam = new NeutralSeparateTeamContainer();
	private int cacheId = 0;

	public void Add(
		in SingleRoleBase role,
		in ExtremeRoleId roleId,
		in int gameControlId)
	{
		this.cacheId = gameControlId;

		var team = roleId switch
		{
			ExtremeRoleId.Alice => NeutralSeparateTeam.Alice,
			ExtremeRoleId.Jackal or ExtremeRoleId.Sidekick => NeutralSeparateTeam.Jackal,
			ExtremeRoleId.Lover => NeutralSeparateTeam.Lover,
			ExtremeRoleId.Missionary => NeutralSeparateTeam.Missionary,
			ExtremeRoleId.Yandere => NeutralSeparateTeam.Yandere,
			ExtremeRoleId.Miner => NeutralSeparateTeam.Miner,
			ExtremeRoleId.Eater => NeutralSeparateTeam.Eater,
			ExtremeRoleId.Traitor => NeutralSeparateTeam.Traitor,
			ExtremeRoleId.Queen or ExtremeRoleId.Servant => NeutralSeparateTeam.Queen,
			ExtremeRoleId.Delinquent => NeutralSeparateTeam.Kids,
			ExtremeRoleId.Tucker or ExtremeRoleId.Chimera => NeutralSeparateTeam.Tucker,
			_ => NeutralSeparateTeam.None,
		};

		if (team is not NeutralSeparateTeam.None)
		{
			addNeutralTeams(team);
			return;
		}

		// 基本的にはキル持ちの特定の役職のときに実行
		// メインチームが生存 => メインチームの人数にカウントしない、サブチームのカウントも行わない(行ってもいいが消す)
		// メインチームがいない => サブチームのカウントを行う、サブチームが勝てばメインチームの勝利判定
		if (role.CanKill &&
			role.Status is ISubTeam subTeam &&
			subTeam.IsSub)
		{
			this.neutralTeam.AddSubTeam(subTeam.Main, subTeam.Sub, this.cacheId);
		}

		switch (roleId)
		{
			case ExtremeRoleId.Vigilante:
				if (((Vigilante)role).Condition ==
					Vigilante.VigilanteCondition.NewEnemyNeutralForTheShip)
				{
					addNeutralTeams(NeutralSeparateTeam.Vigilante);
				}
				break;
			case ExtremeRoleId.Monika:
				if (((Monika)role).IsSoloTeam)
				{
					addNeutralTeams(NeutralSeparateTeam.Monika);
				}
				break;
			default:
				checkMultiAssignedServant(role);
				break;
		}
	}

	private void checkMultiAssignedServant(SingleRoleBase role)
	{
		if (role is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole?.Core.Id == ExtremeRoleId.Servant)
		{
			addNeutralTeams(NeutralSeparateTeam.Queen);
		}
	}

	private void addNeutralTeams(NeutralSeparateTeam team)
	{
		this.neutralTeam.Add(team, this.cacheId);
	}
}

public sealed record PlayerStatistics(
	int AllTeamCrewmate,
	int TeamImpostorAlive,
	int TeamCrewmateAlive,
	int TeamNeutralAlive,
	int TotalAlive,
	int AssassinAlive,
	IReadOnlyDictionary<int, IWinChecker> SpecialWinCheckRoleAlive,
	IReadOnlyDictionary<NeutralSeparateTeamContainer.NeutralTeam, int> SeparatedNeutralAlive)
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
		var builder = new NeutralSeparateTeamBuilder();
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
			ExtremeRoleType team = role.Core.Team;
			ExtremeRoleId roleId = role.Core.Id;

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
						role is Assassin assassin &&
						assassin.Status is AssassinStatusModel status)
					{
						bool canNeutralKill = status.CanKilledFromNeutral;
						if (!canNeutralKill || (canNeutralKill && !status.CanKilled))
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
					builder.Add(role, roleId, gameControlId);
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
			SeparatedNeutralAlive: builder.Team
		);
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
