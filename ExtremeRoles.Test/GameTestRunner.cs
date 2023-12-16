using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Test.Helper;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Logging;
using System.Collections.Generic;
using ExtremeRoles.Roles;

namespace ExtremeRoles.Test;

public class GameTestRunner : TestRunnerBase
{
	public sealed record TestCase(string Name, int Iteration, HashSet<ExtremeRoleId>? Ids);

	public override void Run()
	{
		GameMudderEndTestingBehaviour.Instance.Logger = this.Log;
		GameMudderEndTestingBehaviour.Instance.StartCoroutine(
			GameMudderEndTestingBehaviour.Instance.Run(
				new("Random", 10, null),
				new("YokoWin", 100, new HashSet<ExtremeRoleId>() { ExtremeRoleId.Yoko }),
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

	public IEnumerator Run(params GameTestRunner.TestCase[] testCases)
	{
		foreach (var testCase in testCases)
		{
			yield return runTestCase(testCase);
		}
	}
	private IEnumerator runTestCase(GameTestRunner.TestCase testCase)
	{
		for (int i = 0; i < testCase.Iteration; ++i)
		{
			this.Logger.LogInfo($"{testCase.GetType().Name}.{testCase.Name} - Start iteration:{i}");
			if (testCase.Ids is null)
			{
				yield return GameUtility.StartGameWithRandom(this.Logger);
			}
			else
			{
				yield return GameUtility.StartGameWithRole(this.Logger, testCase.Ids);
			}

			while (GameUtility.IsContinue)
			{
				var player = CachedPlayerControl.AllPlayerControls.OrderBy(x => RandomGenerator.Instance.Next()).First();
				if (!player.Data.IsDead)
				{
					Player.RpcUncheckMurderPlayer(player.PlayerId, player.PlayerId, byte.MinValue);
					yield return new WaitForSeconds(1.0f);
				}
			}
			yield return GameUtility.ReturnLobby(this.Logger);
		}
	}
}
