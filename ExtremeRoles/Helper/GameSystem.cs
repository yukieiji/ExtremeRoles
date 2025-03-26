using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Compat;
using ExtremeRoles.Compat.ModIntegrator;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using UnityObject = UnityEngine.Object;
using UseButtonDict = Il2CppSystem.Collections.Generic.Dictionary<ImageNames, UseButtonSettings>;
using System.Diagnostics.CodeAnalysis;


#nullable enable

namespace ExtremeRoles.Helper;

public static class GameSystem
{
	public static int VanillaMaxPlayerNum =>
		CompatModManager.Instance.TryGetMod<CrowdedMod>(CompatModType.CrowdedMod, out var mod) ?
		mod.MaxPlayerNum : 15;

	public static int MaxImposterNum => VanillaMaxPlayerNum - 1;

	public const string BottomRightButtonGroupObjectName = "BottomRight";

	private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	public static bool IsLobby => AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started;
	public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;


	private static UseButtonDict useButtonSetting => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;

	private static IReadOnlySet<TaskTypes> ignoreTask = new HashSet<TaskTypes>()
    {
        TaskTypes.FixWiring,
        TaskTypes.VentCleaning,
    };

    public static GameObject CreateNoneReportableDeadbody(
        PlayerControl targetPlayer, Vector3 pos)
    {
        var killAnimation = targetPlayer.KillAnimations[0];
        DeadBody deadbody = UnityObject.Instantiate(
            GameManager.Instance.DeadBodyPrefab);
        deadbody.enabled = false;

        foreach (var rend in deadbody.bodyRenderers)
        {
            targetPlayer.SetPlayerMaterialColors(rend);
        }
        targetPlayer.SetPlayerMaterialColors(deadbody.bloodSplatter);
        deadbody.enabled = true;

        GameObject body = deadbody.gameObject;

        Unity.DestroyComponent<Collider2D>(body);
		Unity.DestroyComponent<PassiveButton>(body);

        Vector3 vector = pos + killAnimation.BodyOffset;
        vector.z = vector.y / 1000f;
        body.transform.position = vector;

		UnityObject.Destroy(deadbody);

        return body;
    }

	public static ArrowBehaviour GetArrowTemplate()
	{
		ArrowBehaviour? template = null;

		foreach (var task in CachedShipStatus.Instance.SpecialTasks)
		{
			if (!task.IsTryCast<SabotageTask>(out var saboTask) ||
				saboTask.Arrows.Count == 0)
			{
				continue;
			}
			template = saboTask.Arrows[0];
			break;
		}
		if (template == null)
		{
			throw new ArgumentNullException("Arrow is Null!!");
		}
		return template;
	}

	public static ShipStatus GetShipObj(byte mapId)
	{
		if (Map.Id == mapId &&
			CachedShipStatus.Instance != null)
		{
			return CachedShipStatus.Instance;
		}

		if (mapId > 5)
		{
			throw new ArgumentException("mapId is 5 over");
		}

		var shipPrefabRef = AmongUsClient.Instance.ShipPrefabs[mapId];

		GameObject obj;

		if (shipPrefabRef.IsValid())
		{
			obj = shipPrefabRef
				.OperationHandle
				.Result
				.Cast<GameObject>();
		}
		else
		{
			var asset = shipPrefabRef.LoadAsset<GameObject>();
			obj = asset.WaitForCompletion();
		}

		ShipStatus result = obj.GetComponent<ShipStatus>();

		return result;
	}

	public static DeadBody? GetDeadBody(byte playerId)
	{
		DeadBody[] array = UnityObject.FindObjectsOfType<DeadBody>();
		DeadBody? body = array.FirstOrDefault(
			x => GameData.Instance.GetPlayerById(x.ParentId).PlayerId == playerId);
		return body;
	}

