using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Helper;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using BepInEx;

using HarmonyLib;
using UnhollowerRuntimeLib;
using Hazel;
using UnityEngine;

namespace ExtremeRoles.Compat.Mods
{
    public sealed class SubmergedMap : CompatModBase, IMultiFloorModMap
    {
        public const string Guid = "Submerged";

        public ShipStatus.MapType MapType => (ShipStatus.MapType)5;
        public bool CanPlaceCamera => false;
        public bool IsCustomCalculateLightRadius => true;

        public TaskTypes RetrieveOxygenMask;

        private Dictionary<string, Type> injectedTypes;

        private Type taskType;

        private Type submarineOxygenSystem;
        private PropertyInfo submarineOxygenSystemInstanceGetter;
        private MethodInfo submarineOxygenSystemRepairDamageMethod;

        private MethodInfo getFloorHandlerInfo;
        private MethodInfo rpcRequestChangeFloorMethod;
        private FieldInfo onUpperField;

        private FieldInfo inTransitionField;

        private Type submarineStatusType;
        private MethodInfo calculateLightRadiusMethod;
        private MonoBehaviour submarineStatus;

        private const string elevatorMover = "ElevatorMover";

        private float crewVison;
        private float impostorVison;

        public SubmergedMap(PluginInfo plugin) : base(Guid, plugin)
        {
            // カスタムサボのタスクタイプ取得
            taskType = ClassType.First(
                t => t.Name == "CustomTaskTypes");
            var retrieveOxigenMaskField = AccessTools.Field(taskType, "RetrieveOxygenMask");
            RetrieveOxygenMask = (TaskTypes)retrieveOxigenMaskField.GetValue(null);

            // サブマージドの酸素妨害の修理用
            submarineOxygenSystemInstanceGetter = AccessTools.Property(
                submarineOxygenSystem, "Instance");
            submarineOxygenSystemRepairDamageMethod = AccessTools.Method(
                submarineOxygenSystem, "RepairDamage");

            // サブマージドのカスタムMonoを取ってくる
            injectedTypes = (Dictionary<string, Type>)AccessTools.PropertyGetter(
                ClassType.FirstOrDefault(
                    t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes").Invoke(
                        null, Array.Empty<object>());

            // フロアを変える用
            Type floorHandlerType = ClassType.First(t => t.Name == "FloorHandler");
            getFloorHandlerInfo = AccessTools.Method(
                floorHandlerType, "GetFloorHandler", new Type[] { typeof(PlayerControl) });
            rpcRequestChangeFloorMethod = AccessTools.Method(
                floorHandlerType, "RpcRequestChangeFloor");
            onUpperField = AccessTools.Field(floorHandlerType, "OnUpper");

            submarineStatusType = ClassType.First(
                t => t.Name == "SubmarineStatus");
            calculateLightRadiusMethod = AccessTools.Method(
                submarineStatusType, "CalculateLightRadius");


            Type ventMoveToVentPatchType = ClassType.First(t => t.Name == "Vent_MoveToVent_Patch");
            inTransitionField = AccessTools.Field(ventMoveToVentPatchType, "InTransition");
        }
        public void Awake(ShipStatus map)
        {
            Patches.HudManagerUpdatePatchPostfixPatch.ButtonTriggerReset();
            submarineStatus = map.GetComponent(
                Il2CppType.From(submarineStatusType))?.TryCast(submarineStatusType) as MonoBehaviour;
            
            // 毎回毎回取得すると重いのでキャッシュ化
            crewVison = PlayerControl.GameOptions.CrewLightMod;
            impostorVison = PlayerControl.GameOptions.ImpostorLightMod;
        }

        public void Destroy()
        {
            submarineStatus = null;

            // バグってるかもしれないのでもとに戻しとく
            PlayerControl.GameOptions.CrewLightMod = crewVison;
            PlayerControl.GameOptions.ImpostorLightMod = impostorVison;
        }

        public float CalculateLightRadius(GameData.PlayerInfo player, bool neutral, bool neutralImpostor)
        {
            return (float)calculateLightRadiusMethod.Invoke(
                submarineStatus, new object[] { null, neutral, neutralImpostor });
        }

        public float CalculateLightRadius(
            GameData.PlayerInfo player, float visonMod, bool applayVisonEffects = true)
        {
            // サブマージドの視界計算のロジックは「クルーだと停電効果受ける、インポスターだと受けないので」
            // 1. まずはデフォルトの視界をMOD側で用意した視界の広さにリプレイス
            // 2. 視界効果を受けるかをインポスターかどうかで渡して計算
            // 3. 元の視界の広さに戻す

            PlayerControl.GameOptions.CrewLightMod = visonMod;
            PlayerControl.GameOptions.ImpostorLightMod = visonMod;

            float result = CalculateLightRadius(player, true, !applayVisonEffects);
            
            PlayerControl.GameOptions.CrewLightMod = crewVison;
            PlayerControl.GameOptions.ImpostorLightMod = impostorVison;
            
            return result;
        }

        public int GetLocalPlayerFloor() => GetFloor(CachedPlayerControl.LocalPlayer);
        public int GetFloor(PlayerControl player)
        {
            MonoBehaviour floorHandler = getFloorHandler(player);
            if (floorHandler == null) { return int.MaxValue; }
            bool isUp = (bool)onUpperField.GetValue(floorHandler);
            return isUp ? 1 : 0;
        }
        public void ChangeLocalPlayerFloor(int floor)
        {
            ChangeFloor(CachedPlayerControl.LocalPlayer, floor);
        }
        public void ChangeFloor(PlayerControl player, int floor)
        {
            if (floor > 1) { return; }
            MonoBehaviour floorHandler = getFloorHandler(player);
            if (floorHandler == null) { return; }
            rpcRequestChangeFloorMethod.Invoke(floorHandler, new object[] { floor == 1 });
        }

        public Console GetConsole(TaskTypes task)
        {
            var console = UnityEngine.Object.FindObjectsOfType<Console>();
            switch (task)
            {
                case TaskTypes.FixLights:
                    return console.FirstOrDefault(
                        x => x.gameObject.name.Contains("LightsConsole"));
                case TaskTypes.StopCharles:
                    List<Console> res = new List<Console>(2);
                    Console leftConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("BallastConsole_1"));
                    if (leftConsole != null)
                    {
                        res.Add(leftConsole);
                    }
                    Console rightConsole = console.FirstOrDefault(
                        x => x.gameObject.name.Contains("BallastConsole_2"));
                    if (rightConsole != null)
                    {
                        res.Add(rightConsole);
                    }
                    return res[RandomGenerator.Instance.Next(res.Count)];
                default:
                    return null;
            }
        }

