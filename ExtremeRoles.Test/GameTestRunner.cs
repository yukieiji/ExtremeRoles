using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;


using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Logging;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Test.Helper;

namespace ExtremeRoles.Test;

public class GameTestRunner : TestRunnerBase
{
	public sealed record TestCase(string Name, int Iteration, HashSet<ExtremeRoleId>? Ids);

	public override void Run()
	{
		GameMudderEndTestingBehaviour.Instance.Logger = this.Log;
		GameMudderEndTestingBehaviour.Instance.StartCoroutine(
			GameMudderEndTestingBehaviour.Instance.Run(
				new("Random", 3, null),
				new("NeutralRemove", 5, new HashSet<ExtremeRoleId>()
				{
					ExtremeRoleId.Jester, ExtremeRoleId.TaskMaster,
					ExtremeRoleId.Neet, ExtremeRoleId.Umbrer,
					ExtremeRoleId.Madmate
				}),
				new("YokoWin", 100, new HashSet<ExtremeRoleId>() { ExtremeRoleId.Yoko }),
				new("QueenWin", 100, new HashSet<ExtremeRoleId>() { ExtremeRoleId.Queen }),
				new("YandereWin", 100, new HashSet<ExtremeRoleId>() { ExtremeRoleId.Yandere })));
	}
}

[Il2CppRegister]
public sealed class GameMudderEndTestingBehaviour : MonoBehaviour
{
	public static GameMudderEndTestingBehaviour Instance
	{
		get
		{
			if (instance == null)
			{
				instance = ExtremeRolesTestPlugin.Instance.AddComponent<GameMudderEndTestingBehaviour>();
			}
			return instance;
		}
	}
	private static GameMudderEndTestingBehaviour? instance;

	public ManualLogSource Logger { private get; set; }
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
	public GameMudderEndTestingBehaviour(IntPtr ptr) : base(ptr) { }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

	[HideFromIl2Cpp]
	public IEnumerator Run(params GameTestRunner.TestCase[] testCases)
	{
		foreach (var testCase in testCases)
		{
			yield return runTestCase(testCase);
		}
	}

	[HideFromIl2Cpp]
	private IEnumerator runTestCase(GameTestRunner.TestCase testCase)
	{
		for (int i = 0; i < testCase.Iteration; ++i)
		{
			this.Logger.LogInfo($"{testCase.GetType().Name}.{testCase.Name} - Start iteration:{i}");
			if (testCase.Ids is null)
			{
				GameUtility.PrepereGameWithRandom(this.Logger);
			}
			else
			{
				GameUtility.PrepereGameWithRole(this.Logger, testCase.Ids);
			}

			yield return GameUtility.StartGame(this.Logger);

			while (GameUtility.IsContinue)
			{
				var player = CachedPlayerControl.AllPlayerControls.OrderBy(x => RandomGenerator.Instance.Next()).First();
				if (!player.Data.IsDead || ExtremeRoleManager.GameRole[player.PlayerId].Id != ExtremeRoleId.Assassin)
				{
					Player.RpcUncheckMurderPlayer(player.PlayerId, player.PlayerId, byte.MinValue);
					yield return new WaitForSeconds(1.0f);
				}
			}
			yield return GameUtility.ReturnLobby(this.Logger);
		}
	}
}
