using ExtremeRoles.Helper;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Test.Helper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ExtremeRoles.Module.GameResult.LiberalMoneyHistory;
using UnityResource = UnityEngine.Resources;

namespace ExtremeRoles.Test.InGame.GameLoop;

public class GameLoopTestStep(GameLoopTestCaseFactory factory) : TestStepBase
{
	private readonly GameLoopTestCase[] testCase = factory.Get();

	private int count = 0;
	private const int waitCount = 5;

	public static void Register(IServiceCollection services)
	{
		services.AddTransient<GameLoopTestCaseFactory>();
	}

	public override IEnumerator Run()
	{
		foreach (var @case in testCase)
		{
			yield return runTestCase(@case);
		}
	}

	private IEnumerator runTestCase(GameLoopTestCase testCase)
	{
		for (int i = 0; i < testCase.Iteration; ++i)
		{
			++this.count;

			this.Log.LogInfo($"{testCase.GetType().Name}.{testCase.Name} - Start iteration:{i}");
			if (testCase.Ids is null)
			{
				GameUtility.PrepareGameWithRandom(this.Log);
			}
			else
			{
				GameUtility.PrepereGameWithRole(this.Log, testCase.Ids);
			}

			testCase.PreSetUp?.Invoke();

			if (this.count > waitCount)
			{
				this.Log.LogInfo("Wait for 10s");
				GC.Collect();
				Resources.UnityObjectLoader.ResetCache();
				yield return UnityResource.UnloadUnusedAssets();
				yield return new WaitForSeconds(10.0f);
				this.count = 0;
			}

			yield return GameUtility.StartGame(this.Log);

			while (IntroCutscene.Instance != null)
			{
				yield return new WaitForSeconds(10.0f);
			}

			if (!GameProgressSystem.IsRoleSetUpEnd)
			{
				yield return new WaitForSeconds(1.0f);
			}

			if (testCase.PreTestCase is not null)
			{
				yield return new WaitForSeconds(2.5f);
				yield return testCase.PreTestCase.Invoke(this.Log);
			}

			yield return new WaitForSeconds(1.0f);

			var randomPlayer = new Queue<PlayerControl>(PlayerCache.AllPlayerControl.OrderBy(x => RandomGenerator.Instance.Next()).ToArray());

			while (GameUtility.IsContinue)
			{
				if (!randomPlayer.TryDequeue(out var player))
				{
					// 何故かゲームが終了しない場合があるので、強制的に終了させる
					ShipStatus.Instance.enabled = false;
					GameManager.Instance.RpcEndGame(GameOverReason.CrewmateDisconnect, false);
					GameProgressSystem.Current = GameProgressSystem.Progress.None;

					yield return new WaitForSeconds(1.0f);
					break;
				}

				if (!ExtremeRoleManager.TryGetRole(player.PlayerId, out var role) ||
					player.Data.IsDead ||
					role.Core.Id is (ExtremeRoleId.Assassin or ExtremeRoleId.Leader))
				{
					continue;
				}

				this.Log.LogInfo($"Killed : {player.PlayerId}");
				Player.RpcUncheckMurderPlayer(player.PlayerId, player.PlayerId, byte.MinValue);
				yield return new WaitForSeconds(1.0f);
			}
			yield return GameUtility.ReturnLobby(this.Log);
		}
	}
}
