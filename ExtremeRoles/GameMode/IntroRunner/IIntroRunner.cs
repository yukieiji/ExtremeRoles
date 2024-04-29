using System.Collections;

using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.GameMode.Option.MapModule;

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
using ExtremeRoles.GameMode.Option.ShipGlobal;

namespace ExtremeRoles.GameMode.IntroRunner;

#nullable enable

public interface IIntroRunner
{
    public IEnumerator CoRunModeIntro(IntroCutscene instance, GameObject roleAssignText);

    public IEnumerator CoRunIntro(IntroCutscene instance)
    {
        GameObject roleAssignText = new GameObject("roleAssignText");
        var text = roleAssignText.AddComponent<Module.CustomMonoBehaviour.LoadingText>();
        text.SetFontSize(3.0f);
        text.SetMessage(Translation.GetString("roleAssignNow"));

        roleAssignText.SetActive(true);

        yield return waitRoleAssign();

        Logger.GlobalInstance.Info(
            "IntroCutscene :: CoBegin() :: Starting intro cutscene", null);

        SoundManager.Instance.PlaySound(instance.IntroStinger, false, 1f);

        yield return CoRunModeIntro(instance, roleAssignText);

		ExtremeSystemTypeManager.AddSystem();

		prepareXion();

		InfoOverlay.Instance.InitializeToGame();

		setupRoleWhenIntroEnd();
		disableMapObject();
		changeWallHackTask();

		Object.Destroy(instance.gameObject);

        yield break;
    }

    private static IEnumerator waitRoleAssign()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame ||
                !isAllPlyerDummy())
            {
				RoleAssignCheckPoint.RpcCheckpoint();
                // ホストは全員の処理が終わるまで待つ
                while (!RoleAssignState.Instance.IsReady)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            PlayerRoleAssignData.Instance.AllPlayerAssignToExRole();
        }
        else
        {
            // クライアントはここでオプション値を読み込むことで待ち時間を短く見せるトリック
            OptionManager.Load();

            // ラグも有るかもしれないで1フレーム待機
            yield return null;

			// ホスト以外はここまで処理済みである事を送信
			RoleAssignCheckPoint.RpcCheckpoint();
		}

        // バニラの役職アサイン後すぐこの処理が走るので全員の役職が入るまで待機
        while (!RoleAssignState.Instance.IsRoleSetUpEnd)
        {
            yield return null;
        }
        yield break;
    }

    private static bool isAllPlyerDummy()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
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

		foreach (PlayerControl player in CachedPlayerControl.AllPlayerControls)
		{
			if (player == null ||
				!player.GetComponent<DummyBehaviour>().enabled) { continue; }

			var role = ExtremeRoleManager.GameRole[player.PlayerId];
			if (!role.HasTask())
			{
				continue;
			}

			GameData.PlayerInfo playerInfo = player.Data;

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

	private static void disableMapObject()
	{
		var removeMapModule = ExtremeGameModeManager.Instance.ShipOption.RemoveMapModule;
		modAdmin(in removeMapModule);

		if (removeMapModule.Vital)
		{
			Map.DisableVital();
		}
		if (removeMapModule.Security)
		{
			Map.DisableSecurity();
		}
	}

	private static void modAdmin(in MapModuleDisableFlag flag)
	{
		if (flag.Admin)
		{
			Map.DisableAdmin();
		}
		// AirShipのみ一部のアドミンを消す
		else if (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId) == 4)
		{
			string removeTargetAdmin = flag.AirShipAdminMode switch
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
