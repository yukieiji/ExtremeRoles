using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

using ExtremeRoles.Test.Helper;

using UnityResource = UnityEngine.Resources;
using Microsoft.Extensions.DependencyInjection;

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

			if (testCase.PreTestCase is not null)
			{
				yield return new WaitForSeconds(2.5f);
				yield return testCase.PreTestCase.Invoke(this.Log);
			}

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
			yield return GameUtility.ReturnLobby(this.Log);
		}
	}
}
