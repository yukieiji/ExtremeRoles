using System;

#if RELEASE
using BepInEx;
#endif
using HarmonyLib;

using TMPro;
using Twitch;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Helper;
using ExtremeRoles.Resources;
using ExtremeRoles.Performance;

using UnityObject = UnityEngine.Object;

#nullable enable

namespace ExtremeRoles.Patches.Manager;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class MainMenuManagerStartPatch
{
    private static Color discordColor => new Color32(88, 101, 242, byte.MaxValue);

	public static void Prefix(MainMenuManager __instance)
    {
		ApiServer.Create();

		// Mod DoNotPressButton
		DoNotPressButton doNotPressButton = __instance.GetComponentInChildren<DoNotPressButton>(true);
		AspectPosition asspectPos = doNotPressButton.GetComponent<AspectPosition>();
		Vector3 distanceFromEdge = asspectPos.DistanceFromEdge;
		asspectPos.DistanceFromEdge = new Vector3(1.5f, distanceFromEdge.y - 0.15f, distanceFromEdge.z);

		// Mod ExitButton
		__instance.quitButton.OnClick.AddListener(() => Logging.BackupCurrentLog());

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
			1.9f, async () => await AutoModInstaller.Instance.Update(),
			Vector3.zero, anchorTransform);

		// ModManagerButton
		Compat.CompatModMenu.CreateStartMenuButton(updateButton, anchorTransform);

		// DiscordButton
		var discordButton = createButton(
			__instance, "ExtremeRolesDiscordButton",
			"Discord", 2.4f, () => Application.OpenURL("https://discord.gg/UzJcfBYcyS"),
			new Vector3(0.0f, 0.8f, 0.0f), anchorTransform);
		discordButton.Image.color = discordButton.Text.color = discordColor;
		discordButton.DefaultImgColor = discordButton.DefaultTextColor = discordColor;

		if (!AutoModInstaller.Instance.IsInit)
		{
			TwitchManager man = FastDestroyableSingleton<TwitchManager>.Instance;
			var infoPop = UnityObject.Instantiate(man.TwitchPopup);
			infoPop.TextAreaTMP.fontSize *= 0.7f;
			infoPop.TextAreaTMP.enableAutoSizing = false;
			AutoModInstaller.Instance.InfoPopup = infoPop;
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
        renderer.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(ObjectPath.CommonImagePathFormat, "TitleBurner"));

		if (Prefab.Prop == null || Prefab.Text == null)
        {
            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            Prefab.Prop = UnityObject.Instantiate(man.TwitchPopup);
            UnityObject.DontDestroyOnLoad(Prefab.Prop);
            Prefab.Prop.name = "propForInEx";
            Prefab.Prop.gameObject.SetActive(false);

            Prefab.Text = UnityObject.Instantiate(man.TwitchPopup.TextAreaTMP);
            Prefab.Text.fontSize =
                Prefab.Text.fontSizeMax =
                Prefab.Text.fontSizeMin = 2.25f;
            Prefab.Text.alignment = TextAlignmentOptions.Center;
            UnityObject.DontDestroyOnLoad(Prefab.Text);
            UnityObject.Destroy(Prefab.Text.GetComponent<TextTranslatorTMP>());
            Prefab.Text.gameObject.SetActive(false);
            UnityObject.DontDestroyOnLoad(Prefab.Text);
        }

		CustomRegion.Update();
	}

	private static SimpleButton createButton(
		MainMenuManager instance,
		string name, string text, float fontSize,
		Action action, Vector3 pos, Transform parent)
	{
		var button = UnityObjectLoader.CreateSimpleButton(parent);

		button.gameObject.SetActive(true);
		button.Layer = instance.gameObject.layer;
		button.Scale = new Vector3(0.5f, 0.5f, 1.0f);
		button.name = name;

		button.Text.text = text;
		button.Text.fontSize =
		button.Text.fontSizeMax =
		button.Text.fontSizeMin = fontSize;
		button.ClickedEvent.AddListener(action);
		button.transform.localPosition = pos;

		return button;
	}
}
