using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using AmongUs.GameOptions;

using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Logging;

using ExtremeRoles.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;

using ExtremeRoles.Test.Helper;
using ExtremeRoles.GameMode.Option.ShipGlobal;

using UnityResource = UnityEngine.Resources;
using ExtremeRoles.Module.CustomOption;

namespace ExtremeRoles.Test;

public sealed class GameTestRunner : TestRunnerBase
{
	public sealed record TestCase(
		string Name, int Iteration,
		HashSet<ExtremeRoleId>? Ids = null, Action? PreSetUp = null);

	public override void Run()
	{
		GameMudderEndTestingBehaviour.Instance.Logger = this.Log;
		GameMudderEndTestingBehaviour.Instance.StartCoroutine(
			GameMudderEndTestingBehaviour.Instance.Run(
				new("Random", 256),
				new("IRoleAbilityRole", 5,
				[
					ExtremeRoleId.Carpenter,
					ExtremeRoleId.BodyGuard,
					ExtremeRoleId.SandWorm,
				]),
				new("IRoleAutoBuildAbilityRole", 5,
				[
					ExtremeRoleId.Hatter,
					ExtremeRoleId.Eater,
					ExtremeRoleId.Carrier,
					ExtremeRoleId.Thief,
					ExtremeRoleId.Traitor,
					ExtremeRoleId.Teleporter,
					ExtremeRoleId.Supervisor,
					ExtremeRoleId.Psychic,
					ExtremeRoleId.Mover,
				]),
				new("NeutralRemove", 5,
				[
					ExtremeRoleId.Jester, ExtremeRoleId.TaskMaster,
					ExtremeRoleId.Neet, ExtremeRoleId.Umbrer,
					ExtremeRoleId.Madmate
				]),
				new("YokoWin", 5, [ ExtremeRoleId.Yoko ],
				() =>
				{
					GameUtility.UpdateExROption(
						OptionTab.General,
						(int)ShipGlobalOptionCategory.NeutralWinOption,
						new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
				}),
				new("NeutralWin", 100,
				[
					ExtremeRoleId.Alice, ExtremeRoleId.Jackal,
					ExtremeRoleId.Missionary, ExtremeRoleId.Miner,
					ExtremeRoleId.Eater, ExtremeRoleId.Queen
				],
				() =>
				{
					GameUtility.UpdateExROption(
						OptionTab.General,
						(int)ShipGlobalOptionCategory.NeutralWinOption,
						new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
					GameUtility.UpdateAmongUsOption(
						new RequireOption<Int32OptionNames, int>(
							Int32OptionNames.NumImpostors, 0));
				}),
				new("QueenWin", 100, [ ExtremeRoleId.Queen ],
				() =>
				{
					GameUtility.UpdateExROption(
						OptionTab.General,
						(int)ShipGlobalOptionCategory.NeutralWinOption,
						new RequireOption<int, int>((int)NeutralWinOption.IsSame, 1));
					GameUtility.UpdateAmongUsOption(
						new RequireOption<Int32OptionNames, int>(
							Int32OptionNames.NumImpostors, 3));
				}),
				new("YandereWin", 100, [ ExtremeRoleId.Yandere ])));
	}
}

[Il2CppRegister]
public sealed class GameMudderEndTestingBehaviour : MonoBehaviour
{
	public static bool Enable => instance != null;

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

	[HideFromIl2Cpp]
	public ManualLogSource Logger { private get; set; }

	private int count = 0;
	private const int waitCount = 5;

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
			++this.count;

			this.Logger.LogInfo($"{testCase.GetType().Name}.{testCase.Name} - Start iteration:{i}");
			if (testCase.Ids is null)
			{
				GameUtility.PrepereGameWithRandom(this.Logger);
			}
			else
			{
				GameUtility.PrepereGameWithRole(this.Logger, testCase.Ids);
			}

			testCase.PreSetUp?.Invoke();

			if (this.count > waitCount)
			{
				this.Logger.LogInfo("Wait for 10s");
				GC.Collect();
				Resources.Loader.ResetCache();
				yield return UnityResource.UnloadUnusedAssets();
				yield return new WaitForSeconds(10.0f);
				this.count = 0;
			}

			yield return GameUtility.StartGame(this.Logger);

			while (IntroCutscene.Instance != null)
			{
				yield return new WaitForSeconds(10.0f);
			}

			while (GameUtility.IsContinue)
			{
				var player = PlayerCache.AllPlayerControl.OrderBy(x => RandomGenerator.Instance.Next()).First();
				if (!player.Data.IsDead && ExtremeRoleManager.GameRole[player.PlayerId].Id != ExtremeRoleId.Assassin)
				{
					Player.RpcUncheckMurderPlayer(player.PlayerId, player.PlayerId, byte.MinValue);
					yield return new WaitForSeconds(1.0f);
				}
			}
			yield return GameUtility.ReturnLobby(this.Logger);
		}
	}
}
