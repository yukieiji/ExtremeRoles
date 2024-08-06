using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;

using TMPro;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Performance;
using ExtremeRoles.Compat.Operator;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;

using UnityObject = UnityEngine.Object;
using UnityHelper = ExtremeRoles.Helper.Unity;

namespace ExtremeRoles.Compat;

#nullable enable

internal sealed class CompatModMenu
{
	private static CompatModMenu? instance { get; set; }
	private sealed record MenuLine(TextMeshPro Text, IReadOnlyDictionary<ButtonType, SimpleButton> Button);

	private GameObject? menuBody;
	private SimpleButton? downgradeButton;

	private enum ButtonType
	{
		InstallButton,
		UpdateButton,
		UninstallButton
	}

	private const string titleName = "compatModMenu";

	private readonly Dictionary<CompatModType, MenuLine> compatModMenuLine = new Dictionary<CompatModType, MenuLine>();

	public static void CreateStartMenuButton(SimpleButton template, Transform parent)
	{
		var mngButton = UnityObject.Instantiate(
			template, parent);
		mngButton.name = "ExtremeRolesModManagerButton";
		mngButton.transform.localPosition = new Vector3(0.0f, 1.6f, 0.0f);
		mngButton.Text.text = Tr.GetString("compatModMenuButton");

		mngButton.ClickedEvent.AddListener(() =>
		{
			if (instance == null)
			{
				instance = new CompatModMenu();
			}
			instance.openMenu(template);
		});
	}

	public static void UpdateTranslation()
	{
		if (instance == null)
		{
			return;
		}
		instance.updateTranslation();
	}

