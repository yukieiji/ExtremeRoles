using ExtremeRoles.Performance;

using UnityEngine;

#nullable enable

namespace ExtremeRoles.Extension.Task;

public static class MiniGameExtension
{
	// 普通に呼び出すと再帰エラーが出るのでこれで呼び出す
	public static void AbstractBegin(this Minigame game, PlayerTask? task)
	{
		Minigame.Instance = game;

		game.MyTask = task;
		game.MyNormTask = task as NormalPlayerTask;
		game.timeOpened = Time.realtimeSinceStartup;

		PlayerControl? localPlayer = CachedPlayerControl.LocalPlayer;

		if (localPlayer != null)
		{
			if (MapBehaviour.Instance)
			{
				MapBehaviour.Instance.Close();
			}

			localPlayer.NetTransform.Halt();
			FastDestroyableSingleton<DebugAnalytics>.Instance.Analytics.MinigameOpened(
				localPlayer.Data, game.TaskType);
		}

		ExtremeRolesPlugin.Logger.LogInfo($"Opening ExR minigame {game.GetType().Name}");
		game.StartCoroutine(game.CoAnimateOpen());
	}

	public static void AbstractClose(this Minigame game)
	{
		if (game.amClosing != Minigame.CloseState.Closing)
		{
			if (game.CloseSound != null &&
				Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(game.CloseSound, false, 1f, null);
			}

			PlayerControl? localPlayer = CachedPlayerControl.LocalPlayer;

			if (localPlayer != null)
			{
				if (localPlayer.Data.Role.TeamType == RoleTeamTypes.Crewmate)
				{
					GameManager.Instance.LogicMinigame.OnMinigameClose();
				}
				PlayerControl.HideCursorTemporarily();
			}

			game.amClosing = Minigame.CloseState.Closing;

			ExtremeRolesPlugin.Logger.LogInfo($"Closing ExR minigame {game.GetType().Name}");

			IAnalyticsReporter analytics = FastDestroyableSingleton<DebugAnalytics>.Instance.Analytics;
			NetworkedPlayerInfo data = PlayerControl.LocalPlayer.Data;
			TaskTypes taskType = game.TaskType;
			float num = Time.realtimeSinceStartup - game.timeOpened;
			PlayerTask myTask = game.MyTask;

			analytics.MinigameClosed(data, taskType, num, myTask != null && myTask.IsComplete);

			game.StartCoroutine(game.CoDestroySelf());

			return;
		}
		Object.Destroy(game.gameObject);
	}
}
