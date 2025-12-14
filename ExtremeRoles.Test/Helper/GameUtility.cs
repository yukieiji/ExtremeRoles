using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;
using BepInEx.Logging;

using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;

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

	public static IEnumerator WaitForStabilize()
	{
		var waiter = new WaitForSeconds(2.5f);
		while (
			AmongUsClient.Instance == null ||
			AmongUsClient.Instance.Ping > 100)
		{
			yield return waiter;
		}
		yield return new WaitForSeconds(10.0f);
	}

	public static void ChangePresetTo(int newPreset)
	{
		var mng = OptionManager.Instance;
		if (!mng.TryGetCategory(OptionTab.GeneralTab, (int)OptionCreator.CommonOption.PresetOption, out var presetCate))
		{
			return;
		}
		var option = presetCate.Get(0);
		mng.Update(presetCate, option, newPreset);
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
		var waitor = new WaitForSeconds(2.5f);
		yield return waitor;

		logger.LogInfo("Back to Lobby");
		GameObject navObj = GameObject.Find("EndGameNavigation");
		EndGameNavigation nav = navObj.GetComponent<EndGameNavigation>();
		yield return waitor;
		nav.NextGame();

		yield return new WaitForSeconds(10.0f);
	}

	public static void PrepareGameWithRandom(ManualLogSource logger)
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

		if (RandomGenerator.Instance.Next(2) == 0)
		{
			disableLiberal();
		}

		disableXion();
		disableSomeRole();

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

	public static void PrepareGameWithRandomAndNoNeutral(ManualLogSource logger)
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

		if (RandomGenerator.Instance.Next(2) == 0)
		{
			disableLiberal();
		}

		disableXion();
		disableSomeRole();

		logger.LogInfo("Update Roles and Player....");

		for (int playerId = 0; playerId < 14; ++playerId)
		{
			string playerName = $"TestPlayer_{playerId}";
			logger.LogInfo($"spawn : {playerName}");

			GameSystem.SpawnDummyPlayer(playerName);

			enableRandomNormalRole(logger);
		}

		disableNeutralRole();
		disableCombRole();
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
				if (cate.Id == 0)
				{ 
					continue;
				}

				foreach (var opt in cate.Options)
				{
					int length = opt.Range;
					if (length == 0)
					{
						continue;
					}
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

		disableCategory(OptionTab.GeneralTab, (int)ShipGlobalOptionCategory.RandomMapOption);
		if (RandomGenerator.Instance.Next(2) == 0)
		{
			disableLiberal();
		}
		disableXion();
		disableSomeRole();

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

	private static void disableLiberal()
	{
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.GeneralTab,
				(int)SpawnOptionCategory.RoleSpawnCategory,
				out var category))
		{
			OptionManager.Instance.Update(category, (int)RoleSpawnOption.MinLiberal, 0);
			OptionManager.Instance.Update(category, (int)RoleSpawnOption.MaxLiberal, 0);
		}
	}


	private static void disableXion()
	{
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.GeneralTab,
				ExtremeRoleManager.GetRoleGroupId(ExtremeRoleId.Xion),
				out var category))
		{
			OptionManager.Instance.Update(category, 0, 0);
		}
	}

	private static void disableSomeRole()
	{
		foreach (var role in RandomRoleProvider.IgnoreRole)
		{
			disableCategory(
				OptionTab.NeutralTab,
				ExtremeRoleManager.GetRoleGroupId(role));
		}
		foreach (var role in RandomRoleProvider.IgnoreCombRole)
		{
			disableCategory(
				OptionTab.GeneralTab,
				ExtremeRoleManager.GetCombRoleGroupId(CombinationRoleType.Avalon));
		}
	}

	private static void enableRandomNormalRole(ManualLogSource logger)
	{
		SingleRoleBase role = RandomRoleProvider.GetNormalRole();

		if (OptionManager.Instance.TryGetCategory(
				role.Tab,
				ExtremeRoleManager.GetRoleGroupId(role.Core.Id),
				out var category))
		{
			OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 9);
		}
		logger.LogInfo($"Enable:{role.Core.Id}");
	}
	private static void enableRandomCombRole(ManualLogSource logger)
	{
		CombinationRoleType role = (CombinationRoleType)RandomRoleProvider.GetCombRole();
		if (OptionManager.Instance.TryGetCategory(
				OptionTab.CombinationTab,
				ExtremeRoleManager.GetCombRoleGroupId(role),
				out var category))
		{
			OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 9);
		}
		logger.LogInfo($"Enable:{role}");
	}

	private static void disableCombRole()
	{
		foreach (byte id in RandomRoleProvider.AllCombRole())
		{
			if (OptionManager.Instance.TryGetCategory(
					OptionTab.CombinationTab,
					ExtremeRoleManager.GetCombRoleGroupId((CombinationRoleType)id),
					out var category))
			{
				OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 0);
			}
		}
	}

	private static void disableNeutralRole()
	{
		foreach (var id in RandomRoleProvider.AllNeutral())
		{
			if (OptionManager.Instance.TryGetCategory(
					OptionTab.NeutralTab,
					ExtremeRoleManager.GetRoleGroupId(id),
					out var category))
			{
				OptionManager.Instance.Update(category, (int)RoleCommonOption.SpawnRate, 0);
			}
		}
	}

	private static void disableCategory(OptionTab tab, int id)
	{
		if (OptionManager.Instance.TryGetCategory(tab, id, out var category))
		{
			OptionManager.Instance.Update(category, 0, 0);
		}
	}
}
