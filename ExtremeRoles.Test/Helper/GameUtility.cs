using BepInEx.Logging;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtremeRoles.Test.Helper;

public static class GameUtility
{
	public static bool IsContinue =>
		GameManager.Instance != null &&
		GameManager.Instance.ShouldCheckForGameEnd;

	public static IEnumerator StartGameWithRandom(ManualLogSource logger)
	{
		PrepereGameWithRandom(logger);
		yield return new WaitForSeconds(2.0f);

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
		logger.LogInfo($"WinText : {nav.EndGameManager.WinText.text}");
		nav.NextGame();

		yield return new WaitForSeconds(10.0f);
	}

	public static void PrepereGameWithRandom(ManualLogSource logger)
	{
		logger.LogInfo("Update Option....");
		// オプションを適当にアプデ
		foreach (var opt in OptionManager.Instance.GetAllIOption())
		{
			int newIndex = RandomGenerator.Instance.Next(0, opt.ValueCount);
			if (opt.Name.Contains("AssignWeight"))
			{
				newIndex = 5;
			}
			else if (opt.Name.Contains(RoleCommonOption.SpawnRate.ToString()))
			{
				newIndex = 0;
			}
			opt.UpdateSelection(newIndex);
		}

		enableXion();

		for (int playerId = 0; playerId < 15; ++playerId)
		{
			string playerName = $"TestPlayer_{playerId}";
			logger.LogInfo($"spawn : {playerName}");

			GameSystem.SpawnDummyPlayer(playerName);

			enableRandomNormalRole(logger);
		}

		enableRandomCombRole(logger);
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
