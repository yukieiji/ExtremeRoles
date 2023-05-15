using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;

using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Extension.UnityEvent;
using ExtremeRoles.Resources;
using ExtremeRoles.GameMode;
using ExtremeRoles.GameMode.RoleSelector;
using ExtremeRoles.GameMode.Option.ShipGlobal;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign;

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeOptionMenu : MonoBehaviour
{
    private Dictionary<OptionTab, OptionMenuTab> allMenu = new Dictionary<OptionTab, OptionMenuTab>();

    private SimpleButton button;
    private GameSettingMenu menu;
    private GameObject settingMenuTemplate;
    private GameObject tabTemplate;

    private const string templateName = "menuTemplate";

    public ExtremeOptionMenu(IntPtr ptr) : base(ptr) { }

    public void Awake()
    {
        this.allMenu.Clear();

        this.menu = base.gameObject.GetComponent<GameSettingMenu>();

        // ForceEnable Tabs for fixing HideNSeekOptions turnoff tabs
        this.menu.Tabs.gameObject.SetActive(true);

        createRoleAssignFilterButton();

        setupTemplate();
        setupOptionMenu();

        recreateTabButtonFunction();
        retransformTabButton();
    }

    public void OnDisable()
    {
        this.menu.RegularGameSettings.SetActive(false);
        this.menu.RolesSettings.gameObject.SetActive(false);
        this.menu.HideNSeekSettings.SetActive(false);

        this.menu.GameSettingsHightlight.enabled = false;
        this.menu.RolesSettingsHightlight.enabled = false;

        foreach (OptionMenuTab menu in this.allMenu.Values)
        {
            menu.SetActive(false);
        }
    }

    private void createRoleAssignFilterButton()
    {
        GameObject obj = Instantiate(
           Loader.GetUnityObjectFromResources<GameObject>(
               "ExtremeRoles.Resources.Asset.simplebutton.asset",
               "assets/common/simplebutton.prefab"),
           this.menu.transform);
        this.button = obj.GetComponent<SimpleButton>();

        this.button.Layer = this.menu.gameObject.layer;
        this.button.Scale = new Vector3(0.625f, 0.3f, 1.0f);
        this.button.gameObject.transform.localPosition = new Vector3(2.0f, 1.75f);
        this.button.Text.text = "ロールアサインフィルター";
        this.button.Text.fontSize =
            this.button.Text.fontSizeMax =
            this.button.Text.fontSizeMin = 1.9f;
        this.button.ClickedEvent.AddListener(
            (UnityAction)(() =>
            {
                RoleAssignFilter.Instance.OpenEditor(base.gameObject);
            }));
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
        button.OnClick.RemoveAllPersistentAndListeners();
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
        switch (ExtremeGameModeManager.Instance.CurrentGameMode)
        {
            case GameModes.Normal:
                reconstructButton(
                    this.tabTemplate,
                    (UnityAction)(() =>
                    {
                        this.menu.RegularGameSettings.SetActive(true);
                        this.menu.GameSettingsHightlight.enabled = true;
                    }));

                reconstructButton(
                    this.menu.Tabs.transform.FindChild("RoleTab").gameObject,
                    (UnityAction)(() =>
                    {
                        this.menu.RolesSettings.gameObject.SetActive(true);
                        this.menu.RolesSettingsHightlight.enabled = true;
                    }));
                break;
            case GameModes.HideNSeek:
                reconstructButton(
                    this.tabTemplate,
                    (UnityAction)(() =>
                    {
                        this.menu.HideNSeekSettings.SetActive(true);
                        this.menu.GameSettingsHightlight.enabled = true;
                    }));
                this.menu.Tabs.transform.FindChild("RoleTab").gameObject.SetActive(false);
                break;
            default:
                break;
        };

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
        // ワイドアスペクト解像度対応処理(なんでここ無効になってんだ・・・・)
        spacer.spaceWiderAspectRatios = true;
        spacer.OnEnable();

        this.menu.Tabs.transform.localPosition = new Vector3(-0.465f, 0.0f, 0.0f);

        foreach (OptionMenuTab menu in this.allMenu.Values)
        {
            reconstructButton(
                menu.Tab,
                (UnityAction)(() => {
                    menu.SetActive(true);
                }));
        }
    }

    private void setupTemplate()
    {
        this.tabTemplate = this.menu.Tabs.transform.FindChild("GameTab").gameObject;
        GameObject gameSettingTemplateBase = base.transform.FindChild("Game Settings").gameObject;
        this.settingMenuTemplate = createOptionMenuTemplate(gameSettingTemplateBase);
    }

    private void setupOptionMenu()
    {
        var transform = this.menu.AllItems.FirstOrDefault(
            x =>
            {
                StringOption strOption = x.GetComponent<StringOption>();
                return strOption != null;

            });
        var stringOptionTemplate = transform.GetComponent<StringOption>();
        if (!stringOptionTemplate) { return; }

        foreach (OptionTab tab in Enum.GetValues(typeof(OptionTab)))
        {
            this.allMenu.Add(tab, createMenu(tab, stringOptionTemplate));
        }

        var exGmM = ExtremeGameModeManager.Instance;
        var shipOption = exGmM.ShipOption;
        var roleSelector = exGmM.RoleSelector;

        foreach (var (id, option) in OptionHolder.AllOption)
        {
            OptionTab tab = option.Tab;

            if (tab == OptionTab.General &&
                Enum.IsDefined(typeof(GlobalOption), id))
            {
                option.SetHeaderTo(id == shipOption.HeadOptionId);
            }

            if (Enum.IsDefined(typeof(OptionHolder.CommonOptionKey), id) ||
                roleSelector.IsValidGlobalRoleOptionId((RoleGlobalOption)id) ||
                tab switch
                {
                    OptionTab.General => shipOption.IsValidOption(id),
                    _ => roleSelector.IsValidRoleOption(option),
                })
            {
                this.allMenu[tab].AddOption(option);
            }
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
