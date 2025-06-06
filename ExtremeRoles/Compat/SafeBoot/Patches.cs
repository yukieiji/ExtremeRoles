﻿using Il2CppSystem.Collections.Generic;

using TMPro;
using Twitch;

using UnityEngine;
using AmongUs.Data;
using ExtremeRoles.Translation;


using ExtremeRoles.Module;
using ExtremeRoles.Helper;

using ExtremeRoles.Resources;

using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Compat.SafeBoot;


public static class SafeBootMainMenuManagerPatch
{
	public static void Prefix(MainMenuManager __instance)
	{
		if (__instance == null)
		{
			return;
		}
		// Mod ExitButton
		__instance.quitButton.OnClick.AddListener(
			(System.Action)(
			() =>
			{
				Logging.BackupCurrentLog();
			}));
		createUpdateButton(__instance);
	}

	public static void Postfix(MainMenuManager __instance)
	{
		if (__instance == null)
		{
			return;
		}

		ModManager.Instance.ShowModStamp();

		// 動いてますよアピール
		var exrLogo = new GameObject("bannerLogoExtremeRoles");
		exrLogo.transform.parent = __instance.mainMenuUI.transform;
		exrLogo.transform.position = new Vector3(1.95f, 0.5f, 1.0f);
		var renderer = exrLogo.AddComponent<SpriteRenderer>();
		renderer.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
			ObjectPath.CommonTextureAsset,
			string.Format(ObjectPath.CommonImagePathFormat, "TitleBurner"));

		setupPrefab();
	}

	private static void createUpdateButton(MainMenuManager instance)
	{
		var button = UnityObjectLoader.CreateSimpleButton(
			instance.quitButton.transform);

		button.gameObject.SetActive(true);
		button.Layer = instance.gameObject.layer;
		button.Scale = new Vector3(1.0f, 1.0f, 1.0f);
		button.name = "ExtremeRolesUpdateButton";

		button.Text.text = Tr.GetString("UpdateButton");
		button.Text.fontSize =
			button.Text.fontSizeMax =
			button.Text.fontSizeMin = 3.0f;
		button.ClickedEvent.AddListener(
			(System.Action)(async () =>
			{
				await AutoModInstaller.Instance.Update();
			}));
		button.transform.localPosition = new Vector3(6.25f, 2.0f);

		if (!AutoModInstaller.Instance.IsInit)
		{
			TwitchManager man = TwitchManager.Instance;
			var infoPop = UnityObject.Instantiate(man.TwitchPopup);
			infoPop.TextAreaTMP.fontSize *= 0.7f;
			infoPop.TextAreaTMP.enableAutoSizing = false;
			AutoModInstaller.Instance.InfoPopup = infoPop;
		}
	}

	private static void setupPrefab()
	{
		if (Prefab.Prop != null && Prefab.Text != null)
		{
			return;
		}
		TwitchManager man = TwitchManager.Instance;
		Prefab.Prop = UnityObject.Instantiate(man.TwitchPopup);
		UnityObject.DontDestroyOnLoad(Prefab.Prop);
		Prefab.Prop.name = "propForInEx";
		Prefab.Prop.gameObject.SetActive(false);

		Prefab.Text = UnityObject.Instantiate(man.TwitchPopup.TextAreaTMP);
		Prefab.Text.fontSize =
			Prefab.Text.fontSizeMax =
			Prefab.Text.fontSizeMin = 2.5f;
		Prefab.Text.alignment = TextAlignmentOptions.Center;
		UnityObject.DontDestroyOnLoad(Prefab.Text);
		UnityObject.Destroy(Prefab.Text.GetComponent<TextTranslatorTMP>());
		Prefab.Text.gameObject.SetActive(false);
		UnityObject.DontDestroyOnLoad(Prefab.Text);
	}
}

public static class SafeBootVersionShowerPatch
{
	public static void Postfix(VersionShower __instance)
	{
		StatusTextShower.Instance.RebuildVersionShower(__instance);
	}
}

public static class SafeBootTranslationPatch
{
	public static void Postfix(ref Dictionary<string, string> allStrings)
	{
		TranslatorManager.AddTranslationData(
			DataManager.Settings.Language.CurrentLanguage, allStrings);
	}
}
