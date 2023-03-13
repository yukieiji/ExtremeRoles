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

using HarmonyLib;

using TMPro;

using UnityEngine;
using UnityEngine.Events;

namespace ExtremeRoles.Patches.Region
{
    [HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
    public static class RegionMenuOpenPatch
    {

        private static GameObject ipField;
        private static GameObject portField;

        private static TextMeshPro ipText;
        private static TextMeshPro portText;

        public static void Postfix(RegionMenu __instance)
        {

            var gameIdTextBox = GameObject.Find("NormalMenu/JoinGameButton/JoinGameMenu/GameIdText");
            if (gameIdTextBox is null) { return; }

            if (ipField is null || ipField.gameObject is null || ipText is null)
            {
                ipField = UnityEngine.Object.Instantiate(
                    gameIdTextBox.gameObject, __instance.transform);
                ipText = UnityEngine.Object.Instantiate(
                    Module.Prefab.Text);

                ipField.gameObject.name = "ipTextBox";
                ipText.gameObject.name = "ipText";
                ipText.fontSize = ipText.fontSizeMin = ipText.fontSizeMax = 2.0f;

                var arrow = ipField.transform.FindChild("arrowEnter");
                if (arrow == null || arrow.gameObject == null) { return; }
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                ipField.transform.localPosition = new Vector3(0.2f, -1f, -100f);

                var ipTextBox = ipField.GetComponent<TextBoxTMP>();

                ipTextBox.characterLimit = 30;
                ipTextBox.AllowSymbols = true;
                ipTextBox.ForceUppercase = false;
                ipTextBox.SetText(
                    OptionHolder.ConfigParser.Ip.Value);
                __instance.StartCoroutine(
                    Effects.Lerp(0.1f, new Action<float>(
                        (p) =>
                        {
                            ipTextBox.outputText.SetText(OptionHolder.ConfigParser.Ip.Value);
                            ipTextBox.SetText(OptionHolder.ConfigParser.Ip.Value);
                        })));

                ipTextBox.ClearOnFocus = false;
                ipTextBox.OnEnter = ipTextBox.OnChange = new UnityEngine.UI.Button.ButtonClickedEvent();
                ipTextBox.OnFocusLost = new UnityEngine.UI.Button.ButtonClickedEvent();
                ipTextBox.OnChange.AddListener((UnityAction)onEnterOrIpChange);
                ipTextBox.OnFocusLost.AddListener((UnityAction)onFocusLost);

                ipText.text =  Helper.Translation.GetString(
                    "customServerIp");
                ipText.font = ipTextBox.outputText.font;
                ipText.transform.SetParent(ipField.transform);
                ipText.transform.localPosition = new Vector3(-0.2f, 0.425f, -100f);
                ipText.gameObject.SetActive(true);

            }

            if (portField is null || portField.gameObject is null)
            {
                portField = UnityEngine.Object.Instantiate(
                    gameIdTextBox.gameObject, __instance.transform);
                portText = UnityEngine.Object.Instantiate(
                    Module.Prefab.Text);

                portField.gameObject.name = "portTextBox";
                portText.gameObject.name = "portText";
                portText.fontSize = portText.fontSizeMin = portText.fontSizeMax = 2.0f;

                var arrow = portField.transform.FindChild("arrowEnter");
                if (arrow is null || arrow.gameObject is null) { return; }
                UnityEngine.Object.DestroyImmediate(arrow.gameObject);

                portField.transform.localPosition = new Vector3(0.2f, -2.0f, -100f);

                var portTextBox = portField.GetComponent<TextBoxTMP>();

                portTextBox.characterLimit = 5;
                portTextBox.SetText(
                    OptionHolder.ConfigParser.Port.Value.ToString());
                __instance.StartCoroutine(
                    Effects.Lerp(0.1f, new Action<float>(
                        (p) =>
                        {
                            portTextBox.outputText.SetText(OptionHolder.ConfigParser.Port.Value.ToString());
                            portTextBox.SetText(OptionHolder.ConfigParser.Port.Value.ToString()); 
                        })));


                portTextBox.ClearOnFocus = false;
                portTextBox.OnEnter = portTextBox.OnChange = new UnityEngine.UI.Button.ButtonClickedEvent();
                portTextBox.OnFocusLost = new UnityEngine.UI.Button.ButtonClickedEvent();
                portTextBox.OnChange.AddListener((UnityAction)onEnterOrPortFieldChange);
                portTextBox.OnFocusLost.AddListener((UnityAction)onFocusLost);

                portText.text = Helper.Translation.GetString(
                    "customServerPort");
                portText.font = portTextBox.outputText.font;
                portText.transform.SetParent(portField.transform);
                portText.transform.localPosition = new Vector3(-0.2f, 0.425f, -100f);
                portText.gameObject.SetActive(true);

            }

            void onEnterOrIpChange()
            {
                OptionHolder.ConfigParser.Ip.Value = ipField.GetComponent<TextBoxTMP>().text;
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

                var portTextBox = portField.GetComponent<TextBoxTMP>();

                if (ushort.TryParse(portTextBox.text, out port))
                {
                    OptionHolder.ConfigParser.Port.Value = port;
                    portTextBox.outputText.color = Color.white;
                }
                else
                {
                    portTextBox.outputText.color = Color.red;
                }
            }

        }
    }
}