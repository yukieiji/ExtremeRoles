using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;
using UnhollowerBaseLib;

using ExtremeRoles.Module;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameObject.Find("ExtremeRoleSettings") != null)
            { // Settings setup has already been performed, fixing the title of the tab and returning
                GameObject.Find(
                    "ExtremeRoleSettings").transform.FindChild(
                        "GameGroup").FindChild(
                            "Text").GetComponent<TMPro.TextMeshPro>().SetText(
                                Helper.Translation.GetString("ERSettings"));
                return;
            }

            // Setup ExtreamRole tab
            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            var erSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var erMenu = erSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            erSettings.name = "ExtremeRoleSettings";

            var erTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var tabHighlight = erTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            erTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = Helper.Resources.loadSpriteFromResources("TheOtherRoles.Resources.TabIcon.png", 100f);


            gameTab.transform.position += Vector3.left * 0.5f;
            roleTab.transform.position += Vector3.left * 0.5f;
            erTab.transform.position += Vector3.right * 0.5f;

            var tabs = new GameObject[] { gameTab, roleTab, erTab };
            for (int i = 0; i < tabs.Length; i++)
            {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) continue;
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    
                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    erSettings.gameObject.SetActive(false);

                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;
                    
                    tabHighlight.enabled = false;
                    
                    if (copiedIndex == 0)
                    {
                        gameSettingMenu.RegularGameSettings.SetActive(true);
                        gameSettingMenu.GameSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 1)
                    {
                        gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                        gameSettingMenu.RolesSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 2)
                    {
                        erSettings.gameObject.SetActive(true);
                        tabHighlight.enabled = true;
                    }
                }));
            }

            // まずは元々入っているオプションを消す
            foreach (OptionBehaviour option in erMenu.GetComponentsInChildren<OptionBehaviour>())
            {
                UnityEngine.Object.Destroy(option.gameObject);
            }

            List<OptionBehaviour> erOptions = new List<OptionBehaviour>();

            // ExtremeRolesPlugin.Instance.Log.LogInfo($"Option num: {OptionsHolder.AllOptions.Count}");

            var optionsList = OptionsHolder.AllOptions.Values.ToList();

            for (int i = 0; i < optionsList.Count(); ++i)
            {
                CustomOption option = optionsList[i];
                // ExtremeRolesPlugin.Instance.Log.LogInfo($"Option: {option.Behaviour == null}");
                if (option.Behaviour == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, erMenu.transform);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.Name;
                    stringOption.Value = stringOption.oldValue = option.CurSelection;
                    stringOption.ValueText.text = option.Selections[option.CurSelection].ToString();

                    option.Behaviour = stringOption;
                }
                option.Behaviour.gameObject.SetActive(true);

            }

            erMenu.Children = new Il2CppReferenceArray<OptionBehaviour>(erOptions.ToArray());
            erSettings.gameObject.SetActive(false);

            // Adapt task count for main options

            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);

        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.transform.parent.parent.name.CompareTo("ExtremeRoleSettings") != 0) return;

            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;

            foreach (CustomOption option in OptionsHolder.AllOptions.Values)
            {
                if (option?.Behaviour != null && option.Behaviour.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.Parent;

                    if (AmongUsClient.Instance?.AmHost == false)
                    {
                        enabled = false;
                    }

                    if (option.IsHidden)
                    {
                        enabled = false;
                    }

                    while (parent != null && enabled)
                    {
                        enabled = parent.Enabled;
                        parent = parent.Parent;
                    }

                    option.Behaviour.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.IsHeader ? 0.75f : 0.5f;
                        option.Behaviour.transform.localPosition = new Vector3(
                            option.Behaviour.transform.localPosition.x, offset,
                            option.Behaviour.transform.localPosition.z);

                        if (option.IsHeader)
                        {
                            numItems += 0.5f;
                        }
                    }
                    else
                    {
                        numItems--;
                    }
                }
            }
            __instance.GetComponentInParent<Scroller>().YBounds.max = -4.0f + numItems * 0.5f;
        }
    }
}
