using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

namespace ExtremeRoles.Compat;

internal static class CompatModMenu
{
    private static GameObject menuBody;

    private enum ButtonType
    {
        InstallButton,
        UpdateButton,
        UninstallButton
    }

    private const string titleName = "compatModMenu";

    private static Dictionary<CompatModType,(TextMeshPro, Dictionary<ButtonType, SimpleButton>)> compatModMenuLine = new Dictionary<
        CompatModType, (TextMeshPro, Dictionary<ButtonType, SimpleButton>)>();

    public static void CreateMenuButton(SimpleButton template, Transform parent)
    {
        compatModMenuLine.Clear();

								var mngButton = Object.Instantiate(
												template, parent);
								mngButton.name = "ExtremeRolesModManagerButton";
								mngButton.transform.localPosition = new Vector3(0.0f, 1.6f, 0.0f);
								mngButton.Text.text = Helper.Translation.GetString("compatModMenuButton");

								mngButton.ClickedEvent.AddListener((System.Action)(() =>
        {
            if (!menuBody)
            {
                initMenu(mngButton);
            }
            menuBody.SetActive(true);
        }));
    }

    public static void UpdateTranslation()
    {
        if (menuBody == null) { return; }

        TextMeshPro title = menuBody.GetComponent<TextMeshPro>();
        title.text = Helper.Translation.GetString(titleName);

        foreach (var (mod, (modText, buttons)) in compatModMenuLine)
        {
            modText.text = $"{Helper.Translation.GetString(mod.ToString())}";

            foreach (var (buttonType, button) in buttons)
            {
																updateButtonTextAndName(buttonType, button);
            }
        }

    }

    private static void initMenu(SimpleButton template)
    {
        menuBody = Object.Instantiate(
            FastDestroyableSingleton<EOSManager>.Instance.TimeOutPopup);
        menuBody.name = "ExtremeRoles_CompatModMenu";
								menuBody.SetActive(true);

        TextMeshPro title = Object.Instantiate(
            Module.Prefab.Text, menuBody.transform);
        var rect = title.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(5.4f, 2.0f);
        title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
        title.gameObject.SetActive(true);
        title.name = "title";
        title.text = Helper.Translation.GetString(titleName);
        title.autoSizeTextContainer = false;
        title.fontSizeMin = title.fontSizeMax = 3.25f;
        title.transform.localPosition = new Vector3(0.0f, 2.45f, 0f);

        removeUnnecessaryComponent();
        setTransfoms();
        createCompatModLines(template);
    }