        public List<Vector2> GetSpawnPos(byte playerId)
        {
            ShipStatus ship = CachedShipStatus.Instance;
            Vector2 baseVec = Vector2.up;
            baseVec = baseVec.Rotate(
                (float)(playerId - 1) * (360f / (float)GameData.Instance.AllPlayers.Count));
            Vector2 defaultSpawn = ship.InitialSpawnCenter + 
                baseVec * ship.SpawnRadius + new Vector2(0f, 0.3636f);

            List<Vector2> spawnPos = new List<Vector2>();
            spawnPos.Add(defaultSpawn);
            spawnPos.Add(defaultSpawn + new Vector2(0.0f, 48.119f));

            return spawnPos;
        }

        public HashSet<string> GetSystemObjectName(SystemConsoleType sysConsole)
        {
            switch (sysConsole)
            {
                case SystemConsoleType.Admin:
                    return new HashSet<string>()
                    {
                        "Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable",
                        "Submerged(Clone)/TopFloor/Adm-Obsv-Loun-MR/TaskConsoles/console-adm-admintable (1)",
                    };
                case SystemConsoleType.Vital:
                    return new HashSet<string>()
                    {
                        "Submerged(Clone)/panel_vitals(Clone)",
                    };
                case SystemConsoleType.SecurityCamera:
                    return new HashSet<string>()
                    {
                        "Submerged(Clone)/BottomFloor/Engines-Security/TaskConsoles/SecurityConsole",
                    };
                default:
                    return new HashSet<string>();
            }
        }

