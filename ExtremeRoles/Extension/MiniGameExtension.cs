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

		game.StartCoroutine(game.CoAnimateOpen());
	}
}
