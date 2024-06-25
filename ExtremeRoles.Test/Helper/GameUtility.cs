using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Test.Helper;

public sealed record RequireOption<T, W>(T OptionId, W Velue)
	where T : struct, IConvertible
	where W :
		struct, IComparable, IConvertible,
		IComparable<W>, IEquatable<W>;

public static class GameUtility
{
	public static bool IsContinue =>
		GameManager.Instance != null &&
		GameManager.Instance.ShouldCheckForGameEnd;

	public static void ChangePresetTo(int newPreset)
	{
		var mng = OptionManager.Instance;
		if (!mng.TryGetCategory(OptionTab.General, (int)OptionCreator.CommonOption.PresetOption, out var presetCate))
		{
			return;
		}
		var option = presetCate.Get(0);
		mng.UpdateToStep(presetCate, option, newPreset);
	}

	public static IEnumerator StartGame(ManualLogSource logger)
	{
		yield return new WaitForSeconds(5.0f);

		logger.LogInfo("Start Games....");
		GameStartManager.Instance.BeginGame();

		yield return new WaitForSeconds(10.0f);
		while (IntroCutscene.Instance)
		{
			yield return null;
		}
		yield return new WaitForSeconds(20.0f);
	}

	public static IEnumerator ReturnLobby(ManualLogSource logger)
	{
		yield return new WaitForSeconds(2.5f);

		logger.LogInfo("Back to Lobby");
		GameObject navObj = GameObject.Find("EndGameNavigation");
		EndGameNavigation nav = navObj.GetComponent<EndGameNavigation>();
		nav.NextGame();

		yield return new WaitForSeconds(10.0f);
	}

	public static void PrepereGameWithRandom(ManualLogSource logger)
	{
		logger.LogInfo("Update Option....");
		// オプションを適当にアプデ
		var mng = OptionManager.Instance;
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			if (!mng.TryGetTab(tab, out var tabObj))
			{
				continue;
			}

			foreach (var cate in tabObj.Category)
			{
				if (cate.Id == 0) { continue; }

				foreach (var opt in cate.Options)
				{
					int newIndex = RandomGenerator.Instance.Next(0, opt.Range);
					string name = opt.Info.Name;

					if (name.Contains(RoleCommonOption.AssignWeight.ToString()))
					{
						newIndex = 5;
					}
					else if (name.Contains(RoleCommonOption.SpawnRate.ToString()))
					{
						newIndex = 0;
					}
					mng.Update(cate, opt, newIndex);
				}
			}
		}

		disableXion();

		logger.LogInfo("Update Roles and Player....");

		for (int playerId = 0; playerId < 14; ++playerId)
		{
			string playerName = $"TestPlayer_{playerId}";
			logger.LogInfo($"spawn : {playerName}");

			GameSystem.SpawnDummyPlayer(playerName);

			enableRandomNormalRole(logger);
		}

		enableRandomCombRole(logger);
	}

	public static void PrepereGameWithRole(ManualLogSource logger, HashSet<ExtremeRoleId> ids)
	{
		logger.LogInfo("Update Option....");
		// オプションを適当にアプデ

		var mng = OptionManager.Instance;
		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			if (!mng.TryGetTab(tab, out var tabObj))
			{
				continue;
			}

			foreach (var cate in tabObj.Category)
			{
				if (cate.Id == 0) { continue; }

				foreach (var opt in cate.Options)
				{
					int length = opt.Range;
					int newIndex = RandomGenerator.Instance.Next(0, length);
					string name = opt.Info.Name;

					if (
						ids.Any(x => name.Contains(x.ToString())) &&
						(
							name.Contains(RoleCommonOption.SpawnRate.ToString()) ||
							name.Contains(RoleCommonOption.AssignWeight.ToString())
						))
					{
						newIndex = length - 1;
					}
					else if (
						ids.Any(x => name.Contains(x.ToString())) &&
						name.Contains(RoleCommonOption.RoleNum.ToString()))
					{
						newIndex = RandomGenerator.Instance.Next(1, ((15 - 3) / ids.Count));
					}
					else if (name.Contains(RoleCommonOption.AssignWeight.ToString()))
					{
						newIndex = 5;
					}
					else if (name.Contains(RoleCommonOption.SpawnRate.ToString()))
					{
						newIndex = 0;
					}
					mng.Update(cate, opt, newIndex);
				}
			}
		}

		disableXion();

		logger.LogInfo("Update Player....");
		for (int playerId = 0; playerId < 14; ++playerId)
		{
			string playerName = $"TestPlayer_{playerId}";
			logger.LogInfo($"Spawn : {playerName}");

			GameSystem.SpawnDummyPlayer(playerName);
		}
	}

	public static void UpdateExROption(OptionTab tab, int categoryId, in RequireOption<int, int> option)
	{
		if (OptionManager.Instance.TryGetCategory(tab, categoryId, out var category))
		{
			OptionManager.Instance.Update(category, option.OptionId, option.Velue);
		}
	}

	public static void UpdateAmongUsOption(in RequireOption<BoolOptionNames, bool> option)
	{
		GameOptionsManager.Instance.currentGameOptions.SetBool(option.OptionId, option.Velue);
	}
	public static void UpdateAmongUsOption(in RequireOption<Int32OptionNames, int> option)
	{
		GameOptionsManager.Instance.currentGameOptions.SetInt(option.OptionId, option.Velue);
	}
	public static void UpdateAmongUsOption(in RequireOption<UInt32OptionNames, uint> option)
	{
		GameOptionsManager.Instance.currentGameOptions.SetUInt(option.OptionId, option.Velue);
	}
	public static void UpdateAmongUsOption(in RequireOption<ByteOptionNames, byte> option)
	{
		GameOptionsManager.Instance.currentGameOptions.SetByte(option.OptionId, option.Velue);
	}
	public static void UpdateAmongUsOption(in RequireOption<FloatOptionNames, float> option)
	{
		GameOptionsManager.Instance.currentGameOptions.SetFloat(option.OptionId, option.Velue);
	}


	private static void disableXion()
	{
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.General,
				ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
				out var category))
		{
			OptionManager.Instance.Update(category, 0, 0);
		}
	}

	private static void enableRandomNormalRole(ManualLogSource logger)
	{
		SingleRoleBase role = RandomRoleProvider.GetNormalRole();

		if (OptionManager.Instance.TryGetCategory(
				role.Tab,
				ExtremeRoleManager.GetRoleGroupId(role.Id),
				out var category))
		{
			OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 9);
		}
		logger.LogInfo($"Enable:{role.Id}");
	}
	private static void enableRandomCombRole(ManualLogSource logger)
	{
		CombinationRoleType role = (CombinationRoleType)RandomRoleProvider.GetCombRole();
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.Combination,
				ExtremeRoleManager.GetCombRoleGroupId(role),
				out var category))
		{
			OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 9);
		}
		logger.LogInfo($"Enable:{role}");
	}
}
