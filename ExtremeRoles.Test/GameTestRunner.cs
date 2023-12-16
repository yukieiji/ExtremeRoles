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

namespace ExtremeRoles.Test;

internal class GameTestRunner : TestRunnerBase
{
	private const int iteration = 1000000;

	public override void Export()
	{

	}

	public override void Run()
	{
		GameMudderEndTestingBehaviour.Instance.Logger = this.Log;
		GameMudderEndTestingBehaviour.Instance.StartCoroutine(
			GameMudderEndTestingBehaviour.Instance.Run(iteration));
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

	public IEnumerator Run(int num)
	{
		for (int i = 0; i < num; ++i)
		{
			this.Logger.LogInfo($"Start iteration:{i}");
			yield return GameUtility.StartGameWithRandom(this.Logger);

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
