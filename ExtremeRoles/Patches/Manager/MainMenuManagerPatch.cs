using System;

#if RELEASE
using BepInEx;
#endif
using HarmonyLib;

using TMPro;
using Twitch;

using UnityEngine;
using UnityEngine.Events;

using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
    private static Color discordColor => new Color32(88, 101, 242, byte.MaxValue);

    public static void Prefix(MainMenuManager __instance)
    {
		// Mod ExitButton
		__instance.quitButton.OnClick.AddListener(
			(UnityAction)(() => Logging.BackupCurrentLog()));

		// UpdateButton
		GameObject updateButtonObj = UnityObject.Instantiate(
		   Loader.GetUnityObjectFromResources<GameObject>(
			   "ExtremeRoles.Resources.Asset.simplebutton.asset",
			   "assets/common/simplebutton.prefab"),
		   __instance.quitButton.transform);
		var updateButton = updateButtonObj.GetComponent<SimpleButton>();

		updateButton.Layer = __instance.gameObject.layer;
		updateButton.Scale = new Vector3(0.5f, 0.5f, 1.0f);

		updateButton.gameObject.transform.localPosition = new Vector3(10.25f, 0.5f, 10.0f);
		updateButton.Text.text = Translation.GetString(Translation.GetString("UpdateButton"));
		updateButton.Text.fontSize =
			updateButton.Text.fontSizeMax =
			updateButton.Text.fontSizeMin = 1.9f;
		updateButton.name = "ExtremeRolesUpdateButton";
		updateButton.ClickedEvent.AddListener(
			(UnityAction)(async() => await Module.Updater.Instance.CheckAndUpdate()));


		// DiscordButton
		var discordButton = UnityObject.Instantiate(
		   updateButton, updateButton.transform);
		discordButton.name = "ExtremeRolesDiscordButton";
		discordButton.Scale = new Vector3(1.0f, 1.0f, 1.0f);
		discordButton.transform.localPosition = new Vector3(0.0f, 1.75f, 0.0f);
		discordButton.ClickedEvent.AddListener(
			(UnityAction)(() => Application.OpenURL("https://discord.gg/UzJcfBYcyS")));
		discordButton.Text.text = "Discord";
		discordButton.Text.fontSize =
			discordButton.Text.fontSizeMax =
			discordButton.Text.fontSizeMin = 2.4f;
		discordButton.Image.color = discordButton.Text.color = discordColor;
		discordButton.DefaultImgColor = discordButton.DefaultTextColor = discordColor;

		if (!Module.Updater.Instance.IsInit)
        {
            TwitchManager man = FastDestroyableSingleton<TwitchManager>.Instance;
            var infoPop = UnityObject.Instantiate(man.TwitchPopup);
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

        var exrLogo = new GameObject("bannerLogoExtremeRoles");
		exrLogo.transform.parent = __instance.mainMenuUI.transform;
		exrLogo.transform.position = new Vector3(2.0f, 1.0f, 1.0f);
        var renderer = exrLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Loader.CreateSpriteFromResources(
            Resources.Path.TitleBurner, 275f);

        if (Module.Prefab.Prop == null || Module.Prefab.Text == null)
        {
            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            Module.Prefab.Prop = UnityObject.Instantiate(man.TwitchPopup);
            UnityObject.DontDestroyOnLoad(Module.Prefab.Prop);
            Module.Prefab.Prop.name = "propForInEx";
            Module.Prefab.Prop.gameObject.SetActive(false);

            Module.Prefab.Text = UnityObject.Instantiate(man.TwitchPopup.TextAreaTMP);
            Module.Prefab.Text.fontSize =
                Module.Prefab.Text.fontSizeMax =
                Module.Prefab.Text.fontSizeMin = 2.25f;
            Module.Prefab.Text.alignment = TextAlignmentOptions.Center;
            UnityObject.DontDestroyOnLoad(Module.Prefab.Text);
            UnityObject.Destroy(Module.Prefab.Text.GetComponent<TextTranslatorTMP>());
            Module.Prefab.Text.gameObject.SetActive(false);
            UnityObject.DontDestroyOnLoad(Module.Prefab.Text);

        }
        // Compat.CompatModMenu.CreateMenuButton();

		CustomRegion.Update();
	}
}
