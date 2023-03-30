using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using AmongUs.GameOptions;

using Newtonsoft.Json.Linq;

using ExtremeRoles.Extension.Json;
using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Helper;

public static class GameSystem
{
    public const int VanillaMaxPlayerNum = 15;
    public const int MaxImposterNum = 14;

    public const string BottomRightButtonGroupObjectName = "BottomRight";

    public const string SkeldAdmin = "SkeldShip(Clone)/Admin/Ground/admin_bridge/MapRoomConsole";
    public const string SkeldSecurity = "SkeldShip(Clone)/Security/Ground/map_surveillance/SurvConsole";

    public const string MiraHqAdmin = "MiraShip(Clone)/Admin/MapTable/AdminMapConsole";
    public const string MiraHqSecurity = "MiraShip(Clone)/Comms/comms-top/SurvLogConsole";

    public const string PolusAdmin1 = "PolusShip(Clone)/Admin/mapTable/panel_map";
    public const string PolusAdmin2 = "PolusShip(Clone)/Admin/mapTable/panel_map (1)";
    public const string PolusSecurity = "PolusShip(Clone)/Electrical/Surv_Panel";
    public const string PolusVital = "PolusShip(Clone)/Office/panel_vitals";

    public const string AirShipSecurity = "Airship(Clone)/Security/task_cams";
    public const string AirShipVital = "Airship(Clone)/Medbay/panel_vitals";
    public const string AirShipArchiveAdmin = "Airship(Clone)/Records/records_admin_map";
    public const string AirShipCockpitAdmin = "Airship(Clone)/Cockpit/panel_cockpit_map";

    private const string airShipSpawnJson =
        "ExtremeRoles.Resources.JsonData.AirShipSpawnPoint.json";
    private const string airShipKey = "VanillaRandomSpawn";

    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static HashSet<TaskTypes> SaboTask = new HashSet<TaskTypes>()
    {
        TaskTypes.FixLights,
        TaskTypes.RestoreOxy,
        TaskTypes.ResetReactor,
        TaskTypes.ResetSeismic,
        TaskTypes.FixComms,
        TaskTypes.StopCharles
    };

    private static HashSet<TaskTypes> ignoreTask = new HashSet<TaskTypes>()
    {
        TaskTypes.FixWiring,
        TaskTypes.VentCleaning,
    };

    private static GridArrange cachedArrange = null;

    private static List<PlayerControl> bots = new List<PlayerControl>();

    public static bool IsLobby
    {
        get
        {
            return (
                AmongUsClient.Instance.GameState !=
                InnerNet.InnerNetClient.GameStates.Started
            );
        }
    }
    public static bool IsFreePlay
    {
        get
        {
            return AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay;
        }
    }

    public static void ReGridButtons()
    {
        if (!FastDestroyableSingleton<HudManager>.Instance) { return; }

        if (cachedArrange == null)
        {
            var useButton = FastDestroyableSingleton<HudManager>.Instance.UseButton;
            cachedArrange = useButton.transform.parent.gameObject.GetComponent<GridArrange>();
        }
        cachedArrange.ArrangeChilds();
    }

    public static GameObject CreateNoneReportableDeadbody(
        PlayerControl targetPlayer, Vector3 pos)
    {
        var killAnimation = targetPlayer.KillAnimations[0];
        DeadBody deadbody = UnityEngine.Object.Instantiate(
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

        UnityEngine.Object.Destroy(deadbody);

        return body;
    }

    public static void DisableMapModule(string mapModuleName)
    {
        GameObject obj = GameObject.Find(mapModuleName);
        if (obj != null)
        {
            SetColliderActive(obj, false);
        }
    }

    public static void SetColliderActive(GameObject obj, bool active)
    {
        setColliderEnable<Collider2D>(obj, active);
        setColliderEnable<PolygonCollider2D>(obj, active);
        setColliderEnable<BoxCollider2D>(obj, active);
        setColliderEnable<CircleCollider2D>(obj, active);
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

        return (byte)taskIndex[index];
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
            CachedShipStatus.Instance.NormalTasks);

        int index = RandomGenerator.Instance.Next(taskIndex.Count);

        return taskIndex[index];
    }

