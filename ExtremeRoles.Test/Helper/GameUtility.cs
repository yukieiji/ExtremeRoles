using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using BepInEx.Logging;
using AmongUs.GameOptions;

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

	public static void ChangePresetTo(int newPreset)
	{
		OptionManager.Instance.GetIOption(0).UpdateSelection(newPreset);
		OptionManager.Instance.SwitchPreset(newPreset);
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
		foreach (var opt in OptionManager.Instance.GetAllIOption())
		{
			if (opt.Id == 0) { continue; }

			int newIndex = RandomGenerator.Instance.Next(0, opt.ValueCount);
			string name = opt.Name;

			if (name.Contains(RoleCommonOption.AssignWeight.ToString()))
			{
				newIndex = 5;
			}
			else if (name.Contains(RoleCommonOption.SpawnRate.ToString()))
			{
				newIndex = 0;
			}
			opt.UpdateSelection(newIndex);
		}

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
		foreach (var opt in OptionManager.Instance.GetAllIOption())
		{
			if (opt.Id == 0) { continue; }

			int length = opt.ValueCount;
			int newIndex = RandomGenerator.Instance.Next(0, length);
			string name = opt.Name;

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
			opt.UpdateSelection(newIndex);
		}

		logger.LogInfo("Update Player....");
		for (int playerId = 0; playerId < 14; ++playerId)
		{
			string playerName = $"TestPlayer_{playerId}";
			logger.LogInfo($"Spawn : {playerName}");

			GameSystem.SpawnDummyPlayer(playerName);
		}
	}

	public static void UpdateExROption(in RequireOption<int, int> option)
	{
		OptionManager.Instance.GetIOption(
			option.OptionId).UpdateSelection(
				option.Velue);
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


	private static void enableXion()
	{
		OptionManager.Instance.GetIOption(
			(int)RoleGlobalOption.UseXion).UpdateSelection(1);
	}

	private static void enableRandomNormalRole(ManualLogSource logger)
	{
		SingleRoleBase role = RandomRoleProvider.GetNormalRole();
		int optionId = role.GetRoleOptionId(RoleCommonOption.SpawnRate);
		OptionManager.Instance.GetIOption(optionId).UpdateSelection(
			OptionCreator.SpawnRate.Length - 1);
		logger.LogInfo($"Enable:{role.Id}");
	}
	private static void enableRandomCombRole(ManualLogSource logger)
	{
		CombinationRoleManagerBase role = RandomRoleProvider.GetCombRole();
		int optionId = role.GetRoleOptionId(RoleCommonOption.SpawnRate);
		OptionManager.Instance.GetIOption(optionId).UpdateSelection(1);
		logger.LogInfo($"Enable:{role}");
	}
}
