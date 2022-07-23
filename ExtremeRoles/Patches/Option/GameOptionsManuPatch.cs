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
        public const string GeneralSetting = "ExtremeRoleSettings";
        public const string CrewmateSetting = "ExtremeCrewmateSettings";
        public const string ImpostorSetting = "ExtremeImpostorSettings";
        public const string NeutralSetting = "ExtremeNeutralSettings";
        public const string CombinationSetting = "ExtremeCombinationSettings";

        public static void Postfix(GameOptionsMenu __instance)
        {
            if (isFindAndTrans(GeneralSetting, "ERSettings")) { return; }
            if (isFindAndTrans(CrewmateSetting, "ERSettings")) { return; }
            if (isFindAndTrans(ImpostorSetting, "ERSettings")) { return; }
            if (isFindAndTrans(NeutralSetting, "ERSettings")) { return; }
            if (isFindAndTrans(CombinationSetting, "ERSettings")) { return; }

            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) { return; }

            // Setup ExtreamRole tab
            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");
            var gameSettings = GameObject.Find("Game Settings"); 

            var (erSettings, erMenu) = createOptionSettingAndMenu(gameSettings, GeneralSetting);
            var (erTab, tabHighlight) = createTab(roleTab, roleTab.transform.parent, "ExtremeRoleTab", Path.TabLogo);

            var (crewSettings, crewMenu) = createOptionSettingAndMenu(gameSettings, CrewmateSetting);
            var (crewTab, crewTabHighlight) = createTab(roleTab, erTab.transform, "ExtremeRoleTab", Path.TabLogo);
            var (impostorSettings, impostorMenu) = createOptionSettingAndMenu(gameSettings, ImpostorSetting);
            var (impostorTab, impostorTabHighlight) = createTab(roleTab, crewTab.transform, "ExtremeRoleTab", Path.TabLogo);
            var (neutralSettings, neutralMenu) = createOptionSettingAndMenu(gameSettings, NeutralSetting);
            var (neutralTab, neutralTabHighlight) = createTab(roleTab, impostorTab.transform, "ExtremeRoleTab", Path.TabLogo);
            var (combinationSettings, combinationMenu) = createOptionSettingAndMenu(gameSettings, CombinationSetting);
            var (combinationTab, combinationTabHighlight) = createTab(roleTab, neutralTab.transform, "ExtremeRoleTab", Path.TabLogo);

            gameTab.transform.position += Vector3.left * 3f;
            roleTab.transform.position += Vector3.left * 3f;
            erTab.transform.position += Vector3.left * 2f;
            crewTab.transform.localPosition = Vector3.right * 1f;
            impostorTab.transform.localPosition = Vector3.right * 1f;
            neutralTab.transform.localPosition = Vector3.right * 1f;
            combinationTab.transform.localPosition = Vector3.right * 1f;

            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            
            var tabs = new GameObject[]
            { 
                gameTab,
                roleTab,
                erTab,
                crewTab,
                impostorTab,
                neutralTab,
                combinationTab,
            };
            for (int i = 0; i < tabs.Length; i++)
            {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) { continue; }
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {

                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    
                    erSettings.gameObject.SetActive(false);
                    crewSettings.gameObject.SetActive(false);
                    impostorSettings.gameObject.SetActive(false);
                    neutralSettings.gameObject.SetActive(false);
                    combinationSettings.gameObject.SetActive(false);

                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;

                    tabHighlight.enabled = false;
                    crewTabHighlight.enabled = false;
                    impostorTabHighlight.enabled = false;
                    neutralTabHighlight.enabled = false;
                    combinationTabHighlight.enabled = false;

                    switch (copiedIndex)
                    {
                        case 0:
                            gameSettingMenu.RegularGameSettings.SetActive(true);
                            gameSettingMenu.GameSettingsHightlight.enabled = true;
                            break;
                        case 1:
                            gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                            gameSettingMenu.RolesSettingsHightlight.enabled = true;
                            break;
                        case 2:
                            erSettings.gameObject.SetActive(true);
                            tabHighlight.enabled = true;
                            break;
                        case 3:
                            crewSettings.gameObject.SetActive(true);
                            crewTabHighlight.enabled = true;
                            break;
                        case 4:
                            impostorSettings.gameObject.SetActive(true);
                            impostorTabHighlight.enabled = true;
                            break;
                        case 5:
                            neutralSettings.gameObject.SetActive(true);
                            neutralTabHighlight.enabled = true;
                            break;
                        case 6:
                            combinationSettings.gameObject.SetActive(true);
                            combinationTabHighlight.enabled = true;
                            break;
                        default:
                            break;
                    }
                }));
            }

            removeAllOption(erMenu);
            removeAllOption(crewMenu);
            removeAllOption(impostorMenu);
            removeAllOption(neutralMenu);
            removeAllOption(combinationMenu);

            List<OptionBehaviour> erOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> crewOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> impostorOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> neutralOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> combinationOptions = new List<OptionBehaviour>();

            List<Transform> menu = new List<Transform>()
            { 
                erMenu.transform,
                crewMenu.transform,
                impostorMenu.transform,
                neutralMenu.transform,
                combinationMenu.transform,
            };

            var optionsList = OptionHolder.AllOption.Values.ToList();

            for (int i = 0; i < optionsList.Count; ++i)
            {
                IOption option = optionsList[i];
                int intedTab = (int)option.Tab;
                if (option.Body == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, menu[intedTab]);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.GetName();
                    stringOption.Value = stringOption.oldValue = option.CurSelection;
                    stringOption.ValueText.text = option.GetString();

                    option.SetOptionBehaviour(stringOption);
                    switch (intedTab)
                    {
                        case 0:
                            erOptions.Add(stringOption);
                            break;
                        case 1:
                            crewOptions.Add(stringOption);
                            break;
                        case 2:
                            impostorOptions.Add(stringOption);
                            break;
                        case 3:
                            neutralOptions.Add(stringOption);
                            break;
                        case 4:
                            combinationOptions.Add(stringOption);
                            break;
                        default:
                            erOptions.Add(stringOption);
                            break;

                    }
                    
                }
                option.Body.gameObject.SetActive(true);
            }

            erMenu.Children = erOptions.ToArray();
            erSettings.gameObject.SetActive(false);

            crewMenu.Children = crewOptions.ToArray();
            crewSettings.gameObject.SetActive(false);

            impostorMenu.Children = impostorOptions.ToArray();
            impostorSettings.gameObject.SetActive(false);

            neutralMenu.Children = neutralOptions.ToArray();
            neutralSettings.gameObject.SetActive(false);

            combinationMenu.Children = combinationOptions.ToArray();
            combinationSettings.gameObject.SetActive(false);

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
            menu.name = $"{name}_menu";

            return (setting, menu);
        }

        private static (GameObject, SpriteRenderer) createTab(
            GameObject template, Transform parent, string name, string imgPath)
        {
            GameObject tab = UnityEngine.Object.Instantiate(template, parent);
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

        private static bool isFindAndTrans(string name, string transKey)
        {
            if (GameObject.Find(name) == null) { return false; }

            // Settings setup has already been performed, fixing the title of the tab and returning
            GameObject.Find(name).transform.FindChild("GameGroup").FindChild(
                "Text").GetComponent<TMPro.TextMeshPro>().SetText(Helper.Translation.GetString(transKey));
            return true;
        }

    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    public static class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;

        public static void Postfix(GameOptionsMenu __instance)
        {
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) { return; }

            timer += Time.deltaTime;
            if (timer < 0.1f) { return; }
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;

            bool isGeneralSetting = __instance.name == $"{GameOptionsMenuStartPatch.GeneralSetting}_menu";
            bool isCrewSetting = __instance.name == $"{GameOptionsMenuStartPatch.CrewmateSetting}_menu";
            bool isImpostorSetting = __instance.name == $"{GameOptionsMenuStartPatch.ImpostorSetting}_menu";
            bool isNeutralSetting = __instance.name == $"{GameOptionsMenuStartPatch.NeutralSetting}_menu";
            bool isCombinationSetting = __instance.name == $"{GameOptionsMenuStartPatch.CombinationSetting}_menu";

            foreach (IOption option in OptionHolder.AllOption.Values)
            {
                switch (option.Tab)
                {
                    case OptionTab.General:
                        if (isGeneralSetting) { break; }
                        continue;
                    case OptionTab.Crewmate:
                        if (isCrewSetting) { break; }
                        continue;
                    case OptionTab.Impostor:
                        if (isImpostorSetting) { break; }
                        continue;
                    case OptionTab.Neutral:
                        if (isNeutralSetting) { break; }
                        continue;
                    case OptionTab.Combination:
                        if (isCombinationSetting) { break; }
                        continue;
                    default:
                        if (isGeneralSetting) { break; }
                        continue;
                }


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
