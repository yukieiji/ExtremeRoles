using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Compat
{
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

        private static Dictionary<CompatModType,(TextMeshPro, Dictionary<ButtonType, GameObject>)> compatModMenuLine = new Dictionary<
            CompatModType, (TextMeshPro, Dictionary<ButtonType, GameObject>)>();

        public static void CreateMenuButton()
        {
            compatModMenuLine.Clear();
            GameObject buttonTemplate = GameObject.Find("AnnounceButton");
            GameObject compatModMenuButton = Object.Instantiate<GameObject>(
                buttonTemplate, buttonTemplate.transform.parent);
            compatModMenuButton.name = "CompatModMenuButton";
            compatModMenuButton.transform.SetSiblingIndex(7);
            PassiveButton compatModButton = compatModMenuButton.GetComponent<PassiveButton>();
            SpriteRenderer compatModSprite = compatModMenuButton.GetComponent<SpriteRenderer>();
            compatModSprite.sprite = Resources.Loader.CreateSpriteFromResources(
                Resources.Path.CompatModMenuImage, 200f);
            compatModButton.OnClick = new Button.ButtonClickedEvent();
            compatModButton.OnClick.AddListener((System.Action)(() =>
            {
                if (!menuBody)
                {
                    initMenu();
                }
                menuBody.gameObject.SetActive(true);
                
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
                    updateButtonText(buttonType, button);
                }
            }

        }

        private static void initMenu()
        {
            menuBody = Object.Instantiate(
                FastDestroyableSingleton<EOSManager>.Instance.TimeOutPopup);
            menuBody.name = "ExtremeRoles_CompatModMenu";

            TextMeshPro title = Object.Instantiate(
                Module.Prefab.Text, menuBody.transform);
            var rect = title.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(5.4f, 2.0f);
            title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
            title.gameObject.SetActive(true);
            title.name = "title";
            title.text = Helper.Translation.GetString(titleName);
            title.autoSizeTextContainer = false;
            title.fontSizeMin = title.fontSizeMax = 4.0f;
            title.transform.localPosition = new Vector3(0.0f, 2.45f, 0f);

            removeUnnecessaryComponent();
            setTransfoms();
            createCompatModLines();
        }

        private static void createCompatModLines()
        {
            var buttonTemplate = GameObject.Find("ExitGameButton/ExtremeRolesUpdateButton");
            
            if (buttonTemplate == null) { return; }

            string pluginPath = string.Concat(
                Path.GetDirectoryName(Application.dataPath),
                @"\BepInEx\plugins\");
            int index = 0;

            foreach (CompatModType mod in System.Enum.GetValues(typeof(CompatModType)))
            {
                string modName = mod.ToString();

                if (mod == CompatModType.ExtremeSkins)
                {
                    createAddonButtons(index, pluginPath, mod, buttonTemplate);
                    ++index;
                }

                if (!CompatModManager.ModInfo.ContainsKey(mod)) { continue; }

                TextMeshPro modText = createButtonText(modName, index);

                var button = new Dictionary<ButtonType, GameObject>();
                var (dllName, repoURI) = CompatModManager.ModInfo[mod];

                if (ExtremeRolesPlugin.Compat.LoadedMod.ContainsKey(mod) ||
                    File.Exists($"{pluginPath}{dllName}.dll"))
                {
                    var (uninstallButton, passiveUninstallButton) = createButton(
                        buttonTemplate, modText);
                    uninstallButton.transform.localPosition = new Vector3(1.85f, 0.0f, -5.0f);
                    passiveUninstallButton.OnClick.AddListener(
                        createUnInstallAction(dllName));
                    updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

                    var (updateButton, passiveUpdateButton) = createButton(
                        buttonTemplate, modText);
                    updateButton.transform.localPosition = new Vector3(0.35f, 0.0f, -5.0f);
                    passiveUpdateButton.OnClick.AddListener(
                        createUpdateAction(mod, dllName, repoURI));
                    updateButtonTextAndName(ButtonType.UpdateButton, updateButton);

                    button.Add(ButtonType.UninstallButton, uninstallButton);
                    button.Add(ButtonType.UpdateButton, updateButton);
                }
                else
                {
                    var (installButton, passiveInstallButton) = createButton(buttonTemplate, modText);
                    installButton.transform.localPosition = new Vector3(1.1f, 0.0f, -5.0f);
                    passiveInstallButton.OnClick.AddListener(
                        createInstallAction(dllName, repoURI));
                    updateButtonTextAndName(ButtonType.InstallButton, installButton);
                    button.Add(ButtonType.InstallButton, installButton);
                }

                compatModMenuLine.Add(mod, (modText, button));

                ++index;
            }
        }

        private static (GameObject, PassiveButton) createButton(
            GameObject template, TextMeshPro text)
        {
            GameObject button = Object.Instantiate(
                template, text.transform);
            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();

            return (button, passiveButton);
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

            Object.Destroy(menuBody.transform.FindChild("OfflineButton")?.gameObject);
            Object.Destroy(menuBody.transform.FindChild("RetryButton")?.gameObject);
            Object.Destroy(menuBody.transform.FindChild("Text_TMP")?.gameObject);
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
                    menuBody.gameObject.SetActive(false);

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
            ButtonType buttonType, GameObject button)
        {
            button.name = buttonType.ToString();
            updateButtonText(buttonType, button);
        }

        private static void updateButtonText(ButtonType buttonType, GameObject button)
        {
            var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            GameObject.FindObjectOfType<MainMenuManager>().StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                text.SetText(Helper.Translation.GetString(buttonType.ToString()));
            })));
        }

        private static void createAddonButtons(
            int posIndex,
            string pluginPath,
            CompatModType modType,
            GameObject buttonTemplate)
        {

            string addonName = modType.ToString();

            TextMeshPro addonText = createButtonText(addonName, posIndex);

            if (!File.Exists($"{pluginPath}{addonName}.dll"))
            {
                var (installButton, passiveInstallButton) = createButton(
                    buttonTemplate, addonText);
                installButton.transform.localPosition = new Vector3(
                    1.1f, 0.0f, -5.0f);
                passiveInstallButton.OnClick.AddListener(createInstallAction(
                    addonName,
                    "https://api.github.com/repos/yukieiji/ExtremeRoles/releases/latest"));
                updateButtonTextAndName(ButtonType.InstallButton, installButton);

                compatModMenuLine.Add(
                    modType,
                    (addonText, new Dictionary<ButtonType, GameObject>()
                    { {ButtonType.UninstallButton, installButton}, }));

            }
            else
            {
                var (uninstallButton, passiveUninstallButton) = createButton(
                    buttonTemplate, addonText);
                uninstallButton.transform.localPosition = new Vector3(
                    1.1f, 0.0f, -5.0f);
                passiveUninstallButton.OnClick.AddListener(
                    createUnInstallAction(addonName));
                updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

                compatModMenuLine.Add(
                    modType,
                    (addonText, new Dictionary<ButtonType, GameObject>()
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
            modText.fontSizeMin = modText.fontSizeMax = 3.0f;
            modText.font = Object.Instantiate(Module.Prefab.Text.font);
            modText.GetComponent<RectTransform>().sizeDelta = new Vector2(5.4f, 5.5f);
            modText.text = $"{Helper.Translation.GetString(name)}";
            modText.alignment = TextAlignmentOptions.Left;
            modText.gameObject.SetActive(true);

            return modText;
        }

        private static System.Action createInstallAction(
            string dllName, string url)
        {
            return () =>
            {
                var installer = new Excuter.Installer(dllName, url);
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

    }
}
