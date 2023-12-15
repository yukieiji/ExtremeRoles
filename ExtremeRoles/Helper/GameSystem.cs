using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using AmongUs.GameOptions;

using Newtonsoft.Json.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;
using ExtremeRoles.Compat.ModIntegrator;
using ExtremeRoles.Compat;

using UnityObject = UnityEngine.Object;
using UseButtonDict = Il2CppSystem.Collections.Generic.Dictionary<ImageNames, UseButtonSettings>;

#nullable enable

namespace ExtremeRoles.Helper;

public static class GameSystem
{
	public const int VanillaMaxPlayerNum = 15;
	public const int MaxImposterNum = 14;

	public const string SkeldKey = "Skeld";
	public const string MiraHqKey = "MiraHQ";
	public const string PolusKey = "Polus";
	public const string AirShipKey = "AirShip";
	public const string FungleKey = "Fungle";
	public const string SubmergedKey = "Submerged";

	public const string BottomRightButtonGroupObjectName = "BottomRight";

	public const string SkeldAdmin = "MapRoomConsole";
	public const string SkeldSecurity = "SurvConsole";

	public const string MiraHqAdmin = "AdminMapConsole";
	public const string MiraHqSecurity = "SurvLogConsole";

	public const string PolusAdmin1 = "panel_map";
	public const string PolusAdmin2 = "panel_map (1)";
	public const string PolusSecurity = "Surv_Panel";
	public const string PolusVital = "panel_vitals";

	public const string AirShipSecurity = "task_cams";
	public const string AirShipVital = "panel_vitals";
	public const string AirShipArchiveAdmin = "records_admin_map";
	public const string AirShipCockpitAdmin = "panel_cockpit_map";

	public const string FangleSecurity = "BinocularsSecurityConsole";
	public const string FangleVital = "VitalsConsole";

	private const string airShipSpawnJson =
		"ExtremeRoles.Resources.JsonData.AirShipSpawnPoint.json";
	private const string airShipRandomSpawnKey = "VanillaRandomSpawn";

	private const string ventInfoJson =
		"ExtremeRoles.Resources.JsonData.AllVentLinkInfo.json";

	private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

	public static bool IsLobby => AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started;
	public static bool IsFreePlay => AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;


	private static UseButtonDict useButtonSetting => FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;

	public static string CurMapKey
	{
		get
		{
			string key = string.Empty;

			if (CompatModManager.Instance.TryGetModMap(out var modMap))
			{
				if (modMap is SubmergedIntegrator)
				{
					key = "Submerged";
				}
			}
			else
			{
				byte mapId =  GameOptionsManager.Instance.CurrentGameOptions.GetByte(
					ByteOptionNames.MapId);
				key = mapId switch
				{
					0 => SkeldKey,
					1 => MiraHqKey,
					2 => PolusKey,
					4 => AirShipKey,
					5 => FungleKey,
					_ => string.Empty,
				};
			}
			return key;
		}
	}

	private static HashSet<TaskTypes> ignoreTask = new HashSet<TaskTypes>()
    {
        TaskTypes.FixWiring,
        TaskTypes.VentCleaning,
    };

    private static List<PlayerControl> bots = new List<PlayerControl>();

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
        destroyComponent<Collider2D>(body);
        destroyComponent<PassiveButton>(body);

        Vector3 vector = pos + killAnimation.BodyOffset;
        vector.z = vector.y / 1000f;
        body.transform.position = vector;

		UnityObject.Destroy(deadbody);