    private static void createCompatModLines(SimpleButton template)
    {

        string pluginPath = string.Concat(
            Path.GetDirectoryName(Application.dataPath),
            @"\BepInEx\plugins\");
        int index = 0;

        foreach (CompatModType mod in System.Enum.GetValues(typeof(CompatModType)))
        {
            string modName = mod.ToString();

            if (mod == CompatModType.ExtremeSkins ||
                mod == CompatModType.ExtremeVoiceEngine)
            {
                createAddonButtons(index, pluginPath, mod, template);
                ++index;
																continue;
												}

												if (!CompatModManager.ModInfo.TryGetValue(mod, out var modInfo)) { continue; }

            TextMeshPro modText = createButtonText(modName, index);

            var button = new Dictionary<ButtonType, SimpleButton>();

												string dllName = modInfo.Name;
												string repoUrl = modInfo.RepoUrl;

												if (CompatModManager.Instance.LoadedMod.ContainsKey(mod) ||
                File.Exists($"{pluginPath}{modInfo.Name}.dll"))
            {
                var uninstallButton = createButton(template, modText);
                uninstallButton.transform.localPosition = new Vector3(1.65f, 0.0f, -5.0f);
																uninstallButton.ClickedEvent.AddListener(
                    createUnInstallAction(dllName));
                updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

                var updateButton = createButton(template, modText);
                updateButton.transform.localPosition = new Vector3(0.15f, 0.0f, -5.0f);
																updateButton.ClickedEvent.AddListener(
                    createUpdateAction(mod, dllName, repoUrl));
                updateButtonTextAndName(ButtonType.UpdateButton, updateButton);

                button.Add(ButtonType.UninstallButton, uninstallButton);
                button.Add(ButtonType.UpdateButton, updateButton);
            }
            else
            {
                var installButton = createButton(template, modText);
                installButton.transform.localPosition = new Vector3(0.9f, 0.0f, -5.0f);
																installButton.ClickedEvent.AddListener(
                    createInstallAction(modInfo));
                updateButtonTextAndName(ButtonType.InstallButton, installButton);
                button.Add(ButtonType.InstallButton, installButton);
            }

            compatModMenuLine.Add(mod, (modText, button));

            ++index;
        }
    }

    private static SimpleButton createButton(
        SimpleButton template, TextMeshPro text)
    {
								var button = Object.Instantiate(
												template, text.transform);
								button.name = $"{text.text}Button";
								button.Scale = new Vector3(0.375f, 0.275f, 1.0f);
								button.Text.fontSize =
												button.Text.fontSizeMax =
												button.Text.fontSizeMin = 0.75f;
								return button;
    }

    private static void removeUnnecessaryComponent()
    {
        var timeOutPopup = menuBody.GetComponent<TimeOutPopupHandler>();
        if (timeOutPopup != null)
        {
            Object.Destroy(timeOutPopup);
        }

        var controllerNav = menuBody.GetComponent<ControllerNavMenu>();
        if (controllerNav != null)
        {
            Object.Destroy(controllerNav);
        }

        destroyChild(menuBody, "OfflineButton");
        destroyChild(menuBody, "RetryButton");
        destroyChild(menuBody, "Text_TMP");
    }

    private static void setTransfoms()
    {
        Transform closeButtonTransform = menuBody.transform.FindChild("CloseButton");
        if (closeButtonTransform != null)
        {
            closeButtonTransform.localPosition = new Vector3(-3.25f, 2.5f, 0.0f);

            PassiveButton closeButton = closeButtonTransform.gameObject.GetComponent<PassiveButton>();
            closeButton.OnClick = new Button.ButtonClickedEvent();
            closeButton.OnClick.AddListener((System.Action)(() =>
            {
                menuBody.SetActive(false);

            }));
        }

        Transform bkSprite = menuBody.transform.FindChild("BackgroundSprite");
        if (bkSprite != null)
        {
            bkSprite.localScale = new Vector3(1.0f, 1.9f, 1.0f);
            bkSprite.localPosition = new Vector3(0.0f, 0.0f, 2.0f);
        }
    }

    private static void updateButtonTextAndName(
        ButtonType buttonType, SimpleButton button)
    {
        button.name = buttonType.ToString();
        updateButtonText(buttonType, button);
    }

    private static void updateButtonText(ButtonType buttonType, SimpleButton button)
    {
								button.Text.text = Helper.Translation.GetString(buttonType.ToString());
    }

    private static void createAddonButtons(
        int posIndex,
        string pluginPath,
        CompatModType modType,
								SimpleButton template)
    {
        string addonName = modType.ToString();

        TextMeshPro addonText = createButtonText(addonName, posIndex);

        if (!File.Exists($"{pluginPath}{addonName}.dll"))
        {
            var installButton = createButton(template, addonText);
            installButton.transform.localPosition = new Vector3(
                0.9f, 0.0f, -5.0f);
												installButton.ClickedEvent.AddListener(createInstallAction(
																new CompatModInfo(
																				addonName, "", "https://api.github.com/repos/yukieiji/ExtremeRoles/releases/latest")));
            updateButtonTextAndName(ButtonType.InstallButton, installButton);

            compatModMenuLine.Add(
                modType,
                (addonText, new Dictionary<ButtonType, SimpleButton>()
                { {ButtonType.UninstallButton, installButton}, }));

        }
        else
        {
            var uninstallButton = createButton(template, addonText);
            uninstallButton.transform.localPosition = new Vector3(
																0.9f, 0.0f, -5.0f);
												uninstallButton.ClickedEvent.AddListener(
                createUnInstallAction(addonName));
            updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

            compatModMenuLine.Add(
                modType,
                (addonText, new Dictionary<ButtonType, SimpleButton>()
                { {ButtonType.UninstallButton, uninstallButton}, }));
        }
    }

    private static TextMeshPro createButtonText(
        string name, int posIndex)
    {
        TextMeshPro modText = Object.Instantiate(
            Module.Prefab.Text, menuBody.transform);
        modText.name = name;

        modText.transform.localPosition = new Vector3(0.25f, 1.9f - (posIndex * 0.5f), 0f);
        modText.fontSizeMin = modText.fontSizeMax = 2.0f;
        modText.font = Object.Instantiate(Module.Prefab.Text.font);
        modText.GetComponent<RectTransform>().sizeDelta = new Vector2(5.4f, 5.5f);
        modText.text = $"{Helper.Translation.GetString(name)}";
        modText.alignment = TextAlignmentOptions.Left;
        modText.gameObject.SetActive(true);

        return modText;
    }

    private static System.Action createInstallAction(CompatModInfo modInfo)
    {
								return () =>
								{
											var installer = new Excuter.Installer(modInfo);
											installer.Excute();
        };
    }

    private static System.Action createUnInstallAction(string dllName)
    {
								return () =>
        {
            var uninstaller = new Excuter.Uninstaller(dllName);
            uninstaller.Excute();
        };
    }

    private static System.Action createUpdateAction(
        CompatModType mod, string dllName, string url)
    {
        return () =>
        {
            var updater = new Excuter.Updater(mod, dllName, url);
            updater.Excute();
        };
    }

    private static void destroyChild(GameObject obj, string name)
    {
        Transform targetTrans = obj.transform.FindChild(name);
        if (targetTrans)
        {
            Object.Destroy(targetTrans.gameObject);
        }
    }
}
