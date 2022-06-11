using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ExtremeRoles.Compat.Interface;

using BepInEx;

using HarmonyLib;
using UnhollowerRuntimeLib;
using Hazel;
using UnityEngine;

namespace ExtremeRoles.Compat.Mods
{
    public class Submerged : CompatModBase, IMapMod
    {
        public const string Guid = "Submerged";

        public ShipStatus.MapType MapType => (ShipStatus.MapType)5;
        public TaskTypes RetrieveOxygenMask;

        private Dictionary<string, Type> injectedTypes;

        private Type taskType;

        private Type submarineOxygenSystem;
        private PropertyInfo submarineOxygenSystemInstanceGetter;
        private MethodInfo submarineOxygenSystemRepairDamageMethod;

        private const string elevatorMover = "ElevatorMover";

        public Submerged(PluginInfo plugin) : base(Guid, plugin)
        {
            // カスタムサボのタスクタイプ取得
            taskType = ClassType.First(
                t => t.Name == "CustomTaskTypes");
            var retrieveOxigenMaskField = AccessTools.Field(taskType, "RetrieveOxygenMask");
            RetrieveOxygenMask = (TaskTypes)retrieveOxigenMaskField.GetValue(null);

            submarineOxygenSystemInstanceGetter = AccessTools.Property(
                submarineOxygenSystem, "Instance");
            submarineOxygenSystemRepairDamageMethod = AccessTools.Method(
                submarineOxygenSystem, "RepairDamage");

            injectedTypes = (Dictionary<string, Type>)AccessTools.PropertyGetter(
                ClassType.FirstOrDefault(t => t.Name == "RegisterInIl2CppAttribute"), "RegisteredTypes").Invoke(
                    null, Array.Empty<object>());

        }
        public void Awake()
        {
            Patches.HudManagerUpdatePatchPostfixPatch.ButtonTriggerReset();
        }

        public Console GetConsole(TaskTypes task)
        {
            throw new System.NotImplementedException();
        }

        public SystemConsole GetSystemConsole(SystemConsoleType sysConsole)
        {
            throw new System.NotImplementedException();
        }

        public bool IsCustomSabotageNow()
        {
            foreach (NormalPlayerTask task in PlayerControl.LocalPlayer.myTasks)
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
                ShipStatus.Instance.RpcRepairSystem((SystemTypes)130, 64);
                submarineOxygenSystemRepairDamageMethod.Invoke(
                    submarineOxygenSystemInstanceGetter.GetValue(null),
                    new object[] { PlayerControl.LocalPlayer, (byte)64 });
            }
        }

        public Sprite SystemConsoleUseSprite(SystemConsoleType sysConsole)
        {
            throw new System.NotImplementedException();
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
    }

    public class MissingSubmergedBehaviour : MonoBehaviour
    {
        static MissingSubmergedBehaviour() => ClassInjector.RegisterTypeInIl2Cpp<MissingSubmergedBehaviour>();
        public MissingSubmergedBehaviour(IntPtr ptr) : base(ptr) { }
    }

}
