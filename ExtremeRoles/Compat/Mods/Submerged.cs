using System;
using System.Linq;
using System.Reflection;

using ExtremeRoles.Compat.Interface;

using BepInEx;

using HarmonyLib;
using UnityEngine;

namespace ExtremeRoles.Compat.Mods
{
    public class Submerged : CompatModBase, IMapMod 
    {
        public const string Guid = "Submerged";

        public ShipStatus.MapType MapType => (ShipStatus.MapType)5;

        public Submerged(PluginInfo plugin) : base(Guid, plugin)
        {

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
            throw new System.NotImplementedException();
        }

        public bool IsCustomSabotageTask(TaskTypes saboTask)
        {
            throw new System.NotImplementedException();
        }

        public void RepairCustomSabotage()
        {
            throw new System.NotImplementedException();
        }

        public void RepairCustomSabotage(TaskTypes saboTask)
        {
            throw new System.NotImplementedException();
        }

        public Sprite SystemConsoleUseSprite(SystemConsoleType sysConsole)
        {
            throw new System.NotImplementedException();
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


            Type submarineOxygenSystem = ClassType.First(
                t => t.Name == "SubmarineOxygenSystem");
            MethodInfo submarineOxygenSystemDetoriorate = AccessTools.Method(
                submarineOxygenSystem, "Detoriorate");
            object submarineOxygenSystemInstance = null;
            Patches.SubmarineOxygenSystemDetorioratePatch.SetType(
                submarineOxygenSystem);
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
}