    public static (int, int) GetTaskInfo(
        NetworkedPlayerInfo playerInfo)
    {
        int TotalTasks = 0;
        int CompletedTasks = 0;
        if (!(playerInfo.Disconnected) &&
             (playerInfo.Tasks != null) &&
             (playerInfo.Object) &&
             (playerInfo.Role) &&
             (playerInfo.Role.TasksCountTowardProgress) &&
             (
                GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) ||
                !playerInfo.IsDead
            ) &&
              ExtremeRoleManager.GameRole[playerInfo.PlayerId].HasTask()
            )
        {

            for (int j = 0; j < playerInfo.Tasks.Count; ++j)
            {
                ++TotalTasks;
                if (playerInfo.Tasks[j].Complete)
                {
                    ++CompletedTasks;
                }
            }
        }
        return (CompletedTasks, TotalTasks);
    }

    public static int GetRandomCommonTaskId()
    {
        if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

        List<int> taskIndex = getTaskIndex(
            CachedShipStatus.Instance.CommonTasks);

        int index = RandomGenerator.Instance.Next(taskIndex.Count);

        return taskIndex[index];
    }

    public static int GetRandomLongTask()
    {
        if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

        List<int> taskIndex = getTaskIndex(
            CachedShipStatus.Instance.LongTasks);

        int index = RandomGenerator.Instance.Next(taskIndex.Count);

        return taskIndex[index];
    }

    public static int GetRandomShortTaskId()
    {
        if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

        List<int> taskIndex = getTaskIndex(
            CachedShipStatus.Instance.ShortTasks);

        int index = RandomGenerator.Instance.Next(taskIndex.Count);

        return taskIndex[index];
    }

    public static Sprite GetAdminButtonImage()
    {
		var useButtonKey = Map.Id switch
		{
			0 or 3 => ImageNames.AdminMapButton,
			1 => ImageNames.MIRAAdminButton,
			2 => ImageNames.PolusAdminButton,
			_ => ImageNames.AirshipAdminButton
		};
		return useButtonSetting[useButtonKey].Image;

	}

    public static Sprite GetSecurityImage()
    {
		var useButtonKey = Map.Id switch
		{
			1 => ImageNames.DoorLogsButton,
			_ => ImageNames.CamsButton,
		};
		return useButtonSetting[useButtonKey].Image;
    }
    public static Sprite GetVitalImage() =>
		useButtonSetting[ImageNames.VitalsButton].Image;

    public static void ForceEndGame()
    {
        RPCOperator.Call(RPCOperator.Command.ForceEnd);
        RPCOperator.ForceEnd();
    }

	public static bool IsValidConsole(PlayerControl player, Console console)
    {
        if (player == null || console == null) { return false; }

        Vector2 playerPos = player.GetTruePosition();
        Vector2 consolePos = console.transform.position;

        bool isCheckWall = console.checkWalls;

        return
            player.CanMove &&
            (!console.onlySameRoom || console.InRoom(playerPos)) &&
            (!console.onlyFromBelow || playerPos.y < consolePos.y) &&
            Vector2.Distance(playerPos, consolePos) <= console.UsableDistance &&
            (
                !isCheckWall ||
                !PhysicsHelpers.AnythingBetween(
                        playerPos, consolePos, Constants.ShadowMask, false)
            );
    }

    public static void ReplaceToNewTask(byte playerId, int index, int taskIndex)
    {
        var player = Player.GetPlayerControlById(
            playerId);

        if (player == null) { return; }

        byte taskId = (byte)taskIndex;
		uint id = (uint)index;

        if (SetPlayerNewTask(player, taskId, id))
        {
            player.Data.Tasks[index] = new NetworkedPlayerInfo.TaskInfo(taskId, id);
			player.Data.MarkDirty();
        }
		if (AmongUsClient.Instance != null &&
			AmongUsClient.Instance.AmHost)
		{
			GameData.Instance.RecomputeTaskCounts();
		}
    }

    public static void RpcReplaceNewTask(
        byte targetPlayerId, int replaceTaskIndex, int newTaskId)
    {
        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ReplaceTask))
        {
            caller.WriteByte(targetPlayerId);
            caller.WriteInt(replaceTaskIndex);
            caller.WriteInt(newTaskId);
        }
        ReplaceToNewTask(
            targetPlayerId,
            replaceTaskIndex,
            newTaskId);
    }

    public static void RpcRepairAllSabotage()
    {
		foreach (PlayerTask task in
            PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
        {
            if (task == null) { continue; }

            TaskTypes taskType = task.TaskType;

            if (CompatModManager.Instance.TryGetModMap(out var modMap))
            {
                if (modMap.IsCustomSabotageTask(taskType))
                {
                    modMap.RpcRepairCustomSabotage(taskType);
                    continue;
                }
            }

			var ship = CachedShipStatus.Instance;

			switch (taskType)
            {
				case TaskTypes.ResetReactor:
					ship.RpcUpdateSystem(SystemTypes.Reactor, 16);
					break;
				case TaskTypes.FixLights:
					RpcForceRepairSpecialSabotage(SystemTypes.Electrical);
					break;
				case TaskTypes.FixComms:
					ship.RpcUpdateSystem(SystemTypes.Comms, 0 | 16);
					ship.RpcUpdateSystem(SystemTypes.Comms, 1 | 16);
					break;
				case TaskTypes.RestoreOxy:
                    ship.RpcUpdateSystem(SystemTypes.LifeSupp, 16);
                    break;
				case TaskTypes.ResetSeismic:
                    ship.RpcUpdateSystem(SystemTypes.Laboratory, 16);
                    break;
                case TaskTypes.StopCharles:
                    ship.RpcUpdateSystem(SystemTypes.HeliSabotage, 0 | 16);
                    ship.RpcUpdateSystem(SystemTypes.HeliSabotage, 1 | 16);
                    break;
				case TaskTypes.MushroomMixupSabotage:
					RpcForceRepairSpecialSabotage(SystemTypes.MushroomMixupSabotage);
					break;
				default:
                    break;
            }
        }
    }

	public static void RpcForceRepairSpecialSabotage(SystemTypes sabSystem)
	{
		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.FixForceRepairSpecialSabotage))
		{
			caller.WriteByte((byte)sabSystem);
		}
		ForceRepairrSpecialSabotage(sabSystem);
	}

	public static void ForceRepairrSpecialSabotage(SystemTypes sabSystem)
	{
		if (!CachedShipStatus.Systems.TryGetValue(sabSystem, out var system))
		{
			return;
		}
		switch (sabSystem)
		{
			case SystemTypes.Electrical:

				if (!system.IsTryCast<SwitchSystem>(out var switchSystem))
				{
					return;
				}

				if (Minigame.Instance.IsTryCast<SwitchMinigame>(out var switchMinigame))
				{
					switchMinigame.ForceClose();
				}
				switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
				break;
			case SystemTypes.MushroomMixupSabotage:
				if (!system.IsTryCast<MushroomMixupSabotageSystem>(out var mixupSystem))
				{
					return;
				}
				mixupSystem.currentSecondsUntilHeal = 0.001f;
				break;
			default:
				break;
		}
	}

    public static void SetTask(
        NetworkedPlayerInfo playerInfo,
        int taskIndex)
    {
        NormalPlayerTask task = CachedShipStatus.Instance.GetTaskById((byte)taskIndex);

        PlayerControl player = playerInfo.Object;

        int index = playerInfo.Tasks.Count;
        playerInfo.Tasks.Add(new ((byte)taskIndex, (uint)index));
        playerInfo.Tasks[index].Id = (uint)index;

        task.Id = (uint)index;
        task.Owner = player;
        task.Initialize();

        player.myTasks.Add(task);
        player.SetDirtyBit(1U << (int)player.PlayerId);
    }

    public static bool SetPlayerNewTask(
        PlayerControl player,
        byte taskId, uint gameControlTaskId)
    {
        NormalPlayerTask addTask = CachedShipStatus.Instance.GetTaskById(taskId);
        if (addTask == null)
		{
			return false;
		}

        for (int i = 0; i < player.myTasks.Count; ++i)
        {
			var task = player.myTasks[i];

            if (task == null ||
				task.gameObject.TryGetComponent<ImportantTextTask>(out var _) ||
				PlayerTask.TaskIsEmergency(task))
			{
				continue;
			}

			if (CompatModManager.Instance.TryGetModMap(out var modMap) &&
				modMap.IsCustomSabotageTask(task.TaskType))
            {
				continue;
			}
			if (!task.IsComplete)
			{
				continue;
			}

			NormalPlayerTask normalPlayerTask = UnityObject.Instantiate(
				addTask, player.transform);
			normalPlayerTask.Id = gameControlTaskId;
			normalPlayerTask.Owner = player;
			normalPlayerTask.Initialize();

			player.myTasks[i] = normalPlayerTask;

			task.OnRemove();
			UnityObject.Destroy(task.gameObject);
			if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
			{
				ExtremeRolesPlugin.Logger.LogInfo(
					$"Adding New Task\n - Task:{task.TaskType}\n - Id:{gameControlTaskId}\n - Index:{i}");
				Sound.PlaySound(
					Sound.Type.ReplaceNewTask, 1.2f);
			}
			return true;
		}
        return false;
    }

	public static void SpawnDummyPlayer(string name = "")
    {
        PlayerControl playerControl = UnityObject.Instantiate(
            AmongUsClient.Instance.PlayerPrefab);
        playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

        var dummyData = GameData.Instance.AddDummy(playerControl);
		AmongUsClient.Instance.Spawn(dummyData);
        AmongUsClient.Instance.Spawn(playerControl);
		playerControl.isDummy = true;

        var hatMng = FastDestroyableSingleton<HatManager>.Instance;
        var rng = RandomGenerator.GetTempGenerator();

        int hat = rng.Next(hatMng.allHats.Count);
        int pet = rng.Next(hatMng.allPets.Count);
        int skin = rng.Next(hatMng.allSkins.Count);
        int visor = rng.Next(hatMng.allVisors.Count);
        int color = rng.Next(Palette.PlayerColors.Length);

        playerControl.transform.position = PlayerControl.LocalPlayer.transform.position;
        playerControl.GetComponent<DummyBehaviour>().enabled = true;
        playerControl.NetTransform.enabled = false;
        playerControl.SetName(string.IsNullOrEmpty(name) ?
            new string(Enumerable.Repeat(chars, 10).Select(s => s[rng.Next(s.Length)]).ToArray()) :
            name);
        playerControl.SetColor(color);
        playerControl.SetHat(hatMng.allHats[hat].ProdId, color);
        playerControl.SetPet(hatMng.allPets[pet].ProdId, color);
        playerControl.SetVisor(hatMng.allVisors[visor].ProdId, color);
        playerControl.SetSkin(hatMng.allSkins[skin].ProdId, color);

		dummyData.PlayerId = playerControl.PlayerId;
		dummyData.RpcSetTasks(new Il2CppStructArray<byte>(0));
	}

    private static List<int> getTaskIndex(
        NormalPlayerTask[] tasks)
    {
        List<int> index = new List<int>(tasks.Length);
        for (int i = 0; i < tasks.Length; ++i)
        {
            if (!ignoreTask.Contains(tasks[i].TaskType))
            {
                index.Add(tasks[i].Index);
            }
        }

        return index;
    }

	public static bool TryGetKillDistance([NotNullWhen(true)] out Il2CppStructArray<float>? arr)
	{
		arr = null;
		return
			GameManager.Instance != null &&
			GameManager.Instance.LogicOptions != null &&
			GameManager.Instance.LogicOptions.currentGameOptions.TryGetFloatArray(
				FloatArrayOptionNames.KillDistances, out arr);
	}
}
