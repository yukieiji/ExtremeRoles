using System;
using System.Linq;

using AmongUs.GameOptions;

using Microsoft.Extensions.DependencyInjection;
using UnityEngine;


using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Test.Helper;

using UnityResource = UnityEngine.Resources;

namespace ExtremeRoles.Test.InGame.InfinityTaskLoop;

public sealed class InfinityTaskLoopTestStep() : TestStepBase
{
	private int count = 0;
	private const int waitCount = 5;

#if DEBUG
	private const int taskNum = 256;
#else
	private const int taskNum = 10;
#endif
#if DEBUG
	private const int repeatNum = 10;
#else
	private const int repeatNum = 1;
#endif

	public static void Register(IServiceCollection services)
	{
		services.AddTransient<InfinityTaskLoopTestStep>();
	}

	public override IEnumerator Run()
	{
		for (int i= 0 ; i < repeatNum; ++i)
		{
			yield return runTestCase();
		}
	}

	private IEnumerator runTestCase()
	{
		byte[] mapId = [0, 1, 2, 4, 5];
		foreach (byte id in mapId)
		{
			bool noTask = false;
			Log.LogInfo($"Start map task Check : {id}");

			do
			{
				++count;

				GameUtility.PrepareGameWithRandomAndNoNeutral(Log);
				
				yield return new WaitForSeconds(1.0f);

				GameOptionsManager.Instance.CurrentGameOptions.SetByte(ByteOptionNames.MapId, id);

				if (this.count > waitCount)
				{
					Log.LogInfo("Wait for 10s");
					GC.Collect();
					Resources.UnityObjectLoader.ResetCache();
					yield return UnityResource.UnloadUnusedAssets();
					yield return new WaitForSeconds(10.0f);
					this.count = 0;
				}

				yield return GameUtility.StartGame(Log);

				while (IntroCutscene.Instance != null)
				{
					yield return new WaitForSeconds(10.0f);
				}

				noTask = !(ExtremeRoleManager.TryGetRole(PlayerControl.LocalPlayer.PlayerId, out var role) && role.IsCrewmate());

				if (noTask)
				{
					yield return runNormal();
				}

			} while (noTask);


			for (int j = 0; j < taskNum; ++j)
			{
				yield return taskRun();
			}
			yield return runNormal();
		}
	}

	private IEnumerator runNormal()
	{
		while (GameUtility.IsContinue)
		{
			var player = PlayerCache.AllPlayerControl.OrderBy(x => RandomGenerator.Instance.Next()).First();
			if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
				player.Data.IsDead ||
				role.Id is ExtremeRoleId.Assassin)
			{
				continue;
			}

			Player.RpcUncheckMurderPlayer(player.PlayerId, player.PlayerId, byte.MinValue);
			yield return new WaitForSeconds(1.0f);
		}
		yield return GameUtility.ReturnLobby(Log);
	}

	private IEnumerator taskRun()
	{
		var localPc = PlayerControl.LocalPlayer;

		NormalPlayerTask? targetTask = null;
		do
		{
			var target = localPc.myTasks.GetFastEnumerator().OrderBy(
				x => RandomGenerator.Instance.Next()).First();
			if (GameSystem.IgnoreTask.Contains(target.TaskType))
			{
				continue;
			}
			targetTask = target.TryCast<NormalPlayerTask>();

		} while (targetTask == null);

		
		Console? targetConsole = null;
		foreach (var console in ShipStatus.Instance.AllConsoles)
		{
			var task = console.FindTask(localPc);
			if (task == null || task.TaskType != targetTask.TaskType)
			{
				continue;
			}
			targetConsole = console;
		}

		if (targetConsole == null)
		{
			yield return null;
			yield break;
		}

		localPc.NetTransform.SnapTo(targetConsole.transform.position);

		int waitCount = 0;
		do
		{
			HudManager.Instance.UseButton.DoClick();
			yield return new WaitForSeconds(1.0f);
			if (waitCount == 30)
			{
				Log.LogFatal($"Task : {targetTask.TaskType} can't start");
				yield break;
			}
			waitCount++;
		} while (Minigame.Instance == null);

		Log.LogInfo($"Task : {targetTask.TaskType} start");

		if (Minigame.Instance == null)
		{
			yield break;
		}
		Minigame.Instance.Close();
		
		int taskId = targetTask.Index;
		uint id = targetTask.Id;
		targetTask.Complete();
		targetTask.NextStep();

		yield return new WaitForSeconds(1.0f);

		int taskIndex;

		if (ShipStatus.Instance.CommonTasks.Any(
			(NormalPlayerTask t) => t.Index == taskId))
		{
			taskIndex = GameSystem.GetRandomShortTaskId();
		}
		else if (ShipStatus.Instance.LongTasks.Any(
			(NormalPlayerTask t) => t.Index == taskId))
		{
			taskIndex = GameSystem.GetRandomLongTask();
		}
		else if (ShipStatus.Instance.ShortTasks.Any(
			(NormalPlayerTask t) => t.Index == taskId))
		{
			taskIndex = GameSystem.GetRandomCommonTaskId();
		}
		else
		{
			yield break;
		}

		GameSystem.RpcReplaceNewTask(localPc.PlayerId, (int)id, taskIndex);
	}
}
