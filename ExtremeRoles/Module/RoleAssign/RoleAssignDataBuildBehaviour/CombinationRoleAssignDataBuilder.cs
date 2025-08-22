using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Helper;

#nullable enable

namespace ExtremeRoles.Module.RoleAssign.RoleAssignDataBuildBehaviour;

public sealed class CombinationRoleAssignDataBuilder : IRoleAssignDataBuildBehaviour
{
	private readonly record struct CombinationRoleAssignData(
		byte CombType,
		IReadOnlyList<MultiAssignRoleBase> RoleList,
		int GameControlId);

	public int Priority => (int)ExtremeRoleAssignDataBuilder.Priority.Combination;

	public void Build(in PreparationData data)
	{
		if (data.RoleSpawn.CurrentCombRoleSpawnData.Count == 0)
		{
			return;
		}
		addCombinationExtremeRoleAssignData(data);
	}

	#region Create CombinationRole Assign Data
	private void addCombinationExtremeRoleAssignData(in PreparationData data)
	{
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign - Start -----------------------------");

		var combRoleListData = createCombinationRoleListData(data);
		var shuffledRoleListData = combRoleListData.OrderBy(
			x => RandomGenerator.Instance.Next());

		var anotherRoleAssignPlayer = new List<VanillaRolePlayerAssignData>();

		foreach (var roleListData in shuffledRoleListData)
		{
			foreach (var role in roleListData.RoleList)
			{
				VanillaRolePlayerAssignData? removePlayer = null;

				foreach (var player in data.Assign.NeedRoleAssignPlayer)
				{
					Logging.Debug(
						$"------------------- AssignToPlayer:{player.PlayerName} -------------------");
					Logging.Debug($"---AssignRole:{role.Id}---");

					var vanillaRole = player.Role;
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

					if (!data.Assign.TryAddCombRoleAssignData(
							new PlayerToCombRoleAssignData(
								player.PlayerId, (int)role.Id,
								roleListData.CombType,
								(byte)roleListData.GameControlId,
								(byte)vanillaRole),
							role.Team))
					{
						Logging.Debug($"Cannnot add assignData");
						continue;
					}

					Logging.Debug($"------------------- Assign End -------------------");

					break;
				}

				if (removePlayer.HasValue)
				{
					data.Assign.RemvePlayer(removePlayer.Value);
				}
			}
		}

		Logging.Debug($"------------------- AditionalPlayer -------------------");
		foreach (var player in anotherRoleAssignPlayer)
		{
			Logging.Debug($"------------------- AddPlayer:{player.PlayerName} -------------------");
			data.Assign.AddPlayer(player);
		}
		Logging.Debug(
			$"----------------------------- CombinationRoleAssign - End -----------------------------");
	}

	private IReadOnlyList<CombinationRoleAssignData> createCombinationRoleListData(
		in PreparationData data)
	{
		var roleListData = new List<CombinationRoleAssignData>();

		int curImpNum = 0;
		int curCrewNum = 0;
		int maxImpNum = GameOptionsManager.Instance.CurrentGameOptions.GetInt(
			Int32OptionNames.NumImpostors);

		var notAssignPlayer = new NotAssignPlayerData();
		var shuffleCombRole = data.RoleSpawn.CurrentCombRoleSpawnData
			.OrderByDescending(x => x.Value.Weight) // まずは重みでソート
			.ThenBy(x => RandomGenerator.Instance.Next()); //その上で全体のソート

		var limiter = data.Limit;

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
				int reduceLiberalRole = 0;

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
						case ExtremeRoleType.Liberal:
							++reduceLiberalRole;
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
						limiter,
						notAssignPlayer,
						maxImpNum,
						curCrewNum, curImpNum,
						reduceCrewmateRole,
						reduceImpostorRole,
						reduceNeutralRole,
						reduceLiberalRole,
						combSpawnData.IsMultiAssign) &&
					!RoleAssignFilter.Instance.IsBlock(combType));

				if (!isSpawn) { continue; }

				limiter.Reduce(ExtremeRoleType.Crewmate, reduceCrewmateRole);
				limiter.Reduce(ExtremeRoleType.Impostor, reduceImpostorRole);
				limiter.Reduce(ExtremeRoleType.Neutral, reduceNeutralRole);
				limiter.Reduce(ExtremeRoleType.Liberal, reduceLiberalRole);

				curImpNum = curImpNum + reduceImpostorRole;
				curCrewNum = curCrewNum + (reduceCrewmateRole + reduceNeutralRole + reduceLiberalRole);

				var spawnRoles = new List<MultiAssignRoleBase>();
				foreach (var role in roleManager.Roles)
				{
					spawnRoles.Add((MultiAssignRoleBase)role.Clone());
				}

				notAssignPlayer.ReduceImpostorAssignNum(reduceImpostorRole);
				roleListData.Add(
					new CombinationRoleAssignData(
						combType, spawnRoles,
						data.Assign.ControlId));

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
		ISpawnLimiter limiter,
		in NotAssignPlayerData notAssignPlayer,
		int maxImpNum,
		int curCrewUseNum,
		int curImpUseNum,
		int reduceCrewmateRoleNum,
		int reduceImpostorRoleNum,
		int reduceNeutralRoleNum,
		int reduceLiberalRoleNum,
		bool isMultiAssign)
	{
		int crewNotAssignPlayerNum = isMultiAssign ?
			notAssignPlayer.CrewmateMultiAssignPlayerNum :
			notAssignPlayer.CrewmateSingleAssignPlayerNum;
		int impNotAssignPlayerNum = isMultiAssign ?
			notAssignPlayer.ImpostorMultiAssignPlayerNum :
			notAssignPlayer.ImpostorSingleAssignPlayerNum;

		int totalReduceCrewmateNum = reduceCrewmateRoleNum + reduceNeutralRoleNum + reduceLiberalRoleNum;

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
				limiter.CanSpawn(ExtremeRoleType.Crewmate, reduceCrewmateRoleNum) &&
				isLimitCrewAssignNum
			)
			// ニュートラルのスポーン上限チェック
			&&
			(
				limiter.CanSpawn(ExtremeRoleType.Neutral, reduceNeutralRoleNum) &&
				isLimitCrewAssignNum
			)
			&&
			(
				limiter.CanSpawn(ExtremeRoleType.Liberal, reduceLiberalRoleNum) &&
				isLimitCrewAssignNum
			)
			// インポスターのスポーン上限チェック
			&&
			(
				limiter.CanSpawn(ExtremeRoleType.Impostor, reduceImpostorRoleNum) &&
				isLimitImpAssignNum
			);
	}
	#endregion
}