        return body;
    }

	public static void DisableMapConsole(string mapModuleName)
	{
		var mapConsoleArray = UnityObject.FindObjectsOfType<MapConsole>();
		findAndDisableComponent(mapConsoleArray, mapModuleName);
	}
	public static void DisableMapConsole(IReadOnlySet<string> mapModuleName)
	{
		var mapConsoleArray = UnityObject.FindObjectsOfType<MapConsole>();
		findAndDisableComponent(mapConsoleArray, mapModuleName);
	}
	public static void DisableSystemConsole(IReadOnlySet<string> mapModuleName)
	{
		var systemConsoleArray = UnityObject.FindObjectsOfType<SystemConsole>();
		findAndDisableComponent(systemConsoleArray, mapModuleName);
	}

	public static void SetColliderActive(GameObject obj, bool active)
    {
        setColliderEnable<Collider2D>(obj, active);
        setColliderEnable<PolygonCollider2D>(obj, active);
        setColliderEnable<BoxCollider2D>(obj, active);
        setColliderEnable<CircleCollider2D>(obj, active);
    }

	public static void AddSpawnPoint(in IEnumerable<Vector2> pos, in byte playerId)
	{
		var spawnPoint = new List<Vector2>();
		AddSpawnPoint(spawnPoint, playerId);
		pos.Concat(spawnPoint);
	}

	public static void AddSpawnPoint(in List<Vector2> pos, in byte playerId)
	{
		byte mapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId);

		int playerNum = CachedPlayerControl.AllPlayerControls.Count;

		if (CompatModManager.Instance.TryGetModMap(out var modMap))
		{
			pos.AddRange(modMap!.GetSpawnPos(playerId));
		}
		else
		{
			var ship = CachedShipStatus.Instance;

			switch (mapId)
			{
				case 4:
					pos.AddRange(GetAirShipRandomSpawn());
					break;
				default:
					Vector2 baseVec = Vector2.up;
					baseVec = baseVec.Rotate(
						(float)(playerId - 1) * (360f / (float)playerNum));
					Vector2 offset = baseVec * ship.SpawnRadius + new Vector2(
						0f, 0.3636f);
					pos.Add(ship.InitialSpawnCenter + offset);
					pos.Add(ship.MeetingSpawnCenter + offset);
					break;
			}
		}
	}

	public static ShipStatus GetShipObj(byte mapId)
	{
		byte optionMapId = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId);

		if (optionMapId == mapId)
		{
			return CachedShipStatus.Instance;
		}

		if (mapId > 5)
		{
			throw new ArgumentException("mapId is 5 over");
		}

		var fungleAsset = AmongUsClient.Instance.ShipPrefabs[5];

		GameObject obj;

		if (fungleAsset.IsValid())
		{
			obj = fungleAsset
				.OperationHandle
				.Result
				.Cast<GameObject>();
		}
		else
		{
			var asset = fungleAsset.LoadAsset<GameObject>();
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
        GameData.PlayerInfo playerInfo)
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

    public static int GetRandomNormalTaskId()
    {
        if (CachedShipStatus.Instance == null) { return byte.MaxValue; }

        List<int> taskIndex = getTaskIndex(
            CachedShipStatus.Instance.ShortTasks);

        int index = RandomGenerator.Instance.Next(taskIndex.Count);

        return taskIndex[index];
    }

    public static Sprite GetAdminButtonImage()
    {
		var useButtonKey = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId) switch
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
		var useButtonKey = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId) switch
		{
			1 => ImageNames.DoorLogsButton,
			_ => ImageNames.CamsButton,
		};
		return useButtonSetting[useButtonKey].Image;
    }
    public static Sprite GetVitalImage() =>
		useButtonSetting[ImageNames.VitalsButton].Image;

    public static SystemConsole? GetSecuritySystemConsole()
    {
        SystemConsole? watchConsole;
        if (CompatModManager.Instance.TryGetModMap(out var modMap))
        {
            watchConsole = modMap!.GetSystemConsole(SystemConsoleType.SecurityCamera);
        }
        else
        {
            watchConsole = getVanillaSecurityConsole();
        }
        return watchConsole;
    }

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

	public static void RelinkVent()
	{
		var allVent = new Dictionary<int, Vent>();
		foreach (Vent vent in CachedShipStatus.Instance.AllVents)
		{
			allVent.Add(vent.Id, vent);
		}

		JObject? linkInfoJson = JsonParser.GetJObjectFromAssembly(ventInfoJson);
		string key = CurMapKey;
		if (linkInfoJson == null || key == MiraHqKey) { return; }

		JArray linkInfo = linkInfoJson.Get<JArray>(key);

		for (int i = 0; i < linkInfo.Count; ++i)
		{
			JArray ventLinkedId = linkInfo.Get<JArray>(i);

			if (allVent.TryGetValue((int)ventLinkedId[0], out Vent? from) &&
				allVent.TryGetValue((int)ventLinkedId[1], out Vent? target) &&
				from != null && target != null)
			{
				linkVent(from, target);
			}
		}
	}

    public static void ReplaceToNewTask(byte playerId, int index, int taskIndex)
    {
        var player = Player.GetPlayerControlById(
            playerId);

        if (player == null) { return; }

        byte taskId = (byte)taskIndex;

        if (SetPlayerNewTask(ref player, taskId, (uint)index))
        {
            player.Data.Tasks[index] = new GameData.TaskInfo(
                taskId, (uint)index);
            player.Data.Tasks[index].Id = (uint)index;

            GameData.Instance.SetDirtyBit(
                1U << (int)player.PlayerId);
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
            CachedPlayerControl.LocalPlayer.PlayerControl.myTasks.GetFastEnumerator())
        {
            if (task == null) { continue; }

            TaskTypes taskType = task.TaskType;

            if (CompatModManager.Instance.TryGetModMap(out var modMap))
            {
                if (modMap!.IsCustomSabotageTask(taskType))
                {
                    modMap!.RpcRepairCustomSabotage(taskType);
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

				var minigame = Minigame.Instance;
				if (minigame != null && minigame.TryCast<SwitchMinigame>() != null)
				{
					minigame.ForceClose();
				}
				switchSystem!.ActualSwitches = switchSystem!.ExpectedSwitches;
				break;
			case SystemTypes.MushroomMixupSabotage:
				if (!system.IsTryCast<MushroomMixupSabotageSystem>(out var mixupSystem))
				{
					return;
				}
				mixupSystem!.currentSecondsUntilHeal = 0.001f;
				break;
			default:
				break;
		}
	}

    public static void SetTask(
        GameData.PlayerInfo playerInfo,
        int taskIndex)
    {
        NormalPlayerTask task = CachedShipStatus.Instance.GetTaskById((byte)taskIndex);

        PlayerControl player = playerInfo.Object;

        int index = playerInfo.Tasks.Count;
        playerInfo.Tasks.Add(new GameData.TaskInfo((byte)taskIndex, (uint)index));
        playerInfo.Tasks[index].Id = (uint)index;

        task.Id = (uint)index;
        task.Owner = player;
        task.Initialize();

        player.myTasks.Add(task);
        player.SetDirtyBit(1U << (int)player.PlayerId);
    }

    public static bool SetPlayerNewTask(
        ref PlayerControl player,
        byte taskId, uint gameControlTaskId)
    {
        NormalPlayerTask addTask = CachedShipStatus.Instance.GetTaskById(taskId);
        if (addTask == null) { return false; }

        for (int i = 0; i < player.myTasks.Count; ++i)
        {
			var task = player.myTasks[i];

            if (task == null ||
				task.gameObject.TryGetComponent<ImportantTextTask>(out var _) ||
				PlayerTask.TaskIsEmergency(task)) { continue; }

            if (CompatModManager.Instance.TryGetModMap(out var modMap) &&
				modMap!.IsCustomSabotageTask(task.TaskType))
            {
				continue;
			}

            if (task.IsComplete)
            {
                NormalPlayerTask normalPlayerTask = UnityObject.Instantiate(
                    addTask, player.transform);
                normalPlayerTask.Id = gameControlTaskId;
                normalPlayerTask.Owner = player;
                normalPlayerTask.Initialize();

                var removeTask = player.myTasks[i];
                player.myTasks[i] = normalPlayerTask;

                removeTask.OnRemove();
                UnityObject.Destroy(removeTask.gameObject);
                if (player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                {
					ExtremeRolesPlugin.Logger.LogInfo(
						$"Adding New Task\n - Task:{task.TaskType}\n - Id:{gameControlTaskId}\n - Index:{i}");
                    Sound.PlaySound(
                        Sound.SoundType.ReplaceNewTask, 1.2f);
                }
                return true;
            }
        }
        return false;
    }

    public static List<Vector2> GetAirShipRandomSpawn()
    {
        JObject? json = JsonParser.GetJObjectFromAssembly(airShipSpawnJson);

		List<Vector2> result = new List<Vector2>();

		if (json == null) { return result; }

        JArray airShipSpawn = json.Get<JArray>(airShipRandomSpawnKey);

        for (int i = 0; i < airShipSpawn.Count; ++i)
        {
            JArray pos = airShipSpawn.Get<JArray>(i);
            result.Add(new Vector2((float)pos[0], (float)pos[1]));
        }

        return result;
    }

    public static void ShareVersion()
    {
        Version? ver = Assembly.GetExecutingAssembly().GetName().Version;

		if (ver is null) { return; }

        using (var caller = RPCOperator.CreateCaller(
            RPCOperator.Command.ShareVersion))
        {
            caller.WriteInt(ver.Major);
            caller.WriteInt(ver.Minor);
            caller.WriteInt(ver.Build);
            caller.WriteInt(ver.Revision);
            caller.WritePackedInt(AmongUsClient.Instance.ClientId);
        }

        RPCOperator.AddVersionData(
            ver.Major, ver.Minor,
            ver.Build, ver.Revision,
            AmongUsClient.Instance.ClientId);
    }

    public static void SpawnDummyPlayer(string name = "")
    {
        PlayerControl playerControl = UnityEngine.Object.Instantiate(
                AmongUsClient.Instance.PlayerPrefab);
        playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

        bots.Add(playerControl);
        GameData.Instance.AddPlayer(playerControl);
        AmongUsClient.Instance.Spawn(playerControl, -2, InnerNet.SpawnFlags.None);

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
        GameData.Instance.RpcSetTasks(playerControl.PlayerId, Array.Empty<byte>());
    }

	private static void findAndDisableComponent<T>(
		Il2CppArrayBase<T> array, IReadOnlySet<string> disableComponent) where T : Component
	{
		foreach (string name in disableComponent)
		{
			findAndDisableComponent(array, name);
		}
	}

	private static void findAndDisableComponent<T>(
		Il2CppArrayBase<T> array, string name) where T : Component
	{
		T? target = array.FirstOrDefault(x => x.gameObject.name == name);
		if (target != null)
		{
			SetColliderActive(target.gameObject, false);
		}
	}

	private static void setColliderEnable<T>(GameObject obj, bool active) where T : Collider2D
    {
        T comp = obj.GetComponent<T>();
        if (comp != null)
        {
            comp.enabled = active;
        }
    }

    private static List<int> getTaskIndex(
        NormalPlayerTask[] tasks)
    {
        List<int> index = new List<int>();
        for (int i = 0; i < tasks.Length; ++i)
        {
            if (!ignoreTask.Contains(tasks[i].TaskType))
            {
                index.Add(tasks[i].Index);
            }
        }

        return index;
    }

    private static SystemConsole? getVanillaSecurityConsole()
    {
        // 0 = Skeld
        // 1 = Mira HQ
        // 2 = Polus
        // 3 = Dleks - deactivated
        // 4 = Airship
		string key = GameOptionsManager.Instance.CurrentGameOptions.GetByte(
			ByteOptionNames.MapId) switch
		{
			0 or 3 => SkeldSecurity,
			1 => MiraHqSecurity,
			2 => PolusSecurity,
			4 => AirShipSecurity,
			5 => FangleSecurity,
			_ => string.Empty,
		};

		var systemConsoleArray = UnityObject.FindObjectsOfType<SystemConsole>();

		return systemConsoleArray.FirstOrDefault(
			x => x.gameObject.name.Contains(key));

	}

	private static void destroyComponent<T>(GameObject obj) where T : Behaviour
    {
        T collider = obj.GetComponent<T>();
        if (collider != null)
        {
			UnityObject.Destroy(collider);
        }
    }

	private static void linkVent(Vent from, Vent target)
	{
		if (from == null || target == null) { return; }

		linkVentToEmptyTarget(from, target);
		linkVentToEmptyTarget(target, from);
	}

	private static void linkVentToEmptyTarget(Vent from, Vent target)
	{
		if (from.Right == null)
		{
			from.Right = target;
		}
		else if (from.Center == null)
		{
			from.Center = target;
		}
		else if (from.Left == null)
		{
			from.Left = target;
		}
		else
		{
			ExtremeRolesPlugin.Logger.LogInfo("Vent Link fail!!");
		}
	}
}
