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
		// Mod DoNotPressButton
		DoNotPressButton doNotPressButton = __instance.GetComponentInChildren<DoNotPressButton>(true);
		AspectPosition asspectPos = doNotPressButton.GetComponent<AspectPosition>();
		Vector3 distanceFromEdge = asspectPos.DistanceFromEdge;
		asspectPos.DistanceFromEdge = new Vector3(1.5f, distanceFromEdge.y, distanceFromEdge.z);

		// Mod ExitButton
		__instance.quitButton.OnClick.AddListener(
			(UnityAction)(() => Logging.BackupCurrentLog()));

		// 以下独自ボタン
		var leftButtonAnchor = new GameObject("LeftModButton");
		leftButtonAnchor.transform.parent = __instance.quitButton.transform;
		leftButtonAnchor.SetActive(true);
		leftButtonAnchor.layer = __instance.gameObject.layer;

		Transform anchorTransform = leftButtonAnchor.transform;
		anchorTransform.localScale = Vector3.one;
		anchorTransform.localPosition = new Vector3(10.25f, 0.5f, 10.0f);

		// UpdateButton
		var updateButton = createButton(
			__instance, "ExtremeRolesUpdateButton",
			Translation.GetString(Translation.GetString("UpdateButton")),
			1.9f, async () => await Module.Updater.Instance.CheckAndUpdate(),
			Vector3.zero, anchorTransform);

		// ModManagerButton
		Compat.CompatModMenu.CreateMenuButton(updateButton, anchorTransform);

		// DiscordButton
		var discordButton = createButton(
			__instance, "ExtremeRolesDiscordButton",
			"Discord", 2.4f, () => Application.OpenURL("https://discord.gg/UzJcfBYcyS"),
			new Vector3(0.0f, 0.8f, 0.0f), anchorTransform);
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
		exrLogo.transform.position = new Vector3(1.95f, 1.0f, 1.0f);
        var renderer = exrLogo.AddComponent<SpriteRenderer>();
        renderer.sprite = Loader.CreateSpriteFromResources(
            Resources.Path.TitleBurner, 300f);

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

		CustomRegion.Update();
	}

	private static SimpleButton createButton(
		MainMenuManager instance,
		string name, string text, float fontSize,
		Action action, Vector3 pos, Transform parent)
	{
		var button = Loader.CreateSimpleButton(parent);
		button.gameObject.SetActive(true);
		button.Layer = instance.gameObject.layer;
		button.Scale = new Vector3(0.5f, 0.5f, 1.0f);
		button.name = name;

		button.Text.text = text;
		button.Text.fontSize =
			button.Text.fontSizeMax =
			button.Text.fontSizeMin = fontSize;
		button.ClickedEvent.AddListener((UnityAction)action);
		button.transform.localPosition = pos;

		return button;
	}
}
