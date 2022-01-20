// Adapted from https://github.com/MoltenMods/Unify
/*
MIT License

Copyright (c) 2021 Daemon

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using TMPro;

using UnityEngine;
using UnityEngine.Events;

namespace ExtremeRoles.Patches.Region
{
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.OnEnable))]
    public static class RegionMenuOnEnablePatch
    {
        public static bool Prefix(RegionMenu __instance)
        {
            __instance.controllerSelectable.Clear();
            int num = 0;
            foreach (IRegionInfo regionInfo in 
                from s in DestroyableSingleton<ServerManager>.Instance.AvailableRegions
                orderby (uint)GetDefaultRegionIndex(
                    ServerManager.DefaultRegions.ToList(),
                    (IRegionInfo d) => d.Name.Equals(s.Name))
                select s)
            {
                
                IRegionInfo region = regionInfo;
                ServerListButton serverListButton = __instance.ButtonPool.Get<ServerListButton>();
                serverListButton.transform.localPosition = new Vector3(0f, 2f - 0.5f * (float)num, 0f);
                serverListButton.SetTextTranslationId(regionInfo.TranslateName, regionInfo.Name);

                string printString;
                if (regionInfo.Name.Equals(CustomServer.Id))
                {
                    printString = Helper.Translation.GetString(
                        CustomServer.TranslationKey);
                }
                else
                {
                    printString = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                        regionInfo.TranslateName, regionInfo.Name, Array.Empty<Il2CppSystem.Object>());
                }

                serverListButton.Text.text = printString;
                serverListButton.Text.ForceMeshUpdate(false, false);
                serverListButton.Button.OnClick.RemoveAllListeners();
                serverListButton.Button.OnClick.AddListener((Action)(() =>
                {
                    __instance.ChooseOption(region);
                }));
                serverListButton.SetSelected(DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(regionInfo));
                if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Equals(regionInfo))
                {
                    __instance.defaultButtonSelected = serverListButton.Button;
                }
                __instance.controllerSelectable.Add(serverListButton.Button);
                ++num;
            }
            if (__instance.defaultButtonSelected == null && __instance.controllerSelectable.Count > 0)
            {
                __instance.defaultButtonSelected = __instance.controllerSelectable[0];
            }

            ControllerManager.Instance.OpenOverlayMenu(
                __instance.name, __instance.BackButton,
                __instance.defaultButtonSelected, __instance.controllerSelectable, false);
            ++num;

            return false;
        }

        private static int GetDefaultRegionIndex(
            List<IRegionInfo> list, Func<IRegionInfo, bool> pred)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (pred(list[i]))
                {
                    return i;
                }
            }
            return -1;
        }
    }
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.ChooseOption))]
    public static class RegionMenuChooseOptionPatch
    {
        public static bool Prefix(
            RegionMenu __instance,
            [HarmonyArgument(0)] IRegionInfo region)
        {
            DestroyableSingleton<ServerManager>.Instance.SetRegion(region);

            string printString;
            if (region.Name.Equals(CustomServer.Id))
            {
                printString = Helper.Translation.GetString(
                    CustomServer.TranslationKey);
            }
            else
            {
                printString = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                    region.TranslateName, region.Name, Array.Empty<Il2CppSystem.Object>());
            }

            __instance.RegionText.text = printString;
            __instance.Close();
            return false;
        }
    }

    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {

        private static TextBoxTMP ipField;
        private static TextBoxTMP portField;

        private static TextMeshPro ipText;
        private static TextMeshPro portText;

        public static void Postfix(RegionMenu __instance)
        {

            var template = DestroyableSingleton<JoinGameButton>.Instance;
            if (template == null || template.GameIdText == null) { return; }

            if (ipField == null || ipField.gameObject == null || ipText == null)
            {
                ipField = UnityEngine.Object.Instantiate(
                    template.GameIdText, __instance.transform);
                ipText = UnityEngine.Object.Instantiate(
                    Module.Prefab.Text);

                ipField.gameObject.name = "ipTextBox";
                ipText.gameObject.name = "ipText";

                var arrow = ipField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) { return; }
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                ipField.transform.localPosition = new Vector3(0.2f, -1f, -100f);
                ipField.characterLimit = 30;
                ipField.AllowSymbols = true;
                ipField.ForceUppercase = false;
                ipField.SetText(
                    OptionHolder.ConfigParser.Ip.Value);
                __instance.StartCoroutine(
                    Effects.Lerp(0.1f, new Action<float>(
                        (p) =>
                        {
                            ipField.outputText.SetText(OptionHolder.ConfigParser.Ip.Value);
                            ipField.SetText(OptionHolder.ConfigParser.Ip.Value);
                        })));

                ipField.ClearOnFocus = false; 
                ipField.OnEnter = ipField.OnChange = new UnityEngine.UI.Button.ButtonClickedEvent();
                ipField.OnFocusLost = new UnityEngine.UI.Button.ButtonClickedEvent();
                ipField.OnChange.AddListener((UnityAction)onEnterOrIpChange);
                ipField.OnFocusLost.AddListener((UnityAction)onFocusLost);

                ipText.text =  Helper.Translation.GetString(
                    "customServerIp");
                ipText.font = ipField.outputText.font;
                ipText.transform.parent = ipField.transform;
                ipText.transform.localPosition = new Vector3(-0.2f, 0.425f, -100f);
                ipText.gameObject.SetActive(true);

            }

            if (portField == null || portField.gameObject == null)
            {
                portField = UnityEngine.Object.Instantiate(
                    template.GameIdText, __instance.transform);
                portText = UnityEngine.Object.Instantiate(
                    Module.Prefab.Text);

                portField.gameObject.name = "portTextBox";
                portText.gameObject.name = "portText";

                var arrow = portField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) { return; }
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                portField.transform.localPosition = new Vector3(0.2f, -2.0f, -100f);
                portField.characterLimit = 5;
                portField.SetText(
                    OptionHolder.ConfigParser.Port.Value.ToString());
                __instance.StartCoroutine(
                    Effects.Lerp(0.1f, new Action<float>(
                        (p) =>
                        {
                            portField.outputText.SetText(OptionHolder.ConfigParser.Port.Value.ToString());
                            portField.SetText(OptionHolder.ConfigParser.Port.Value.ToString()); 
                        })));


                portField.ClearOnFocus = false;
                portField.OnEnter = portField.OnChange = new UnityEngine.UI.Button.ButtonClickedEvent();
                portField.OnFocusLost = new UnityEngine.UI.Button.ButtonClickedEvent();
                portField.OnChange.AddListener((UnityAction)onEnterOrPortFieldChange);
                portField.OnFocusLost.AddListener((UnityAction)onFocusLost);

                portText.text = Helper.Translation.GetString(
                    "customServerPort");
                portText.font = portField.outputText.font;
                portText.transform.parent = portField.transform;
                portText.transform.localPosition = new Vector3(-0.2f, 0.425f, -100f);
                portText.gameObject.SetActive(true);

            }

            void onEnterOrIpChange()
            {
                OptionHolder.ConfigParser.Ip.Value = ipField.text;
            }

            void onFocusLost()
            {
                OptionHolder.UpdateRegion();
                __instance.ChooseOption(
                    ServerManager.DefaultRegions[ServerManager.DefaultRegions.Length - 1]);
            }

            void onEnterOrPortFieldChange()
            {
                ushort port = 0;
                if (ushort.TryParse(portField.text, out port))
                {
                    OptionHolder.ConfigParser.Port.Value = port;
                    portField.outputText.color = Color.white;
                }
                else
                {
                    portField.outputText.color = Color.red;
                }
            }

        }
    }
}