        public SystemConsole GetSystemConsole(SystemConsoleType sysConsole)
        {
            var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
            switch (sysConsole)
            {
                case SystemConsoleType.SecurityCamera:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SecurityConsole"));
                case SystemConsoleType.Vital:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("panel_vitals(Clone)"));
                case SystemConsoleType.EmergencyButton:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("console-mr-callmeeting"));
                default:
                    return null;
            }
        }

        public bool IsCustomSabotageNow()
        {
            foreach (NormalPlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
            {
                if (task != null && IsCustomSabotageTask(task.TaskType))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCustomSabotageTask(TaskTypes saboTask) => saboTask == this.RetrieveOxygenMask;

        public bool IsCustomVentUse(Vent vent)
        {
            switch (vent.Id)
            {
                case 9:  // Cannot enter vent 9 (Engine Room Exit Only Vent)!
                    if (CachedPlayerControl.LocalPlayer.PlayerControl.inVent)
                    { 
                        return false; 
                    }
                    return true;
                case 0:
                case 14: // Lower and Upper Central
                    return true;
                default:
                    return false;
            }
        }

        public (float, bool, bool) IsCustomVentUseResult(
            Vent vent, GameData.PlayerInfo player, bool isVentUse)
        {
            if ((bool)inTransitionField.GetValue(null))
            {
                return (float.MaxValue, false, false);
            }
            switch (vent.Id)
            {
                case 0:
                case 14: // Lower and Upper Central
                    float result = float.MaxValue;
                    bool couldUse = isVentUse && !player.IsDead && (player.Object.CanMove || player.Object.inVent);
                    bool canUse = couldUse;
                    if (canUse)
                    {
                        Vector3 center = player.Object.Collider.bounds.center;
                        Vector3 position = vent.transform.position;
                        result = Vector2.Distance(center, position);
                        canUse &= result <= vent.UsableDistance;
                    }
                    return (result, canUse, couldUse);
                default:
                    return (float.MaxValue, false, false);
            }
        }

        public void RpcRepairCustomSabotage()
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.IntegrateModCall,
                Hazel.SendOption.Reliable, -1);
            writer.Write(IMapMod.RpcCallType);
            writer.Write((byte)MapRpcCall.RepairAllSabo);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RepairCustomSabotage();
        }

        public void RpcRepairCustomSabotage(TaskTypes saboTask)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                PlayerControl.LocalPlayer.NetId,
                (byte)RPCOperator.Command.IntegrateModCall,
                Hazel.SendOption.Reliable, -1);
            writer.Write(IMapMod.RpcCallType);
            writer.Write((byte)MapRpcCall.RepairAllSabo);
            writer.Write((int)saboTask);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RepairCustomSabotage(saboTask);
        }

        public void RepairCustomSabotage()
        {
            RepairCustomSabotage(this.RetrieveOxygenMask);
        }

        public void RepairCustomSabotage(TaskTypes saboTask)
        {
            if (saboTask == this.RetrieveOxygenMask)
            {
                CachedShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
                submarineOxygenSystemRepairDamageMethod.Invoke(
                    submarineOxygenSystemInstanceGetter.GetValue(null),
                    new object[] { PlayerControl.LocalPlayer, (byte)64 });
            }
        }
        public void AddCustomComponent(
            GameObject addObject, CustomMonoBehaviourType customMonoType)
        {
            switch (customMonoType)
            {
                case CustomMonoBehaviourType.MovableFloorBehaviour:
                    bool validType = injectedTypes.TryGetValue(elevatorMover, out Type type);
                    if (validType)
                    {
                        addObject.AddComponent(Il2CppType.From(type)).TryCast<MonoBehaviour>();
                    }
                    else
                    {
                        addObject.AddComponent<MissingSubmergedBehaviour>();
                    }
                    break;
                default:
                    break;

            }
        }

        public void SetUpNewCamera(SurvCamera camera)
        {
            var fixConsole = camera.transform.FindChild("FixConsole");
            if (fixConsole != null)
            {
                var boxCollider = fixConsole.GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    UnityEngine.Object.Destroy(boxCollider);
                }
            }
        }

        protected override void PatchAll(Harmony harmony)
        {
            Type exileCont = ClassType.First(
                t => t.Name == "SubmergedExileController");
            MethodInfo wrapUpAndSpawn = AccessTools.Method(
                exileCont, "WrapUpAndSpawn");
            ExileController cont = null;
            MethodInfo wrapUpAndSpawnPrefix = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Prefix(cont));
            MethodInfo wrapUpAndSpawnPostfix = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmergedExileControllerWrapUpAndSpawnPatch.Postfix(cont));

            System.Collections.IEnumerator enumerator = null;
            Type submarineSelectSpawn = ClassType.First(
                t => t.Name == "SubmarineSelectSpawn");
            MethodInfo prespawnStep = AccessTools.Method(
                submarineSelectSpawn, "PrespawnStep");
            MethodInfo prespawnStepPrefix = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineSelectSpawnPrespawnStepPatch.Prefix(ref enumerator));

            MethodInfo onDestroy = AccessTools.Method(
                submarineSelectSpawn, "OnDestroy");
            MethodInfo onDestroyPrefix = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineSelectOnDestroyPatch.Prefix());

            Type hudManagerUpdatePatch = ClassType.First(
                t => t.Name == "HudManager_Update_Patch");
            MethodInfo hudManagerUpdatePatchPostfix = AccessTools.Method(
                hudManagerUpdatePatch, "Postfix");
            object hudManagerUpdatePatchInstance = null;
            Patches.HudManagerUpdatePatchPostfixPatch.SetType(
                hudManagerUpdatePatch);
            MethodInfo hubManagerUpdatePatchPostfixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.HudManagerUpdatePatchPostfixPatch.Postfix(
                    hudManagerUpdatePatchInstance));

            this.submarineOxygenSystem = ClassType.First(
                t => t.Name == "SubmarineOxygenSystem");
            MethodInfo submarineOxygenSystemDetoriorate = AccessTools.Method(
                submarineOxygenSystem, "Detoriorate");
            object submarineOxygenSystemInstance = null;
            Patches.SubmarineOxygenSystemDetorioratePatch.SetType(this.submarineOxygenSystem);
            MethodInfo submarineOxygenSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineOxygenSystemDetorioratePatch.Postfix(
                    submarineOxygenSystemInstance));

            Minigame game = null;

            Type submarineSurvillanceMinigame = ClassType.First(
                t => t.Name == "SubmarineSurvillanceMinigame");
            MethodInfo submarineSurvillanceMinigameSystemUpdate = AccessTools.Method(
                submarineSurvillanceMinigame, "Update");
            Patches.SubmarineSurvillanceMinigamePatch.SetType(submarineSurvillanceMinigame);
            MethodInfo submarineSurvillanceMinigameSystemUpdatePrefixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineSurvillanceMinigamePatch.Prefix(game));
            MethodInfo submarineSurvillanceMinigameSystemUpdatePostfixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineSurvillanceMinigamePatch.Postfix(game));


            // 会議終了時のリセット処理を呼び出せるように
            harmony.Patch(wrapUpAndSpawn,
                new HarmonyMethod(wrapUpAndSpawnPrefix),
                new HarmonyMethod(wrapUpAndSpawnPostfix));
            
            // アサシン会議発動するとスポーン画面が出ないように
            harmony.Patch(prespawnStep,
                new HarmonyMethod(prespawnStepPrefix));

            // キルクール周りが上書きされているのでそれの調整
            harmony.Patch(onDestroy,
                new HarmonyMethod(onDestroyPrefix));

            // フロアの階層変更ボタンの位置を変えるパッチ
            harmony.Patch(hudManagerUpdatePatchPostfix,
                postfix : new HarmonyMethod(hubManagerUpdatePatchPostfixPatch));

            // 酸素枯渇発動時アサシンは常にマスクを持つパッチ
            harmony.Patch(submarineOxygenSystemDetoriorate,
                postfix: new HarmonyMethod(submarineOxygenSystemDetorioratePostfixPatch));

            // サブマージドのセキュリティカメラの制限をつけるパッチ
            harmony.Patch(submarineSurvillanceMinigameSystemUpdate,
                new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePrefixPatch),
                new HarmonyMethod(submarineSurvillanceMinigameSystemUpdatePostfixPatch));
        }

        private MonoBehaviour getFloorHandler(PlayerControl player) => ((Component)getFloorHandlerInfo.Invoke(
                null, new object[] { player })) as MonoBehaviour;
    }

    public class MissingSubmergedBehaviour : MonoBehaviour
    {
        static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
        public MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }

}
