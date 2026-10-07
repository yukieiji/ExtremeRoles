using System.Collections.Generic;

using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Solo.Crewmate;
using ExtremeRoles.Roles.Solo.Neutral.Jackal;

namespace ExtremeRoles.Roles.Combination;

public sealed class GuesserNormalRoleProvider : IGuesserNormalRoleProvider
{
	public void AddExRNormalRoles(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		out GuesserNormalRoleAssignState assignState)
	{
		assignState = new GuesserNormalRoleAssignState();

		foreach (var (id, role) in ExtremeRoleManager.NormalRole)
		{
			var loader = role.Loader;

			int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.SpawnRate);
			int roleNum = loader.GetValue<RoleCommonOption, int>(
				RoleCommonOption.RoleNum);

			if (spawnOptSel < 1 || roleNum <= 0)
			{
				continue;
			}

			ExtremeRoleId exId = (ExtremeRoleId)id;
			ExtremeRoleType team = role.Core.Team;

			// クイーンとサーヴァントとジャッカルとサイドキックはニュートラルの最後に追加する(役職のパターンがいくつかあるため)
			if (exId != ExtremeRoleId.Queen &&
				exId != ExtremeRoleId.Jackal)
			{
				add(result, exId, team);
				separatedRoleId[team].Add(exId);
			}
			switch (exId)
			{
				case ExtremeRoleId.Jackal:
					assignState.IsJackalOn = true;
					assignState.IsJackalForceReplaceLover = OptionManager.Instance.TryGetCategory(
						OptionTab.NeutralTab,
						ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jackal),
						out var cate) && cate.GetValue<JackalRole.JackalOption, bool>(JackalRole.JackalOption.ForceReplaceLover);
					break;
				case ExtremeRoleId.Queen:
					assignState.IsQueenOn = true;
					break;
				case ExtremeRoleId.Hypnotist:
					// 本来はニュートラルであるがソート用にインポスターとして突っ込む
					add(result, ExtremeRoleId.Doll, ExtremeRoleType.Impostor);
					separatedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Doll);
					break;
				case ExtremeRoleId.Jailer:
					add(result, ExtremeRoleId.Yardbird, ExtremeRoleType.Crewmate);
					if (OptionManager.Instance.TryGetCategory(
							OptionTab.CrewmateTab,
							ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Jailer),
							out var jailer) &&
						!jailer.GetValue<Jailer.Option, bool>(Jailer.Option.IsMissingToDead))
					{
						add(result, ExtremeRoleId.Lawbreaker, ExtremeRoleType.Neutral);
					}
					break;
				case ExtremeRoleId.Tucker:
					add(result, ExtremeRoleId.Chimera, ExtremeRoleType.Neutral);
					separatedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Chimera);
					break;
				default:
					break;
			}
		}

		// ジャッカルとサイドキック、サイドキック + ラバーズの追加
		if (assignState.IsJackalOn)
		{
			add(result, ExtremeRoleId.Jackal, ExtremeRoleType.Neutral);
			add(result, ExtremeRoleId.Sidekick, ExtremeRoleType.Neutral);

			separatedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Jackal);
			separatedRoleId[ExtremeRoleType.Neutral].Add(ExtremeRoleId.Sidekick);
		}

		// クイーンとサーヴァント、サーヴァント + 〇〇、〇〇 + サーヴァントの追加
		if (assignState.IsQueenOn)
		{
			ExtremeRoleType queenTeam = ExtremeRoleType.Neutral;
			add(result, ExtremeRoleId.Queen, queenTeam);

			ExtremeRoleId servantId = ExtremeRoleId.Servant;
			if (separatedRoleId[queenTeam].Count > 1)
			{
				add(result, servantId, queenTeam);
			}

			listAddTargetTeam(result, separatedRoleId, servantId, queenTeam, ExtremeRoleType.Crewmate);
			listAddTargetTeam(result, separatedRoleId, servantId, queenTeam, ExtremeRoleType.Impostor);

			foreach (var (id, roleMng) in ExtremeRoleManager.CombRole)
			{
				var loader = roleMng.Loader;

				int spawnOptSel = loader.GetValue<RoleCommonOption, int>(
					RoleCommonOption.SpawnRate);
				int roleNum = loader.GetValue<RoleCommonOption, int>(
					RoleCommonOption.RoleNum);

				if (spawnOptSel < 1 || roleNum <= 0)
				{
					continue;
				}
				if (roleMng is FlexibleCombinationRoleManagerBase flexMng)
				{
					add(result, flexMng.BaseRole.Core.Id, queenTeam, servantId);
				}
				else
				{
					foreach (var role in roleMng.Roles)
					{
						if (role.IsNeutral())
						{
							continue;
						}

						add(result, role.Core.Id, queenTeam, servantId);
					}
				}
				// 見習い捜査官の追加
				if (isInvestigatorOffice(id))
				{
					add(result, ExtremeRoleId.InvestigatorApprentice, queenTeam, servantId);
				}
			}

			separatedRoleId[queenTeam].Add(ExtremeRoleId.Queen);
			separatedRoleId[queenTeam].Add(servantId);
		}
	}

	private static bool isInvestigatorOffice(byte checkId)
		=> checkId == (byte)CombinationRoleType.InvestigatorOffice;

	private static void add(
		List<GuessBehaviour.RoleInfo> result,
		ExtremeRoleId id,
		ExtremeRoleType team,
		ExtremeRoleId another = ExtremeRoleId.Null)
	{
		result.Add(
			new GuessBehaviour.RoleInfo()
			{
				Id = id,
				AnothorId = another,
				Team = team,
			});
	}

	private static void listAdd(
		List<GuessBehaviour.RoleInfo> result,
		ExtremeRoleId baseId,
		ExtremeRoleType team,
		List<ExtremeRoleId> list)
	{
		foreach (var roleId in list)
		{
			add(result, baseId, team, roleId);
		}
	}

	private static void listAddTargetTeam(
		List<GuessBehaviour.RoleInfo> result,
		Dictionary<ExtremeRoleType, List<ExtremeRoleId>> separatedRoleId,
		ExtremeRoleId baseId,
		ExtremeRoleType team,
		ExtremeRoleType targetType)
	{
		listAdd(result, baseId, team, separatedRoleId[targetType]);
	}
}
