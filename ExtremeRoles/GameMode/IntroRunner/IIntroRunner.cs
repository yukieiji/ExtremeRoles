using System;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

using UnityEngine;

using ExtremeRoles.Helper;

using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.CheckPoint;

using ExtremeRoles.Performance;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Roles.Solo.Host;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.Interface;

using TMPro;
using ExtremeRoles.GameMode.Option.ShipGlobal.Sub.MapModule;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.GameMode.IntroRunner;

#nullable enable

public sealed class IntroText
{
	public TextMeshPro RoleInfoText { get; }
	private readonly GameObject roleAssignText;
	
	public IntroText(GameObject roleAssignText, TextMeshPro roleInfoText)
	{
		RoleInfoText = roleInfoText;
		this.roleAssignText = roleAssignText;

		this.roleAssignText.SetActive(true);
		this.RoleInfoText.gameObject.SetActive(true);
	}

	public void Destroy()
	{
		if (this.roleAssignText != null)
		{
			this.roleAssignText.SetActive(false);
			UnityObject.Destroy(this.roleAssignText);
		}
		if (this.RoleInfoText != null)
		{
			this.RoleInfoText.gameObject.SetActive(false);
			UnityObject.Destroy(RoleInfoText.gameObject);
		}
	}
}

public interface IIntroRunner
{
    public IEnumerator CoRunModeIntro(IntroCutscene instance, IntroText introText);

    public IEnumerator CoRunIntro(IntroCutscene instance)
    {
		var text = new IntroText(
			createLoadingText(),
			new TextMeshPro());

        // Turn on loading animation
        var loadingAnimation = HudManager.Instance.GameLoadAnimation;
        loadingAnimation.SetActive(true);

        yield return waitRoleAssign(text.RoleInfoText, 5f );


        Logger.GlobalInstance.Info(
            "IntroCutscene :: CoBegin() :: Starting intro cutscene", null);

        SoundManager.Instance.PlaySound(instance.IntroStinger, false, 1f);

        yield return CoRunModeIntro(instance, text);

		ExtremeSystemTypeManager.AddSystem();

		prepareXion();

		InfoOverlay.Instance.InitializeToGame();

		setupRoleWhenIntroEnd();
		modMapObject();
		changeWallHackTask();

		UnityObject.Destroy(instance.gameObject);

        yield break;
    }

	private static GameObject createLoadingText()
	{
		// Original "Assigning roles" text setup
		GameObject roleAssignText = new GameObject("roleAssignText");
		var text = roleAssignText.AddComponent<Module.CustomMonoBehaviour.LoadingText>();
		text.SetFontSize(3.0f);
		text.SetMessage(Tr.GetString("roleAssignNow"));

		return roleAssignText;
	}

