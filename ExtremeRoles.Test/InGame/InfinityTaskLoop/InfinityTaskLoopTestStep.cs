using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Test.Helper;
using Microsoft.Extensions.DependencyInjection;
using System;


using System.Linq;
using UnityEngine;

using UnityResource = UnityEngine.Resources;

namespace ExtremeRoles.Test.InGame.InfinityTaskLoop;

public class InfinityTaskLoopTestStep() : TestStepBase
{
	private int count = 0;
	private const int waitCount = 5;

	public static void Register(IServiceCollection services)
	{
		services.AddTransient<InfinityTaskLoopTestStep>();
	}

	public override IEnumerator Run()
	{
		for (int i= 0 ; i < 10; ++i)
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

				GameUtility.PrepereGameWithRandomAndNoNeutral(Log);
				
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


			for (int j = 0; j < 100; ++j)
			{
				yield return taskRun();
			}

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

		int waitCount = -1;
		do
		{
			HudManager.Instance.UseButton.DoClick();
			waitCount++;
			yield return new WaitForSeconds(1.0f);
			if (waitCount == 30)
			{
				Log.LogFatal($"Task : {targetTask.TaskType} can't start");
				yield break;
			}
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

		if (ShipStatus.Instance.CommonTasks.FirstOrDefault(
			(NormalPlayerTask t) => t.Index == taskId) != null)
		{
			taskIndex = GameSystem.GetRandomShortTaskId();
		}
		else if (ShipStatus.Instance.LongTasks.FirstOrDefault(
			(NormalPlayerTask t) => t.Index == taskId) != null)
		{
			taskIndex = GameSystem.GetRandomLongTask();
		}
		else if (ShipStatus.Instance.ShortTasks.FirstOrDefault(
			(NormalPlayerTask t) => t.Index == taskId) != null)
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
