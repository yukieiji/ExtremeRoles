using System;

#if RELEASE
using BepInEx;
#endif
using HarmonyLib;

using TMPro;
using Twitch;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvent;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;


namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{

    private static Color discordColor = new Color32(88, 101, 242, byte.MaxValue);

    public static void Prefix(MainMenuManager __instance)
    {

        var template = GameObject.Find("ExitGameButton");
        if (template == null) { return; }

        // Mod ExitButton
        PassiveButton passiveExitButton = template.GetComponent<PassiveButton>();
        passiveExitButton.OnClick.AddListener(
            (UnityEngine.Events.UnityAction)(() => Logging.BackupCurrentLog()));

        // UpdateButton
        GameObject updateButton = UnityEngine.Object.Instantiate(template, template.transform);
        updateButton.name = "ExtremeRolesUpdateButton";
        UnityEngine.Object.Destroy(updateButton.GetComponent<AspectPosition>());
        UnityEngine.Object.Destroy(updateButton.GetComponent<ConditionalHide>());
        updateButton.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f);

        PassiveButton passiveUpdateButton = updateButton.GetComponent<PassiveButton>();
        passiveUpdateButton.OnClick.RemoveAllPersistentAndListeners();
        passiveUpdateButton.OnClick.AddListener(
            (UnityEngine.Events.UnityAction)(
                async () => await Module.Updater.Instance.CheckAndUpdate()));

        TMP_Text textUpdate = updateButton.transform.GetChild(0).GetComponent<TMP_Text>();
        __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
        {
            textUpdate.SetText(Translation.GetString("UpdateButton"));
        })));

        // DiscordButton
        GameObject discordButton = UnityEngine.Object.Instantiate(updateButton, template.transform);
        discordButton.name = "ExtremeRolesDiscordButton";
        discordButton.transform.localPosition = new Vector3(0.0f, 1.2f, 0.0f);

        PassiveButton passiveDiscordButton = discordButton.GetComponent<PassiveButton>();
        passiveDiscordButton.OnClick.RemoveAllPersistentAndListeners();
        passiveDiscordButton.OnClick.AddListener(
            (UnityEngine.Events.UnityAction)(() => Application.OpenURL("https://discord.gg/UzJcfBYcyS")));

        TMP_Text textDiscord = discordButton.transform.GetChild(0).GetComponent<TMP_Text>();
        __instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) =>
        {
            textDiscord.SetText("Discord");
        })));

        SpriteRenderer buttonSpriteDiscord = discordButton.GetComponent<SpriteRenderer>();
        buttonSpriteDiscord.color = textDiscord.color = discordColor;
        passiveDiscordButton.OnMouseOut.AddListener((Action)delegate
        {
            buttonSpriteDiscord.color = textDiscord.color = discordColor;
        });


        if (!Module.Updater.Instance.IsInit)
        {
            TwitchManager man = FastDestroyableSingleton<TwitchManager>.Instance;
            var infoPop = UnityEngine.Object.Instantiate(man.TwitchPopup);
            infoPop.TextAreaTMP.fontSize *= 0.7f;
            infoPop.TextAreaTMP.enableAutoSizing = false;
            Module.Updater.Instance.InfoPopup = infoPop;
        }
    }

    public static void Postfix(MainMenuManager __instance)
    {
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();

#if RELEASE
        if (!ExtremeRolesPlugin.IgnoreOverrideConsoleDisable.Value &&
            ConsoleManager.ConfigConsoleEnabled.Value)
        {
            ConsoleManager.ConfigConsoleEnabled.Value = false;
            Application.Quit();
            return;
        }
#endif

        var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
        if (amongUsLogo != null)
        {
            amongUsLogo.transform.localScale *= 0.9f;
            amongUsLogo.transform.position += Vector3.up * 0.25f;
        }

        var exrLogo = new GameObject("bannerLogoExtremeRoles");
        exrLogo.transform.position = Vector3.up;
        var renderer = exrLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Loader.CreateSpriteFromResources(
            Resources.Path.TitleBurner, 300f);

        if (Module.Prefab.Prop == null || Module.Prefab.Text == null)
        {
            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            Module.Prefab.Prop = UnityEngine.Object.Instantiate(man.TwitchPopup);
            UnityEngine.Object.DontDestroyOnLoad(
                Module.Prefab.Prop);
            Module.Prefab.Prop.name = "propForInEx";
            Module.Prefab.Prop.gameObject.SetActive(false);

            Module.Prefab.Text = UnityEngine.Object.Instantiate(
                man.TwitchPopup.TextAreaTMP);
            Module.Prefab.Text.fontSize =
                Module.Prefab.Text.fontSizeMax =
                Module.Prefab.Text.fontSizeMin = 2.25f;
            Module.Prefab.Text.alignment = TextAlignmentOptions.Center;
            UnityEngine.Object.DontDestroyOnLoad(Module.Prefab.Text);
            UnityEngine.Object.Destroy(Module.Prefab.Text.GetComponent<
                TextTranslatorTMP>());
            Module.Prefab.Text.gameObject.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(Module.Prefab.Text);

        }
        Compat.CompatModMenu.CreateMenuButton();
    }
}
