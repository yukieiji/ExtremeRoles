using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeRoleAssignee
{
	private readonly struct CombinationRoleAssignData
	{
		public readonly byte CombType;
		public readonly IReadOnlyList<MultiAssignRoleBase> RoleList;
		public readonly int GameControlId;

		public CombinationRoleAssignData(
			int controlId, byte combType,
			IReadOnlyList<MultiAssignRoleBase> roleList)
		{
			CombType = combType;
			RoleList = roleList;
			GameControlId = controlId;
		}
	}

	public IReadOnlyList<PlayerControl> NeedRoleAssignPlayer => assignData.NeedRoleAssignPlayer;

	private readonly PlayerRoleAssignData assignData = new PlayerRoleAssignData();
	private readonly RoleSpawnDataManager spawnData;

	public ExtremeRoleAssignee()
	{
		uint netId = PlayerControl.LocalPlayer.NetId;

		RPCOperator.Call(netId, RPCOperator.Command.Initialize);
		RPCOperator.Initialize();

		spawnData = new RoleSpawnDataManager();

		if (!ExtremeGameModeManager.Instance.EnableXion) { return; }

		PlayerControl loaclPlayer = PlayerControl.LocalPlayer;

		assignData.AddAssignData(
			new PlayerToSingleRoleAssignData(
				loaclPlayer.PlayerId,
				(int)ExtremeRoleId.Xion,
				assignData.GetControlId()));
		assignData.RemvePlayer(loaclPlayer);
	}

	public void CreateAssignData()
	{
		GhostRoleSpawnDataManager.Instance.Create(spawnData.UseGhostCombRole);
		RoleAssignFilter.Instance.Initialize();

		addCombinationExtremeRoleAssignData();
		addSingleExtremeRoleAssignData();
		addNotAssignPlayerToVanillaRoleAssign();
	}

	private void addNotAssignPlayerToVanillaRoleAssign()
	{
		foreach (PlayerControl player in assignData.NeedRoleAssignPlayer)
		{
			var roleId = player.Data.Role.Role;
			Logging.Debug($"------------------- AssignToPlayer:{player.Data.PlayerName} -------------------");
			Logging.Debug($"---AssignRole:{roleId}---");
			assignData.AddAssignData(new PlayerToSingleRoleAssignData(
				player.PlayerId, (byte)roleId, assignData.GetControlId()));
		}
	}

	private void addCombinationExtremeRoleAssignData()
	{
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign Start!! -----------------------------");

		if (!spawnData.CurrentCombRoleSpawnData.Any()) { return; }

		List<CombinationRoleAssignData> combRoleListData = createCombinationRoleListData();
		var shuffledRoleListData = combRoleListData.OrderBy(
			x => RandomGenerator.Instance.Next());
		assignData.Shuffle();

		List<PlayerControl> anotherRoleAssignPlayer = new List<PlayerControl>();

		foreach (var roleListData in shuffledRoleListData)
		{
			foreach (var role in roleListData.RoleList)
			{
				PlayerControl? removePlayer = null;

				foreach (PlayerControl player in assignData.NeedRoleAssignPlayer)
				{
					Logging.Debug(
						$"------------------- AssignToPlayer:{player.Data.PlayerName} -------------------");
					Logging.Debug($"---AssignRole:{role.Id}---");

					bool assign = isCanMulitAssignRoleToPlayer(role, player);

					Logging.Debug($"AssignResult:{assign}");

					if (!assign)
					{
						Logging.Debug($"Assign missing!!");
						continue;
					}
					if (role.CanHasAnotherRole)
					{
						anotherRoleAssignPlayer.Add(player);
					}
					removePlayer = player;

					assignData.AddCombRoleAssignData(
						new PlayerToCombRoleAssignData(
							player.PlayerId, (int)role.Id,
							roleListData.CombType,
							(byte)roleListData.GameControlId,
							(byte)player.Data.Role.Role),
						role.Team);

					Logging.Debug($"------------------- Assign End -------------------");

					break;
				}

				if (removePlayer != null)
				{
					assignData.RemvePlayer(removePlayer);
				}
			}
		}

		foreach (PlayerControl player in anotherRoleAssignPlayer)
		{
			if (player != null)
			{
				Logging.Debug($"------------------- AditionalPlayer -------------------");
				assignData.AddPlayer(player);
			}
		}
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign End!! -----------------------------");
	}

	private void addSingleExtremeRoleAssignData()
	{
		Logging.Debug(
			$"----------------------------- SingleRoleAssign Start!! -----------------------------");
		addImpostorSingleExtremeRoleAssignData();
		addNeutralSingleExtremeRoleAssignData();
		addCrewmateSingleExtremeRoleAssignData();
		Logging.Debug(
			$"----------------------------- SingleRoleAssign End!! -----------------------------");
	}

	private void addImpostorSingleExtremeRoleAssignData()
	{
		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			ExtremeRoleType.Impostor,
			assignData.GetCanImpostorAssignPlayer(),
			[RoleTypes.Shapeshifter, RoleTypes.Phantom]);
	}

	private void addNeutralSingleExtremeRoleAssignData()
	{
		List<PlayerControl> neutralAssignTargetPlayer = new List<PlayerControl>();

		foreach (PlayerControl player in assignData.GetCanCrewmateAssignPlayer())
		{
			RoleTypes vanillaRoleId = player.Data.Role.Role;

			if ((
					assignData.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType team) &&
					team != ExtremeRoleType.Neutral
				)
				||
				(
					!ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign &&
					vanillaRoleId != RoleTypes.Crewmate
				))
			{
				continue;
			}
			neutralAssignTargetPlayer.Add(player);
		}

		int assignNum = Math.Clamp(
			spawnData.MaxRoleNum[ExtremeRoleType.Neutral],
			0, Math.Min(
				neutralAssignTargetPlayer.Count,
				spawnData.CurrentSingleRoleUseNum[ExtremeRoleType.Neutral]));

		Logging.Debug($"Neutral assign num:{assignNum}");

		neutralAssignTargetPlayer = neutralAssignTargetPlayer.OrderBy(
			x => RandomGenerator.Instance.Next()).Take(assignNum).ToList();

		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			ExtremeRoleType.Neutral,
			neutralAssignTargetPlayer,
			[RoleTypes.Engineer, RoleTypes.Scientist, RoleTypes.Noisemaker, RoleTypes.Tracker]);
	}

	private void addCrewmateSingleExtremeRoleAssignData()
	{
		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			ExtremeRoleType.Crewmate,
			assignData.GetCanCrewmateAssignPlayer(),
			[RoleTypes.Engineer, RoleTypes.Scientist, RoleTypes.Noisemaker, RoleTypes.Tracker]);
	}

	private void addSingleExtremeRoleAssignDataFromTeamAndPlayer(
		ExtremeRoleType team,
		in IReadOnlyList<PlayerControl> targetPlayer,
		in HashSet<RoleTypes> vanilaTeams)
	{

		Dictionary<int, SingleRoleSpawnData> teamSpawnData = spawnData.CurrentSingleRoleSpawnData[team];

		if (!targetPlayer.Any() || !targetPlayer.Any()) { return; }

		List<(int intedRoleId, int weight)> spawnCheckRoleId =
			createSingleRoleIdData(teamSpawnData);

		if (!spawnCheckRoleId.Any()) { return; }

		var shuffledSpawnCheckRoleId = spawnCheckRoleId
			.OrderByDescending(x => x.weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()) //同じ重みをシャッフル
			.Select(x => x.intedRoleId)
			.ToList();
		var shuffledTargetPlayer = targetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

		foreach (PlayerControl player in shuffledTargetPlayer)
		{
			Logging.Debug(
				$"-------------------AssignToPlayer:{player.Data.PlayerName}-------------------");
			PlayerControl? removePlayer = null;

			RoleTypes vanillaRoleId = player.Data.Role.Role;

			if (vanilaTeams.Contains(vanillaRoleId))
			{
				// マルチアサインでコンビ役職にアサインされてないプレイヤーは追加でアサインが必要
				removePlayer =
					ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign
					||
					(
						assignData.TryGetCombRoleAssign(player.PlayerId, out ExtremeRoleType combTeam) &&
						combTeam == team
					)
					? null : player;

				assignData.AddAssignData(
					new PlayerToSingleRoleAssignData(
						player.PlayerId, (int)vanillaRoleId,
						assignData.GetControlId()));
				Logging.Debug($"---AssignRole:{vanillaRoleId}---");
			}

			if (spawnData.IsCanSpawnTeam(team) &&
				shuffledSpawnCheckRoleId.Any() &&
				removePlayer == null)
			{
				for (int i = 0; i < shuffledSpawnCheckRoleId.Count; ++i)
				{
					int intedRoleId = shuffledSpawnCheckRoleId[i];

					if (RoleAssignFilter.Instance.IsBlock(intedRoleId)) { continue; }

					removePlayer = player;
					shuffledSpawnCheckRoleId.RemoveAt(i);

					Logging.Debug($"---AssignRole:{intedRoleId}---");

					spawnData.ReduceSpawnLimit(team);
					assignData.AddAssignData(
						new PlayerToSingleRoleAssignData(
							player.PlayerId, intedRoleId, assignData.GetControlId()));

					RoleAssignFilter.Instance.Update(intedRoleId);
					break;
				}
			}

			Logging.Debug($"-------------------AssignEnd-------------------");
			if (removePlayer != null)
			{
				assignData.RemvePlayer(removePlayer);
			}
		}
	}

	private List<CombinationRoleAssignData> createCombinationRoleListData()
	{
		List<CombinationRoleAssignData> roleListData = new List<CombinationRoleAssignData>();

		int curImpNum = 0;
		int curCrewNum = 0;
		int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
			Int32OptionNames.NumImpostors);

		NotAssignPlayerData notAssignPlayer = new NotAssignPlayerData();
		var shuffleCombRole = spawnData.CurrentCombRoleSpawnData
			.OrderByDescending(x => x.Value.Weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()); //その上で全体のソート

		foreach (var (combType, combSpawnData) in shuffleCombRole)
		{
			var roleManager = combSpawnData.Role;

			for (int i = 0; i < combSpawnData.SpawnSetNum; i++)
			{
				roleManager.AssignSetUpInit(curImpNum);
				bool isSpawn = combSpawnData.IsSpawn();

				int reduceCrewmateRole = 0;
				int reduceImpostorRole = 0;
				int reduceNeutralRole = 0;

				foreach (var role in roleManager.Roles)
				{
					switch (role.Team)
					{
						case ExtremeRoleType.Crewmate:
							++reduceCrewmateRole;
							break;
						case ExtremeRoleType.Impostor:
							++reduceImpostorRole;
							break;
						case ExtremeRoleType.Neutral:
							++reduceNeutralRole;
							break;
						default:
							break;
					}
					if (roleManager is GhostAndAliveCombinationRoleManagerBase)
					{
						isSpawn = !GhostRoleSpawnDataManager.Instance.IsGlobalSpawnLimit(role.Team);
					}
				}

				isSpawn = (
					isSpawn &&
					isCombinationLimit(
						notAssignPlayer,
						maxImpNum,
						curCrewNum, curImpNum,
						reduceCrewmateRole,
						reduceImpostorRole,
						reduceNeutralRole,
						combSpawnData.IsMultiAssign) &&
					!RoleAssignFilter.Instance.IsBlock(combType));

				if (!isSpawn) { continue; }

				spawnData.ReduceSpawnLimit(ExtremeRoleType.Crewmate, reduceCrewmateRole);
				spawnData.ReduceSpawnLimit(ExtremeRoleType.Impostor, reduceImpostorRole);
				spawnData.ReduceSpawnLimit(ExtremeRoleType.Neutral, reduceNeutralRole);

				curImpNum = curImpNum + reduceImpostorRole;
				curCrewNum = curCrewNum + (reduceCrewmateRole + reduceNeutralRole);

				var spawnRoles = new List<MultiAssignRoleBase>();
				foreach (var role in roleManager.Roles)
				{
					spawnRoles.Add((MultiAssignRoleBase)role.Clone());
				}

				notAssignPlayer.ReduceImpostorAssignNum(reduceImpostorRole);
				roleListData.Add(
					new CombinationRoleAssignData(
						assignData.GetControlId(),
						combType, spawnRoles));

				RoleAssignFilter.Instance.Update(combType);
			}
		}

		return roleListData;
	}

	private bool isCombinationLimit(
		in NotAssignPlayerData notAssignPlayer,
		int maxImpNum,
		int curCrewUseNum,
		int curImpUseNum,
		int reduceCrewmateRoleNum,
		int reduceImpostorRoleNum,
		int reduceNeutralRoleNum,
		bool isMultiAssign)
	{
		int crewNotAssignPlayerNum = isMultiAssign ?
			notAssignPlayer.CrewmateMultiAssignPlayerNum :
			notAssignPlayer.CrewmateSingleAssignPlayerNum;
		int impNotAssignPlayerNum = isMultiAssign ?
			notAssignPlayer.ImpostorMultiAssignPlayerNum :
			notAssignPlayer.ImpostorSingleAssignPlayerNum;

		int totalReduceCrewmateNum = reduceCrewmateRoleNum + reduceNeutralRoleNum;

		bool isLimitCrewAssignNum = crewNotAssignPlayerNum >= totalReduceCrewmateNum;
		bool isLimitImpAssignNum = impNotAssignPlayerNum >= reduceImpostorRoleNum;

		return
			// まずはアサインの上限チェック
			(
				curCrewUseNum + totalReduceCrewmateNum <= crewNotAssignPlayerNum &&
				curImpUseNum + reduceImpostorRoleNum <= maxImpNum
			)
			// クルーのスポーン上限チェック
			&&
			(
				spawnData.IsCanSpawnTeam(ExtremeRoleType.Crewmate, reduceCrewmateRoleNum) &&
				isLimitCrewAssignNum
			)
			// ニュートラルのスポーン上限チェック
			&&
			(
				spawnData.IsCanSpawnTeam(ExtremeRoleType.Neutral, reduceNeutralRoleNum) &&
				isLimitCrewAssignNum
			)
			// インポスターのスポーン上限チェック
			&&
			(
				spawnData.IsCanSpawnTeam(ExtremeRoleType.Impostor, reduceImpostorRoleNum) &&
				isLimitImpAssignNum
			);
	}

	private static List<(int intedRoleId, int weight)> createSingleRoleIdData(
		in IReadOnlyDictionary<int, SingleRoleSpawnData> spawnData)
	{
		List<(int, int)> result = new List<(int, int)>();

		foreach (var (intedRoleId, data) in spawnData)
		{
			for (int i = 0; i < data.SpawnSetNum; ++i)
			{
				if (!data.IsSpawn()) { continue; }

				result.Add((intedRoleId, data.Weight));
			}
		}

		return result;
	}

	private static bool isCanMulitAssignRoleToPlayer(
		in MultiAssignRoleBase role,
		in PlayerControl player)
	{

		RoleTypes roleType = player.Data.Role.Role;

		bool hasAnotherRole = role.CanHasAnotherRole;
		bool isImpostor = role.IsImpostor();
		bool isAssignToCrewmate = role.IsCrewmate() || role.IsNeutral();

		return
			(
				roleType is RoleTypes.Crewmate && isAssignToCrewmate
			)
			||
			(
				roleType is RoleTypes.Impostor && isImpostor
			)
			||
			(
				(
					roleType is
						RoleTypes.Engineer or
						RoleTypes.Scientist or
						RoleTypes.Noisemaker or
						RoleTypes.Tracker
				)
				&& hasAnotherRole && isAssignToCrewmate
			)
			||
			(
				(
					roleType is
						RoleTypes.Shapeshifter or
						RoleTypes.Phantom
				) &&
				hasAnotherRole && isImpostor
			);
	}

	public void Assign()
	{
		this.assignData.AllPlayerAssignToExRole();
	}
}
