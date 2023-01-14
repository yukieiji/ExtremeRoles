using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class ExtremeOptionMenu : MonoBehaviour
    {
        private Dictionary<OptionTab, OptionMenuTab> allMenu = new Dictionary<OptionTab, OptionMenuTab>();

        private GameSettingMenu menu;
        private GameObject settingMenuTemplate;

        private GameObject tabTemplate;

        private const string templateName = "menuTemplate";

        public ExtremeOptionMenu(IntPtr ptr) : base(ptr) { }

        public void Awake()
        {
            this.allMenu.Clear();

            this.menu = base.gameObject.GetComponent<GameSettingMenu>();

            setupTemplate();
            setupOptionMenu();

            recreateTabButtonFunction();
            retransformTabButton();
        }

        private OptionMenuTab createMenu(OptionTab tab, StringOption optionTemplate)
        {
            OptionMenuTab menu = OptionMenuTab.Create(tab, this.settingMenuTemplate);
            menu.CreateTabButton(this.tabTemplate, this.menu.Tabs);
            menu.SetOptionTemplate(optionTemplate);
            
            return menu;
        }

        private void reconstructButton(
            GameObject tabButtonObject, UnityAction newAction)
        {
            PassiveButton button = tabButtonObject.GetComponentInChildren<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener(
                (UnityAction)(() => {

                    this.menu.RegularGameSettings.SetActive(false);
                    this.menu.RolesSettings.gameObject.SetActive(false);
                    this.menu.HideNSeekSettings.SetActive(false);

                    this.menu.GameSettingsHightlight.enabled = false;
                    this.menu.RolesSettingsHightlight.enabled = false;

                    foreach (OptionMenuTab menu in this.allMenu.Values)
                    {
                        menu.SetActive(false);
                    }
                }));
            button.OnClick.AddListener(newAction);
        }

        public void recreateTabButtonFunction()
        {
            // 基本ゲーム設定
            reconstructButton(
                this.tabTemplate,
                (UnityAction)(() => {
                    this.menu.RegularGameSettings.SetActive(true);
                    this.menu.GameSettingsHightlight.enabled = true;
                }));

            reconstructButton(
                this.menu.Tabs.transform.FindChild("RoleTab").gameObject,
                (UnityAction)(() => {
                    this.menu.RolesSettings.gameObject.SetActive(true);
                    this.menu.RolesSettingsHightlight.enabled = true;
                }));

            foreach (OptionMenuTab menu in this.allMenu.Values)
            {
                reconstructButton(
                    menu.Tab,
                    (UnityAction)(() => {
                        menu.SetActive(true);
                    }));
            }
        }
        private void retransformTabButton()
        {
            AspectSpacer spacer = this.menu.Tabs.GetComponent<AspectSpacer>();
            spacer.xSpacing = 0.77f;
            spacer.OnEnable();

            this.menu.Tabs.transform.localPosition = new Vector3(-0.465f, 0.0f, 0.0f);
        }

        private void setupTemplate()
        {
            this.tabTemplate = this.menu.Tabs.transform.FindChild("GameTab").gameObject;
            GameObject gameSettingTemplateBase = base.transform.FindChild("Game Settings").gameObject;
            this.settingMenuTemplate = createOptionMenuTemplate(gameSettingTemplateBase);
        }

        private void setupOptionMenu()
        {
            var stringOptionTemplate = FindObjectsOfType<StringOption>().FirstOrDefault();

            foreach (OptionTab tab in Enum.GetValues(typeof(OptionTab)))
            {
                this.allMenu.Add(tab, createMenu(tab, stringOptionTemplate));
            }

            foreach (var option in OptionHolder.AllOption.Values)
            {
                this.allMenu[option.Tab].AddOption(option);
            }
        }

        private static GameObject createOptionMenuTemplate(GameObject templateBase)
        {
            GameObject template = Instantiate(
                templateBase, templateBase.transform.parent);
            GameOptionsMenu menu = template.transform.FindChild(
                "GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();

            foreach (OptionBehaviour option in menu.GetComponentsInChildren<OptionBehaviour>())
            {
                Destroy(option.gameObject);
            }

            menu.Children = new OptionBehaviour[] { };

            template.name = templateName;
            template.SetActive(false);

            return template;
        }
    }
}
