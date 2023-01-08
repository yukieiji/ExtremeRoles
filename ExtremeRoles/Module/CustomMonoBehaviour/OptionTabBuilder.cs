using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Resources;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{
    [Il2CppRegister]
    public sealed class ExtremeOptionMenu : MonoBehaviour
    {
        private class Menu
        {
            public GameObject Obj { get; private set; }
            public GameOptionsMenu Component { get; private set; }

            public GameObject Tab { get; private set; }
            public SpriteRenderer TabHighLight { get; private set; }

            private OptionTab tabType;

            public Menu(OptionTab tab)
            {
                this.tabType = tab;
            }

            public void CreateTabButton(GameObject template, GameObject parent)
            {
                GameObject tabParent = Instantiate(template, parent.transform);
                tabParent.name = $"{this.tabType}_Tab";

                // Tabの構造はGameTab => ColorButton（こいつだけ）なので一個だけ取得
                Transform colorButtonTrans = tabParent.transform.GetChild(0);
                
                this.Tab = colorButtonTrans.gameObject;
                this.TabHighLight = colorButtonTrans.FindChild(
                    "Tab Background").GetComponentInChildren<SpriteRenderer>();
                
                string imgPath = this.tabType switch
                {
                    OptionTab.General  => Path.TabGlobal,

                    OptionTab.Crewmate => Path.TabCrewmate,
                    OptionTab.Impostor => Path.TabImpostor,
                    OptionTab.Neutral  => Path.TabNeutral,

                    OptionTab.Combination => Path.TabCombination,
                    
                    OptionTab.GhostCrewmate => Path.TabGhostCrewmate,
                    OptionTab.GhostImpostor => Path.TabGhostImpostor,
                    OptionTab.GhostNeutral  => Path.TabGhostNeutral,

                    _ => Path.TestButton,
                };


                colorButtonTrans.FindChild("Icon").GetComponent<SpriteRenderer>().sprite =
                    Loader.CreateSpriteFromResources(imgPath, 150f);
                this.TabHighLight.enabled = false;
            }

            public void CreateMenuBody(GameObject template)
            {
                this.Obj = Instantiate(
                    template, template.transform.parent);
                this.Component = this.Obj.transform.FindChild("GameGroup").FindChild(
                    "SliderInner").GetComponent<GameOptionsMenu>();

                string name = string.Format(MenuNameTemplate, this.tabType.ToString());

                this.Obj.name = string.Format(MenuNameTemplate, this.tabType.ToString());
                this.Component.name = $"{name}_menu";
            }

            public void SetActive(bool activate)
            {
                this.Obj.gameObject.SetActive(activate);
                this.TabHighLight.enabled = activate;
            }
        }

        public const string MenuNameTemplate = "ExtremeRoles_{0}Settings";
        public const string TemplateName = "menuTemplate";

        private Dictionary<OptionTab, Menu> allMenu = new Dictionary<OptionTab, Menu>();

        private GameSettingMenu menu;
        private GameObject settingMenuTemplate;

        private GameObject tabTemplate;

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

        private Menu createMenu(OptionTab tab)
        {
            Menu menu = new Menu(tab);
            menu.CreateTabButton(this.tabTemplate, this.menu.Tabs);
            menu.CreateMenuBody(this.settingMenuTemplate);
            
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

                    foreach (Menu menu in this.allMenu.Values)
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
                this.tabTemplate,
                (UnityAction)(() => {
                    this.menu.RolesSettings.gameObject.SetActive(true);
                    this.menu.RolesSettingsHightlight.enabled = true;
                }));

            foreach (Menu menu in this.allMenu.Values)
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
            foreach (OptionTab tab in Enum.GetValues(typeof(OptionTab)))
            {
                this.allMenu.Add(tab, createMenu(tab));
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
            template.name = TemplateName;
            template.SetActive(false);

            return template;
        }
    }
}
