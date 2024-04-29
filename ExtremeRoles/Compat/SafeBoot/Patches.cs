using TMPro;
using Twitch;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module;
using ExtremeRoles.Helper;

using ExtremeRoles.Resources;

using UnityObject = UnityEngine.Object;


namespace ExtremeRoles.Compat.SafeBoot;


public static class SafeModeMainMenuManagerPatch
{
	public static void Prefix(MainMenuManager __instance)
	{
		// Mod ExitButton
		__instance.quitButton.OnClick.AddListener(
			() => Logging.BackupCurrentLog());
		createUpdateButton(__instance);
	}

	public static void Postfix(MainMenuManager __instance)
	{
		DestroyableSingleton<ModManager>.Instance.ShowModStamp();

		// 動いてますよアピール
		var exrLogo = new GameObject("bannerLogoExtremeRoles");
		exrLogo.transform.parent = __instance.mainMenuUI.transform;
		exrLogo.transform.position = new Vector3(1.95f, 1.0f, 1.0f);
		var renderer = exrLogo.AddComponent<SpriteRenderer>();
		renderer.sprite = Loader.CreateSpriteFromResources(
			Path.TitleBurner, 300f);

		// プレイできないようにボタンを消し飛ばす
		__instance.playButton.gameObject.SetActive(false);

		setupPrefab();
	}

	private static void createUpdateButton(MainMenuManager instance)
	{
		var button = Loader.CreateSimpleButton(
			instance.quitButton.transform);

		button.gameObject.SetActive(true);
		button.Layer = instance.gameObject.layer;
		button.Scale = new Vector3(1.0f, 1.0f, 1.0f);
		button.name = "ExtremeRolesUpdateButton";

		button.Text.text = Translation.GetString(
			Translation.GetString("UpdateButton"));
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
			TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
			var infoPop = UnityObject.Instantiate(man.TwitchPopup);
			infoPop.TextAreaTMP.fontSize *= 0.7f;
			infoPop.TextAreaTMP.enableAutoSizing = false;
			AutoModInstaller.Instance.InfoPopup = infoPop;
		}
	}

	private static void setupPrefab()
	{
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
				Prefab.Text.fontSizeMin = 2.5f;
			Prefab.Text.alignment = TextAlignmentOptions.Center;
			UnityObject.DontDestroyOnLoad(Prefab.Text);
			UnityObject.Destroy(Prefab.Text.GetComponent<TextTranslatorTMP>());
			Prefab.Text.gameObject.SetActive(false);
			UnityObject.DontDestroyOnLoad(Prefab.Text);
		}
	}
}

public static class SafeModeVersionShowerPatch
{
	public static void Postfix(VersionShower __instance)
	{
		StatusTextShower.Instance.RebuildVersionShower(__instance);
	}
}
