using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ExtremeRoles.Compat.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

using BepInEx;

using HarmonyLib;
using UnhollowerRuntimeLib;
using Hazel;
using UnityEngine;

namespace ExtremeRoles.Compat.Mods
{
    public class SubmergedMap : CompatModBase, IMultiFloorModMap
    {
        public const string Guid = "Submerged";

        public ShipStatus.MapType MapType => (ShipStatus.MapType)5;
        public bool CanPlaceCamera => false;

        public TaskTypes RetrieveOxygenMask;

        private Dictionary<string, Type> injectedTypes;

        private Type taskType;

        private Type submarineOxygenSystem;
        private PropertyInfo submarineOxygenSystemInstanceGetter;
        private MethodInfo submarineOxygenSystemRepairDamageMethod;

        private MethodInfo getFloorHandlerInfo;
        private MethodInfo rpcRequestChangeFloorMethod;
        private FieldInfo onUpperField;

        private const string elevatorMover = "ElevatorMover";

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

        }
        public void Awake()
        {
            Patches.HudManagerUpdatePatchPostfixPatch.ButtonTriggerReset();
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

        public SystemConsole GetSystemConsole(SystemConsoleType sysConsole)
        {
            var systemConsoleArray = UnityEngine.Object.FindObjectsOfType<SystemConsole>();
            switch (sysConsole)
            {
                case SystemConsoleType.SecurityCamera:
                    return systemConsoleArray.FirstOrDefault(
                        x => x.gameObject.name.Contains("SecurityConsole"));
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


            Type exileControllerBeginPatch = ClassType.First(
                t => t.Name == "ExileController_Begin_Patch");
            MethodInfo exileControllerBeginPatchPrefix = AccessTools.Method(
                exileControllerBeginPatch, "Prefix");
            object exileControllerBeginPatchInstance = null;
            Patches.ExileControllerBeginPrefixPatch.SetType(
                exileControllerBeginPatch);
            MethodInfo exileControllerBeginPatchPrefixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.ExileControllerBeginPrefixPatch.Postfix(
                    exileControllerBeginPatchInstance));

            this.submarineOxygenSystem = ClassType.First(
                t => t.Name == "SubmarineOxygenSystem");
            MethodInfo submarineOxygenSystemDetoriorate = AccessTools.Method(
                submarineOxygenSystem, "Detoriorate");
            object submarineOxygenSystemInstance = null;
            Patches.SubmarineOxygenSystemDetorioratePatch.SetType(this.submarineOxygenSystem);
            MethodInfo submarineOxygenSystemDetorioratePostfixPatch = SymbolExtensions.GetMethodInfo(
                () => Patches.SubmarineOxygenSystemDetorioratePatch.Postfix(
                    submarineOxygenSystemInstance));


            // 会議終了時のリセット処理を呼び出せるように
            harmony.Patch(wrapUpAndSpawn,
                new HarmonyMethod(wrapUpAndSpawnPrefix),
                new HarmonyMethod(wrapUpAndSpawnPostfix));
            
            // アサシン会議発動するとスポーン画面が出ないように
            harmony.Patch(prespawnStep,
                new HarmonyMethod(prespawnStepPrefix));

            // フロアの階層変更ボタンの位置を変えるパッチ
            harmony.Patch(hudManagerUpdatePatchPostfix,
                postfix : new HarmonyMethod(hubManagerUpdatePatchPostfixPatch));

            // アサシン会議終了後のテキスト表示を変えるパッチ
            harmony.Patch(exileControllerBeginPatchPrefix,
                postfix: new HarmonyMethod(exileControllerBeginPatchPrefixPatch));

            // 酸素枯渇発動時アサシンは常にマスクを持つパッチ
            harmony.Patch(submarineOxygenSystemDetoriorate,
                postfix: new HarmonyMethod(submarineOxygenSystemDetorioratePostfixPatch));
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