    private static string createRoleListString(ISpawnDataManager spawnDataManager)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Tr.GetString("RoleSpawnRate"));
        sb.AppendLine();

        var singleRoleData = spawnDataManager.CurrentSingleRoleSpawnData
            .SelectMany(x => x.Value.Select(y => (team: x.Key, id: y.Key, data: y.Value)))
            .GroupBy(x => x.team);

        foreach (var group in singleRoleData)
        {
            string teamName = Tr.GetString($"Team{group.Key}");
            sb.Append($"{teamName}: ");

            var roleTexts = group.Select(x => {
                var role = ExtremeRoleManager.NormalRole[x.id];
                return $"{role.RoleName}({x.data.SpawnRate}%)";
            });

            sb.AppendLine(string.Join(", ", roleTexts));
        }

        if (spawnDataManager.CurrentCombRoleSpawnData.Any())
        {
            sb.AppendLine();
            string teamName = Tr.GetString("TeamCombination");
            sb.Append($"{teamName}: ");

            var roleTexts = spawnDataManager.CurrentCombRoleSpawnData.Select(x => {
                var role = ExtremeRoleManager.CombRole[x.Key];
                return $"{role.RoleName}({x.Value.SpawnRate}%)";
            });
            sb.AppendLine(string.Join(", ", roleTexts));
        }

        return sb.ToString();
    }

    private static IEnumerator waitRoleAssign(TextMeshPro text, float minWaitTime)
    {
        float timer = 0f;

		var localPlayer = PlayerControl.LocalPlayer;
		if (localPlayer == null)
		{
			yield break;
		}

		var provider = ExtremeRolesPlugin.Instance.Provider;
		if (AmongUsClient.Instance.AmHost)
        {
			RPCOperator.Call(localPlayer.NetId, RPCOperator.Command.Initialize);
			RPCOperator.Initialize();
	
			var assignee = provider.GetRequiredService<IRoleAssignee>();
			var spawnData = assignee.PreparationData.RoleSpawn;

			if (!isAllPlyerDummy())
            {
				RoleAssignCheckPoint.RpcCheckpoint();
				text.text = createRoleListString(spawnData);
				// ホストは全員の処理が終わるまで待つ
				do
				{
                    timer += Time.deltaTime;
					yield return null;

				} while (!RoleAssignState.Instance.IsReady);

				yield return null;
			}
            else
            {
				text.text = createRoleListString(spawnData);
				yield return new WaitForSeconds(2.5f);
                timer += 2.5f;
            }

			yield return assignee.CoRpcAssign();
		}
        else
        {
            // クライアントはここでオプション値を読み込むことで待ち時間を短く見せるトリック
            OptionManager.Load();

            // ラグも有るかもしれないで1フレーム待機
            yield return null;
            timer += Time.deltaTime;

			// ホスト以外はここまで処理済みである事を送信
			RoleAssignCheckPoint.RpcCheckpoint();
			text.text = createRoleListString(
				(provider.GetRequiredService<IRoleAssignDataPreparer>().Prepare().RoleSpawn));
		}

        // バニラの役職アサイン後すぐこの処理が走るので全員の役職が入るまで待機
        while (!RoleAssignState.Instance.IsRoleSetUpEnd)
        {
            timer += Time.deltaTime;
            yield return null;
        }

		timer += Time.deltaTime;
		yield return null;

		// 割り当て完了後、最低待機時間に満たない場合は待機
		if (timer < minWaitTime)
        {
            yield return new WaitForSeconds(minWaitTime - timer);
        }

		yield break;
    }

    private static bool isAllPlyerDummy()
    {
        foreach (var player in PlayerCache.AllPlayerControl)
        {
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) { continue; }

            if (!player.GetComponent<DummyBehaviour>().enabled)
            {
                return false;
            }
        }
        return true;
    }

	private static void prepareXion()
	{
		if (!ExtremeGameModeManager.Instance.EnableXion)
		{
			return;
		}

		Xion.XionPlayerToGhostLayer();
		Xion.RemoveXionPlayerToAllPlayerControl();

		if (AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame)
		{
			return;
		}

		foreach (PlayerControl player in PlayerCache.AllPlayerControl)
		{
			if (player == null ||
				!player.GetComponent<DummyBehaviour>().enabled) { continue; }

			var role = ExtremeRoleManager.GameRole[player.PlayerId];
			if (!role.HasTask())
			{
				continue;
			}

			NetworkedPlayerInfo playerInfo = player.Data;

			var (_, totalTask) = GameSystem.GetTaskInfo(playerInfo);
			if (totalTask != 0)
			{
				continue;
			}
			GameSystem.SetTask(playerInfo,
				GameSystem.GetRandomCommonTaskId());
		}
	}

	private static void setupRoleWhenIntroEnd()
	{
		var localRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (localRole is IRoleSpecialSetUp setUpRole)
		{
			setUpRole.IntroEndSetUp();
		}

		if (localRole is MultiAssignRoleBase multiAssignRole &&
			multiAssignRole.AnotherRole is IRoleSpecialSetUp multiSetUpRole)
		{
			multiSetUpRole.IntroEndSetUp();
		}
	}

	private static void changeWallHackTask()
	{
		var shipOpt = ExtremeGameModeManager.Instance.ShipOption;
		if (!shipOpt.ChangeForceWallCheck) { return; }

		var changeWallCheckTask = shipOpt.ChangeTask;
		var wallCheckTasks = shipOpt.WallCheckTask;

		var allConsole = Object.FindObjectsOfType<Console>();

		foreach (Console console in allConsole)
		{
			foreach (var taskType in console.TaskTypes)
			{
				if (wallCheckTasks.Contains(taskType))
				{
					console.checkWalls = changeWallCheckTask.Contains(taskType);
					break;
				}
			}
		}
	}

	private static void modMapObject()
	{
		var shipOption = ExtremeGameModeManager.Instance.ShipOption;
		byte mapId = Map.Id;

		modAdmin(shipOption.Admin, mapId);
		modVital(shipOption.Vital, mapId);

		if (shipOption.Security.Disable)
		{
			Map.DisableSecurity();
		}
	}

	private static void modVital(in VitalDeviceOption option, in byte mapId)
	{
		if (option.Disable)
		{
			Map.DisableVital();
		}
		// ポーラスだけバイタルの位置を変える
		else if (mapId == 2)
		{
			var systemConsoleArray = Object.FindObjectsOfType<SystemConsole>();
			var vitalConsole = systemConsoleArray.FirstOrDefault(
				x => x.gameObject.name == Map.PolusVital);
			if (vitalConsole == null) { return; }
			var vitalTrans = vitalConsole.transform;
			switch (option.PolusPos)
			{
				case PolusVitalPos.Laboratory:
					vitalTrans.localPosition = new Vector3(12.75f, 10.5f, -1.1f);
					break;
				case PolusVitalPos.Specimens:
					vitalTrans.localPosition = new Vector3(16.9f, - 3.25f, -0.1f);
					vitalTrans.localEulerAngles = new Vector3(0, 0, 90);
					vitalTrans.localScale = new Vector3(0.9f, 0.9f, 1.0f);
					break;
				default:
					break;
			}
		}
	}

	private static void modAdmin(in AdminDeviceOption option, in byte mapId)
	{
		if (option.Disable)
		{
			Map.DisableAdmin();
		}
		// AirShipのみ一部のアドミンを消す
		else if (mapId == 4)
		{
			string removeTargetAdmin = option.AirShipEnable switch
			{
				AirShipAdminMode.ModeArchiveOnly => Map.AirShipCockpitAdmin,
				AirShipAdminMode.ModeCockpitOnly => Map.AirShipArchiveAdmin,
				_ => string.Empty,
			};
			if (string.IsNullOrEmpty(removeTargetAdmin))
			{
				return;
			}
			Map.DisableConsole(removeTargetAdmin);
		}
	}
}
