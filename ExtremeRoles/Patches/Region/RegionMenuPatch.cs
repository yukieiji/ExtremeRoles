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

using UnityObject = UnityEngine.Object;
using UIButton = UnityEngine.UI.Button;
using Config = ExtremeRoles.OptionHolder.ConfigParser;

namespace ExtremeRoles.Patches.Region;

[HarmonyPatch(typeof(RegionMenu), nameof(RegionMenu.Open))]
public static class RegionMenuOpenPatch
{
    private static GameObject ipField;
    private static GameObject portField;

    private static TextMeshPro ipText;
    private static TextMeshPro portText;

    public static void Postfix(RegionMenu __instance)
    {
        var gameIdTextBox = GameObject.Find(
            "NormalMenu/JoinGameButton/JoinGameMenu/GameIdText");
        if (gameIdTextBox == null) { return; }

        var allButton = __instance.controllerSelectable;

        if (allButton == null) { return; }

        for (int i = 0; i < allButton.Count; ++i)
        {
            allButton[i].transform.localPosition =
                new Vector3(-2.0f, 2f - 0.5f * (float)i, 0f);
        }

        if (ipField == null || ipText == null)
        {
            ipField = UnityObject.Instantiate(
                gameIdTextBox.gameObject, __instance.transform);
            ipText = UnityObject.Instantiate(
                Module.Prefab.Text);

            ipField.gameObject.name = "ipTextBox";
            ipText.gameObject.name = "ipText";
            ipText.fontSize = ipText.fontSizeMin = ipText.fontSizeMax = 2.0f;

            var arrow = ipField.transform.FindChild("arrowEnter");
            if (arrow == null || arrow.gameObject == null) { return; }
            UnityObject.DestroyImmediate(arrow.gameObject);

            ipField.transform.localPosition = new Vector3(2.0f, 1.0f, -100f);

            var ipTextBox = ipField.GetComponent<TextBoxTMP>();

            ipTextBox.characterLimit = 30;
            ipTextBox.AllowSymbols = true;
            ipTextBox.ForceUppercase = false;
            ipTextBox.SetText(Config.Ip.Value);
            __instance.StartCoroutine(
                Effects.Lerp(0.1f, new Action<float>(
                    (p) =>
                    {
                        ipTextBox.outputText.SetText(Config.Ip.Value);
                        ipTextBox.SetText(Config.Ip.Value);
                    })));

            ipTextBox.ClearOnFocus = false;

            ipTextBox.OnEnter = ipTextBox.OnChange = ipTextBox.OnFocusLost = 
                new UIButton.ButtonClickedEvent();
            ipTextBox.OnChange.AddListener((UnityAction)onEnterOrIpChange);
            ipTextBox.OnFocusLost.AddListener((UnityAction)onFocusLost);

            ipText.text =  Helper.Translation.GetString(
                "customServerIp");
            ipText.font = ipTextBox.outputText.font;
            ipText.transform.SetParent(ipField.transform);
            ipText.transform.localPosition = new Vector3(-0.2f, 0.425f, -100f);
            ipText.gameObject.SetActive(true);
        }

        if (portField == null || portText == null)
        {
            portField = UnityObject.Instantiate(
                gameIdTextBox.gameObject, __instance.transform);
            portText = UnityObject.Instantiate(
                Module.Prefab.Text);

            portField.gameObject.name = "portTextBox";
            portText.gameObject.name = "portText";
            portText.fontSize = portText.fontSizeMin = portText.fontSizeMax = 2.0f;

            var arrow = portField.transform.FindChild("arrowEnter");
            if (arrow == null || arrow.gameObject == null) { return; }
            UnityObject.DestroyImmediate(arrow.gameObject);

            portField.transform.localPosition = new Vector3(2.0f, 0.0f, -100f);

            var portTextBox = portField.GetComponent<TextBoxTMP>();

            portTextBox.characterLimit = 5;
            portTextBox.SetText(Config.Port.Value.ToString());
            __instance.StartCoroutine(
                Effects.Lerp(0.1f, new Action<float>(
                    (p) =>
                    {
                        portTextBox.outputText.SetText(Config.Port.Value.ToString());
                        portTextBox.SetText(Config.Port.Value.ToString()); 
                    })));


            portTextBox.ClearOnFocus = false;
            portTextBox.OnEnter = portTextBox.OnChange = portTextBox.OnFocusLost = 
                new UIButton.ButtonClickedEvent();
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
            Config.Ip.Value = ipField.GetComponent<TextBoxTMP>().text;
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
                Config.Port.Value = port;
                portTextBox.outputText.color = Color.white;
            }
            else
            {
                portTextBox.outputText.color = Color.red;
            }
        }

    }
}