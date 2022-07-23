using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using HarmonyLib;
using UnhollowerBaseLib;

using ExtremeRoles.Module;
using ExtremeRoles.Resources;


namespace ExtremeRoles.Patches.Option
{

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    public static class GameOptionsMenuStartPatch
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

            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) { return; }

            // Setup ExtreamRole tab
            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");
            var gameSettings = GameObject.Find("Game Settings");

            var (erSettings, erMenu) = createOptionSettingAndMenu(gameSettings, "ExtremeRoleSettings");
            var (erTab, tabHighlight) = createTab(roleTab, "ExtremeRoleTab", Path.TabLogo);

            gameTab.transform.position += Vector3.left * 0.5f;
            roleTab.transform.position += Vector3.left * 0.5f;
            erTab.transform.position += Vector3.right * 0.5f;

            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
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

            removeAllOption(erMenu);

            List<OptionBehaviour> erOptions = new List<OptionBehaviour>();

            var optionsList = OptionHolder.AllOption.Values.ToList();

            for (int i = 0; i < optionsList.Count; ++i)
            {
                IOption option = optionsList[i];
                if (option.Body == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, erMenu.transform);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.GetName();
                    stringOption.Value = stringOption.oldValue = option.CurSelection;
                    stringOption.ValueText.text = option.GetString();

                    option.SetOptionBehaviour(stringOption);
                }
                option.Body.gameObject.SetActive(true);
            }

            erMenu.Children = new Il2CppReferenceArray<OptionBehaviour>(erOptions.ToArray());
            erSettings.gameObject.SetActive(false);

            // Adapt task count for main options

            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if (commonTasksOption != null) { commonTasksOption.ValidRange = new FloatRange(0f, 4f); }

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if (shortTasksOption != null){ shortTasksOption.ValidRange = new FloatRange(0f, 23f); }

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if (longTasksOption != null) { longTasksOption.ValidRange = new FloatRange(0f, 15f); }

        }

        private static (GameObject, GameOptionsMenu) createOptionSettingAndMenu(
            GameObject template, string name)
        {
            GameObject setting = UnityEngine.Object.Instantiate(template, template.transform.parent);
            GameOptionsMenu menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            setting.name = name;

            return (setting, menu);
        }

        private static (GameObject, SpriteRenderer) createTab(GameObject template, string name, string imgPath)
        {
            GameObject tab = UnityEngine.Object.Instantiate(template, template.transform.parent);
            SpriteRenderer tabHighlight = tab.transform.FindChild(
                "Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            tab.name = name;
            tab.transform.FindChild("Hat Button").FindChild(
                "Icon").GetComponent<SpriteRenderer>().sprite = 
                    Loader.CreateSpriteFromResources(imgPath, 100f);

            return (tab, tabHighlight);
        }

        private static void removeAllOption(GameOptionsMenu menu)
        {
            // まずは元々入っているオプションを消す
            foreach (OptionBehaviour option in menu.GetComponentsInChildren<OptionBehaviour>())
            {
                UnityEngine.Object.Destroy(option.gameObject);
            }
        }

    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public static class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.transform.parent.parent.name.CompareTo("ExtremeRoleSettings") != 0) { return; }

            timer += Time.deltaTime;
            if (timer < 0.1f) { return; }
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;

            foreach (IOption option in OptionHolder.AllOption.Values)
            {
                if (option?.Body != null && option.Body.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.Parent;

                    if (AmongUsClient.Instance?.AmHost == false)
                    {
                        enabled = false;
                    }

                    enabled = option.IsActive();


                    option.Body.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.IsHeader ? 0.75f : 0.5f;
                        option.Body.transform.localPosition = new Vector3(
                            option.Body.transform.localPosition.x, offset,
                            option.Body.transform.localPosition.z);

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
            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -4.0f + numItems * 0.5f;
        }
    }
}