    public static Sprite GetAdminButtonImage()
    {
        var imageDict = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;
        switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId))
        {
            case 0:
            case 3:
                return imageDict[ImageNames.AdminMapButton].Image;
            case 1:
                return imageDict[ImageNames.MIRAAdminButton].Image;
            case 2:
                return imageDict[ImageNames.PolusAdminButton].Image;
            default:
                return imageDict[ImageNames.AirshipAdminButton].Image;
        }
    }

    public static Sprite GetSecurityImage()
    {
        var imageDict = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings;
        switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId))
        {
            case 1:
                return imageDict[ImageNames.DoorLogsButton].Image;
            default:
                return imageDict[ImageNames.CamsButton].Image;
        }
    }
    public static Sprite GetVitalImage() => 
        FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[
            ImageNames.VitalsButton].Image;

    public static SystemConsole GetSecuritySystemConsole()
    {
        SystemConsole watchConsole;
        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            watchConsole = ExtremeRolesPlugin.Compat.ModMap.GetSystemConsole(
                SystemConsoleType.SecurityCamera);
        }
        else
        {
            watchConsole = getVanillaSecurityConsole();
        }
        return watchConsole;
    }
    public static SystemConsole GetVitalSystemConsole()
    {
        SystemConsole vitalConsole;
        if (ExtremeRolesPlugin.Compat.IsModMap)
        {
            vitalConsole = ExtremeRolesPlugin.Compat.ModMap.GetSystemConsole(
                SystemConsoleType.Vital);
        }
        else
        {
            vitalConsole = getVanillaVitalConsole();
        }
        return vitalConsole;
    }

    public static void ForceEndGame()
    {
        RPCOperator.Call(RPCOperator.Command.ForceEnd);
        RPCOperator.ForceEnd();
    }

    public static bool IsValidConsole(PlayerControl player, Console console)
    {
        if (player is null || console is null) { return false; }

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

    public static Minigame OpenMinigame(
        Minigame prefab,
        PlayerTask task = null,
        Console console = null)
    {
        Minigame minigame = UnityEngine.Object.Instantiate(
            prefab, Camera.main.transform, false);
        minigame.transform.SetParent(Camera.main.transform, false);
        minigame.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
        if (console != null)
        {
            minigame.Console = console;
        }
        minigame.Begin(task);

        return minigame;
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

            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                if (ExtremeRolesPlugin.Compat.ModMap.IsCustomSabotageTask(taskType))
                {
                    ExtremeRolesPlugin.Compat.ModMap.RpcRepairCustomSabotage(
                        taskType);
                    continue;
                }
            }
            switch (taskType)
            {
                case TaskTypes.FixLights:
                    RPCOperator.Call(
                        CachedPlayerControl.LocalPlayer.PlayerControl.NetId,
                        RPCOperator.Command.FixLightOff);
                    RPCOperator.FixLightOff();
                    break;
                case TaskTypes.RestoreOxy:
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.LifeSupp, 0 | 64);
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.LifeSupp, 1 | 64);
                    break;
                case TaskTypes.ResetReactor:
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Reactor, 16);
                    break;
                case TaskTypes.ResetSeismic:
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Laboratory, 16);
                    break;
                case TaskTypes.FixComms:
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Comms, 16 | 0);
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Comms, 16 | 1);
                    break;
                case TaskTypes.StopCharles:
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Reactor, 0 | 16);
                    CachedShipStatus.Instance.RpcRepairSystem(
                        SystemTypes.Reactor, 1 | 16);
                    break;
                default:
                    break;
            }
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
            var textTask = player.myTasks[i].gameObject.GetComponent<ImportantTextTask>();
            if (textTask != null) { continue; }

            if (SaboTask.Contains(player.myTasks[i].TaskType)) { continue; }
            if (ExtremeRolesPlugin.Compat.IsModMap)
            {
                if (ExtremeRolesPlugin.Compat.ModMap.IsCustomSabotageTask(
                        player.myTasks[i].TaskType)) { continue; }
            }

            if (player.myTasks[i].IsComplete)
            {
                NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(
                    addTask, player.transform);
                normalPlayerTask.Id = gameControlTaskId;
                normalPlayerTask.Owner = player;
                normalPlayerTask.Initialize();

                var removeTask = player.myTasks[i];
                player.myTasks[i] = normalPlayerTask;

                removeTask.OnRemove();
                UnityEngine.Object.Destroy(
                    removeTask.gameObject);
                if (player.PlayerId == CachedPlayerControl.LocalPlayer.PlayerId)
                {
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
        JObject json = JsonParser.GetJObjectFromAssembly(airShipSpawnJson);
        JArray airShipSpawn = json.Get<JArray>(airShipKey);

        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < airShipSpawn.Count; ++i)
        {
            JArray pos = airShipSpawn.Get<JArray>(i);
            result.Add(new Vector2((float)pos[0], (float)pos[1]));
        }

        return result;
    }

    public static void ShareVersion()
    {
        Version ver = Assembly.GetExecutingAssembly().GetName().Version;

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
        GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
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

    private static SystemConsole getVanillaSecurityConsole()
    {
        // 0 = Skeld
        // 1 = Mira HQ
        // 2 = Polus
        // 3 = Dleks - deactivated
        // 4 = Airship
        var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
        switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId))
        {
            case 0:
            case 3:
                return systemConsoleArray.FirstOrDefault(
                    x => x.gameObject.name.Contains("SurvConsole"));
            case 1:
                return systemConsoleArray.FirstOrDefault(
                    x => x.gameObject.name.Contains("SurvLogConsole"));
            case 2:
                return systemConsoleArray.FirstOrDefault(
                    x => x.gameObject.name.Contains("Surv_Panel"));
            case 4:
                return systemConsoleArray.FirstOrDefault(
                    x => x.gameObject.name.Contains("task_cams"));
            default:
                return null;
        }
    }

    private static SystemConsole getVanillaVitalConsole()
    {
        // 0 = Skeld
        // 1 = Mira HQ
        // 2 = Polus
        // 3 = Dleks - deactivated
        // 4 = Airship
        var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
        switch (GameOptionsManager.Instance.CurrentGameOptions.GetByte(
            ByteOptionNames.MapId))
        {
            case 0:
            case 1:
            case 3:
                return null;
            case 2:
            case 4:
                return systemConsoleArray.FirstOrDefault(
                    x => x.gameObject.name.Contains("panel_vitals"));
            default:
                return null;
        }
    }

    private static void destroyComponent<T>(GameObject obj) where T : Behaviour
    {
        T collider = obj.GetComponent<T>();
        if (collider != null)
        {
            UnityEngine.Object.Destroy(collider);
        }
    }

}
