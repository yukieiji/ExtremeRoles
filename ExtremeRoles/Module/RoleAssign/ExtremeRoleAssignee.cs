using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign;

public sealed class ExtremeRoleAssignee(
	IVanillaRoleProvider roleProvider,
	PlayerRoleAssignData assignData,
	RoleSpawnDataManager spawnData) : IRoleAssignee
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

	private readonly PlayerRoleAssignData assignData = assignData;
	private readonly RoleSpawnDataManager spawnData = spawnData;

	private readonly IReadOnlySet<RoleTypes> vanillaCrewRoleType = roleProvider.CrewmateRole;
	private readonly IReadOnlySet<RoleTypes> vanillaImpRoleType = roleProvider.ImpostorRole;

	public IEnumerator CoRpcAssign()
	{
		createAssignData();

		yield return null;

		this.assignData.AllPlayerAssignToExRole();
	}

	private void createAssignData()
	{
		if (ExtremeGameModeManager.Instance.EnableXion)
		{
			PlayerControl loaclPlayer = PlayerControl.LocalPlayer;

			this.assignData.AddAssignData(
				new PlayerToSingleRoleAssignData(
					loaclPlayer.PlayerId,
					(int)ExtremeRoleId.Xion,
					assignData.ControlId));
			this.assignData.RemveFromPlayerControl(loaclPlayer);
		}

		GhostRoleSpawnDataManager.Instance.Create(spawnData.UseGhostCombRole);

		RoleAssignFilter.Instance.Initialize();

		addCombinationExtremeRoleAssignData();
		addSingleExtremeRoleAssignData();
		addNotAssignPlayerToVanillaRoleAssign();
	}

	#region Create CombinationRole Assign Data
	private void addCombinationExtremeRoleAssignData()
	{
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign Start!! -----------------------------");

		if (spawnData.CurrentCombRoleSpawnData.Count == 0) { return; }

		List<CombinationRoleAssignData> combRoleListData = createCombinationRoleListData();
		var shuffledRoleListData = combRoleListData.OrderBy(
			x => RandomGenerator.Instance.Next());
		assignData.Shuffle();

		var anotherRoleAssignPlayer = new List<VanillaRolePlayerAssignData>();

		foreach (var roleListData in shuffledRoleListData)
		{
			foreach (var role in roleListData.RoleList)
			{
				VanillaRolePlayerAssignData? removePlayer = null;

				foreach (var player in assignData.NeedRoleAssignPlayer)
				{
					Logging.Debug(
						$"------------------- AssignToPlayer:{player.PlayerName} -------------------");
					Logging.Debug($"---AssignRole:{role.Id}---");

					RoleTypes vanillaRole = player.Role;
					bool assign = canMulitAssignRoleToPlayer(role, vanillaRole);

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
							(byte)vanillaRole),
						role.Team);

					Logging.Debug($"------------------- Assign End -------------------");

					break;
				}

				if (removePlayer.HasValue)
				{
					assignData.RemvePlayer(removePlayer.Value);
				}
			}
		}

		Logging.Debug($"------------------- AditionalPlayer -------------------");
		foreach (var player in anotherRoleAssignPlayer)
		{
			Logging.Debug($"------------------- AddPlayer:{player.PlayerName} -------------------");
			assignData.AddPlayer(player);
		}
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign End!! -----------------------------");
	}

	private List<CombinationRoleAssignData> createCombinationRoleListData()
	{
		var roleListData = new List<CombinationRoleAssignData>();

		int curImpNum = 0;
		int curCrewNum = 0;
		int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
			Int32OptionNames.NumImpostors);

		var notAssignPlayer = new NotAssignPlayerData();
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
				spawnData.ReduceSpawnLimit(ExtremeRoleType.Neutral , reduceNeutralRole );

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
						assignData.ControlId,
						combType, spawnRoles));

				RoleAssignFilter.Instance.Update(combType);
			}
		}

		return roleListData;
	}

	private static bool canMulitAssignRoleToPlayer(
		in MultiAssignRoleBase role,
		in RoleTypes roleType)
	{
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
	#endregion

	#region Create SingleRole Assign Data
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
			vanillaImpRoleType);
	}

	private void addNeutralSingleExtremeRoleAssignData()
	{
		var neutralAssignTargetPlayer = new List<VanillaRolePlayerAssignData>();

		foreach (var player in assignData.GetCanCrewmateAssignPlayer())
		{
			RoleTypes vanillaRoleId = player.Role;

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
			vanillaCrewRoleType);
	}

	private void addCrewmateSingleExtremeRoleAssignData()
	{
		addSingleExtremeRoleAssignDataFromTeamAndPlayer(
			ExtremeRoleType.Crewmate,
			assignData.GetCanCrewmateAssignPlayer(),
			vanillaCrewRoleType);
	}

	private void addSingleExtremeRoleAssignDataFromTeamAndPlayer(
		ExtremeRoleType team,
		in IReadOnlyList<VanillaRolePlayerAssignData> targetPlayer,
		in IReadOnlySet<RoleTypes> vanilaTeams)
	{

		var teamSpawnData = spawnData.CurrentSingleRoleSpawnData[team];

		if (targetPlayer.Count == 0) { return; }

		var spawnCheckRoleId = createSingleRoleIdData(teamSpawnData);

		if (spawnCheckRoleId.Count == 0) { return; }

		var shuffledSpawnCheckRoleId = spawnCheckRoleId
			.OrderByDescending(x => x.weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()) //同じ重みをシャッフル
			.Select(x => x.intedRoleId)
			.ToList();
		var shuffledTargetPlayer = targetPlayer.OrderBy(x => RandomGenerator.Instance.Next());

		foreach (var player in shuffledTargetPlayer)
		{
			Logging.Debug(
				$"-------------------AssignToPlayer:{player.PlayerName}-------------------");
			VanillaRolePlayerAssignData? removePlayer = null;

			RoleTypes vanillaRoleId = player.Role;

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
						assignData.ControlId));
				Logging.Debug($"---AssignRole:{vanillaRoleId}---");
			}

			if (spawnData.IsCanSpawnTeam(team) &&
				shuffledSpawnCheckRoleId.Count > 0 &&
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
							player.PlayerId, intedRoleId, assignData.ControlId));

					RoleAssignFilter.Instance.Update(intedRoleId);
					break;
				}
			}

			Logging.Debug($"-------------------AssignEnd-------------------");
			if (removePlayer.HasValue)
			{
				assignData.RemvePlayer(removePlayer.Value);
			}
		}
	}

	private static List<(int intedRoleId, int weight)> createSingleRoleIdData(
		in IReadOnlyDictionary<int, SingleRoleSpawnData> spawnData)
	{
		var result = new List<(int, int)>();

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
	#endregion

	#region Post prosesss for not assign player
	private void addNotAssignPlayerToVanillaRoleAssign()
	{
		foreach (var player in assignData.NeedRoleAssignPlayer)
		{
			var roleId = player.Role;
			Logging.Debug($"------------------- AssignToPlayer:{player.PlayerName} -------------------");
			Logging.Debug($"---AssignRole:{roleId}---");
			assignData.AddAssignData(new PlayerToSingleRoleAssignData(
				player.PlayerId, (byte)roleId, assignData.ControlId));
		}
	}
	#endregion
}