	private void createAddonButtons(
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
			installButton.transform.localPosition = new Vector3(0.9f, 0.0f, -5.0f);
			installButton.ClickedEvent.AddListener(
				createOperator<ExRAddonInstaller>(modType));
			updateButtonTextAndName(ButtonType.InstallButton, installButton);

			this.compatModMenuLine.Add(
				modType,
				new (addonText, new Dictionary<ButtonType, SimpleButton>()
				{ {ButtonType.UninstallButton, installButton}, }));

		}
		else
		{
			var uninstallButton = createButton(template, addonText);
			uninstallButton.transform.localPosition = new Vector3(0.9f, 0.0f, -5.0f);
			uninstallButton.ClickedEvent.AddListener(
				createOperator<ExRAddonUninstaller>(modType));
			updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

			this.compatModMenuLine.Add(
				modType,
				new (addonText, new Dictionary<ButtonType, SimpleButton>()
				{ {ButtonType.UninstallButton, uninstallButton}, }));
		}
	}

	private TextMeshPro createButtonText(
		string name, int posIndex)
	{
		if (this.menuBody == null)
		{
			throw new ArgumentNullException("menu is null!!");
		}

		TextMeshPro modText = UnityObject.Instantiate(
			Module.Prefab.Text, this.menuBody.transform);
		modText.name = name;

		modText.transform.localPosition = new Vector3(0.25f, 1.9f - (posIndex * 0.5f), 0f);
		modText.fontSizeMin = modText.fontSizeMax = 2.0f;
		modText.font = UnityObject.Instantiate(Module.Prefab.Text.font);
		modText.GetComponent<RectTransform>().sizeDelta = new Vector2(5.4f, 5.5f);
		modText.text = $"{Tr.GetString(name)}";
		modText.alignment = TextAlignmentOptions.Left;
		modText.gameObject.SetActive(true);

		return modText;
	}

	private void createCompatModLines(SimpleButton template)
	{
		string pluginPath = string.Concat(
			Path.GetDirectoryName(Application.dataPath),
			@"\BepInEx\plugins\");
		int index = 0;

		foreach (CompatModType mod in Enum.GetValues<CompatModType>())
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
					createOperator<Uninstaller>(modInfo));
				updateButtonTextAndName(ButtonType.UninstallButton, uninstallButton);

				var updateButton = createButton(template, modText);
				updateButton.transform.localPosition = new Vector3(0.15f, 0.0f, -5.0f);
				updateButton.ClickedEvent.AddListener(
					createOperator<Updater>(modInfo));
				updateButtonTextAndName(ButtonType.UpdateButton, updateButton);

				button.Add(ButtonType.UninstallButton, uninstallButton);
				button.Add(ButtonType.UpdateButton, updateButton);
			}
			else
			{
				var installButton = createButton(template, modText);
				installButton.transform.localPosition = new Vector3(0.9f, 0.0f, -5.0f);
				installButton.ClickedEvent.AddListener(
					createOperator<Installer>(modInfo));
				updateButtonTextAndName(ButtonType.InstallButton, installButton);
				button.Add(ButtonType.InstallButton, installButton);
			}

			this.compatModMenuLine.Add(mod, new(modText, button));

			++index;
		}
	}

	private void createDowngradeButton(SimpleButton template)
	{
		if (this.menuBody == null)
		{
			return;
		}

		this.downgradeButton = UnityObject.Instantiate(
			template, this.menuBody.transform);
		this.downgradeButton.name = "DowngradeButton";
		this.downgradeButton.Scale = new Vector3(0.325f, 0.225f, 1.0f);
		this.downgradeButton.Text.fontSize =
			this.downgradeButton.Text.fontSizeMax =
			this.downgradeButton.Text.fontSizeMin = 0.5f;

		this.downgradeButton.Text.text = Tr.GetString(
			this.downgradeButton.name);

		this.downgradeButton.transform.localPosition = new Vector3(2.0f, -2.35f, 0f);
		this.downgradeButton.ClickedEvent.AddListener(
			Module.AutoModInstaller.Instance.Downgrade);
	}

	private void initMenu(SimpleButton template)
	{
		if (this.menuBody == null)
		{
			return;
		}

		TextMeshPro title = UnityObject.Instantiate(
			Module.Prefab.Text, this.menuBody.transform);
		var rect = title.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(5.4f, 2.0f);
		title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
		title.gameObject.SetActive(true);
		title.name = "title";
		title.text = Tr.GetString(titleName);
		title.autoSizeTextContainer = false;
		title.fontSizeMin = title.fontSizeMax = 3.25f;
		title.transform.localPosition = new Vector3(0.0f, 2.45f, 0f);

		removeUnnecessaryComponent();
		setTransfoms();
		createCompatModLines(template);
		createDowngradeButton(template);
	}

	private void openMenu(SimpleButton mngButton)
	{
		if (this.menuBody == null)
		{
			this.menuBody = UnityObject.Instantiate(
				FastDestroyableSingleton<EOSManager>.Instance.TimeOutPopup);
			this.menuBody.name = "ExtremeRoles_CompatModMenu";
			this.menuBody.SetActive(true);
			this.compatModMenuLine.Clear();

			initMenu(mngButton);
		}
		this.menuBody.SetActive(true);
	}

	private void removeUnnecessaryComponent()
	{
		if (this.menuBody == null)
		{
			return;
		}

		UnityHelper.DestroyComponent<TimeOutPopupHandler>(this.menuBody);
		UnityHelper.DestroyComponent<ControllerNavMenu>(this.menuBody);

		destroyChild(this.menuBody, "OfflineButton");
		destroyChild(this.menuBody, "RetryButton");
		destroyChild(this.menuBody, "Text_TMP");
	}

	private void setTransfoms()
	{
		if (this.menuBody == null)
		{
			return;
		}

		Transform closeButtonTransform = this.menuBody.transform.FindChild("CloseButton");
		if (closeButtonTransform != null)
		{
			closeButtonTransform.localPosition = new Vector3(-3.25f, 2.5f, 0.0f);

			PassiveButton closeButton = closeButtonTransform.gameObject.GetComponent<PassiveButton>();
			closeButton.OnClick.RemoveAllPersistentAndListeners();
			closeButton.OnClick.AddListener(() =>
			{
				this.menuBody.SetActive(false);

			});
		}

		Transform bkSprite = this.menuBody.transform.FindChild("BackgroundSprite");
		if (bkSprite != null)
		{
			bkSprite.localScale = new Vector3(1.0f, 1.9f, 1.0f);
			bkSprite.localPosition = new Vector3(0.0f, 0.0f, 2.0f);
		}
	}

	private void updateTranslation()
	{
		if (this.menuBody == null) { return; }

		TextMeshPro title = this.menuBody.GetComponent<TextMeshPro>();
		title.text = Tr.GetString(titleName);

		foreach (var (mod, menu) in this.compatModMenuLine)
		{
			menu.Text.text = $"{Tr.GetString(mod.ToString())}";

			foreach (var (buttonType, button) in menu.Button)
			{
				updateButtonTextAndName(buttonType, button);
			}
		}

		if (this.downgradeButton == null) { return; }

		this.downgradeButton.Text.text = Tr.GetString(
			this.downgradeButton.name);
	}

	private static SimpleButton createButton(
		SimpleButton template, TextMeshPro text)
	{
		var button = UnityObject.Instantiate(
			template, text.transform);
		button.name = $"{text.text}Button";
		button.Scale = new Vector3(0.375f, 0.275f, 1.0f);
		button.Text.fontSize =
			button.Text.fontSizeMax =
			button.Text.fontSizeMin = 0.75f;
		return button;
	}

	private static void updateButtonTextAndName(
		ButtonType buttonType, SimpleButton button)
	{
		button.name = buttonType.ToString();
		updateButtonText(buttonType, button);
	}

	private static void updateButtonText(ButtonType buttonType, SimpleButton button)
	{
		button.Text.text = Tr.GetString(buttonType.ToString());
	}

	private static Action createOperator<T>(object parm)
		where T : OperatorBase
	{
		return () =>
		{
			object? instance = Activator.CreateInstance(
				typeof(T),
				BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.OptionalParamBinding,
				null,
				[ parm ],
				null);
			if (instance is T curOperator)
			{
				curOperator.Excute();
			}
		};
	}

	private static void destroyChild(GameObject obj, string name)
	{
		Transform targetTrans = obj.transform.FindChild(name);
		if (targetTrans != null)
		{
			UnityObject.Destroy(targetTrans.gameObject);
		}
	}
}
