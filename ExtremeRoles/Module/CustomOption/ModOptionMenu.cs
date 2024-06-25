using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using AmongUs.Data;
using UnityEngine;
using TMPro;

using BepInEx.Configuration;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Patches.Meeting;

using UnityObject = UnityEngine.Object;
using ExtremeRoles.Extension.Controller;
using ExtremeRoles.Beta;


#nullable enable

namespace ExtremeRoles.Module.CustomOption;

public sealed class ModOptionMenu
{
	public bool IsReCreate =>
		modOptionButton == null ||
		popUp == null ||
		creditText == null ||
		titleText == null ||
		menuButtons.Any(x => x == null) ||
		csvButton.Any(x => x.IsReCreate);

	public enum MenuButton : byte
	{
		GhostsSeeTasksButton,
		GhostsSeeVotesButton,
		GhostsSeeRolesButton,
		ShowRoleSummaryButton,
		HideNamePlateButton,

		PublicBetaMode,
	}

	private readonly record struct ButtonActionBuilder(
		bool CurState,
		Action OnClick);

	private sealed class PopupActionButton
	{
		public bool IsReCreate => popUp == null || button == null;

		private readonly GenericPopup? popUp;
		private readonly ToggleButtonBehaviour? button;
		private readonly string key;
		private readonly Func<bool> onClick;

		public PopupActionButton(
			string key,
			in ToggleButtonBehaviour prefab,
			in Transform parent,
			in Color color,
			in Vector3 pos,
			in Func<bool> onClick)
		{
			this.key = key;
			this.onClick = onClick;
			button = UnityObject.Instantiate(
				prefab, parent);
			button.transform.localPosition = pos;
			button.Text.enableWordWrapping = false;
			button.Background.color = color;
			button.Text.fontSizeMin =
				button.Text.fontSizeMax = 2.2f;
			button.Text.transform.SetLocalZ(0.0f);

			var passiveButton = button.GetComponent<PassiveButton>();
			passiveButton.gameObject.SetActive(true);
			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnClick.AddListener(this.excute);

			popUp = UnityObject.Instantiate(
				Prefab.Prop, passiveButton.transform);

			popUp.transform.localPosition = new Vector3(-pos.x, -pos.y, -10.0f);
			popUp.TextAreaTMP.fontSize *= 0.75f;
			popUp.TextAreaTMP.enableAutoSizing = false;
		}

		public void UpdateTranslation(string postfix)
		{
			if (button != null)
			{
				button.Text.text = Translation.GetString(
					$"{key}{postfix}");
			}
		}

		private void excute()
		{
			if (popUp == null) { return; }

			foreach (var sr in popUp.GetComponentsInChildren<SpriteRenderer>())
			{
				sr.sortingOrder = 8;
			}
			foreach (var mr in popUp.GetComponentsInChildren<MeshRenderer>())
			{
				mr.sortingOrder = 9;
			}

			string info = Translation.GetString(
				$"{key}PleaseWait");
			popUp.Show(info); // Show originally
			bool result = onClick.Invoke();

			string transKey = result ?
				$"{key}Success" : $"{key}Error";
			info = Translation.GetString(transKey);

			popUp.StartCoroutine(
				Effects.Lerp(0.01f,
				new Action<float>((p) => { setPopupText(info); })));
		}

		private void setPopupText(string message)
		{
			if (popUp == null)
			{
				return;
			}
			if (popUp.TextAreaTMP != null)
			{
				popUp.TextAreaTMP.text = message;
			}
		}
	}

	private readonly ToggleButtonBehaviour? modOptionButton;

	private readonly GameObject? popUp;

	private readonly TextMeshPro? creditText;
	private readonly TextMeshPro? titleText;
	private readonly IReadOnlyList<ToggleButtonBehaviour> menuButtons;
	private readonly IReadOnlyList<PopupActionButton> csvButton;
	private GenericPopup? confirmMenu;

	private static ClientOption clientOpt => ClientOption.Instance;

	public ModOptionMenu(in OptionsMenuBehaviour optionMenu)
	{
		popUp = createCustomMenu(optionMenu);

		var buttonPrefab = createButtonPrefab(optionMenu);
		menuButtons = initializeCustomMenu(buttonPrefab);
		csvButton = initializeCsvLogic(buttonPrefab);
		modOptionButton = initializeModButton(buttonPrefab, optionMenu);

		creditText = initializeCreditText();
		titleText = initializeMenuTitle();
		popUp.SetActive(false);
	}

	public void Hide()
	{
		if (popUp != null)
		{
			popUp.SetActive(false);
		}
	}

	public void UpdateTranslation()
	{
		if (modOptionButton != null)
		{
			modOptionButton.Text.text = Translation.GetString(
				"modOptionText");
		}
		if (titleText != null)
		{
			titleText.text = Translation.GetString("moreOptionText");
		}

		foreach (var button in menuButtons)
		{
			if (button != null)
			{
				button.Text.text = Translation.GetString(
					button.name);
			}
		}
		foreach (var button in csvButton)
		{
			button.UpdateTranslation("Csv");
		}
		updateCreditText();
	}

	private void updateCreditText()
	{
		if (creditText == null) { return; }

		creditText.transform.localPosition = new Vector3(0.0f, -2.0f);

		StringBuilder showTextBuilder = new StringBuilder();

		showTextBuilder
			.Append("<size=175%>Extreme Roles<space=0.9em>")
			.Append(Translation.GetString("version"))
			.Append(Assembly.GetExecutingAssembly().GetName().Version)
			.AppendLine("</size>")
			.AppendLine($"<align=left>")
			.Append(Translation.GetString("developer"))
			.Append("yukieiji")
			.AppendLine($"<align=left>")
			.Append(Translation.GetString("debugThunk"))
			.AppendLine("stou59，Tyoubi，mamePi,")
			.AppendLine($"<align=left>　アンハッピーセット");

		if (DataManager.Settings.Language.CurrentLanguage != SupportedLangs.Japanese)
		{
			creditText.transform.localPosition = new Vector3(0.0f, -1.895f);
			showTextBuilder
				.Append($"<align=left>")
				.Append(Translation.GetString("langTranslate"))
				.Append(Translation.GetString("translatorMember"));
		}

		creditText.text = showTextBuilder.ToString();
	}

	private TextMeshPro initializeCreditText()
	{
		var text = UnityObject.Instantiate(
			Prefab.Text, popUp!.transform);
		text.name = "credit";
		text.fontSize = text.fontSizeMin = text.fontSizeMax = 2.0f;
		text.font = UnityObject.Instantiate(Prefab.Text.font);
		text.GetComponent<RectTransform>().sizeDelta = new Vector2(
			5.0f, 5.5f);
		text.gameObject.SetActive(true);
		return text;
	}

	private TextMeshPro initializeMenuTitle()
	{
		var title = UnityObject.Instantiate(
			Prefab.Text, popUp!.transform);
		title.GetComponent<RectTransform>().localPosition = Vector3.up * 2.3f;
		title.gameObject.SetActive(true);
		title.fontSize = title.fontSizeMin = title.fontSizeMax = 3.25f;
		title.transform.localPosition += new Vector3(0.0f, 0.25f);
		title.name = "titleText";
		return title;
	}

	private ToggleButtonBehaviour initializeModButton(in ToggleButtonBehaviour prefab, OptionsMenuBehaviour instance)
	{
		var trans = instance.CensorChatButton.transform;
		var button = UnityObject.Instantiate(prefab, trans.parent);
		button.transform.localPosition = trans.localPosition +
			Vector3.down * 1.0f;
		button.name = "modMenuButton";
		button.gameObject.SetActive(true);

		var passiveButton = button.GetComponent<PassiveButton>();
		passiveButton.OnClick.RemoveAllPersistentAndListeners();
		passiveButton.OnClick.AddListener(() =>
		{
			if (popUp == null) { return; }
			popUp.SetActive(false);
			popUp.SetActive(true);
		});
		return button;
	}

	private IReadOnlyList<PopupActionButton> initializeCsvLogic(
		in ToggleButtonBehaviour prefab)
		=> new List<PopupActionButton>(2)
		{
			new PopupActionButton(
				"import",
				prefab, popUp!.transform,
				Color.green,  new Vector3(-1.35f, -0.9f),
				CustomOptionCsvProcessor.Import),
			new PopupActionButton(
				"export",
				prefab, popUp!.transform,
				Palette.ImpostorRed,  new Vector3(1.35f, -0.9f),
				CustomOptionCsvProcessor.Export),
		};

	private IReadOnlyList<ToggleButtonBehaviour> initializeCustomMenu(
		in ToggleButtonBehaviour prefab)
	{
		var modOptionArr = Enum.GetValues<MenuButton>();
		var optionButton = new List<ToggleButtonBehaviour>(modOptionArr.Length);

		var buttonSize = new Vector2(2.2f, .7f);
		var mouseColor = new Color32(34, 139, 34, byte.MaxValue);
		var rectSize = new Vector2(2, 2);

		foreach (MenuButton menuType in modOptionArr)
		{
			int index = (int)menuType;

			var button = UnityObject.Instantiate(
				prefab, popUp!.transform);
			button.transform.position = Vector3.zero;
			button.transform.localPosition = new Vector3(
				index % 2 == 0 ? -1.17f : 1.17f,
				1.75f - index / 2 * 0.8f);

			button.Text.transform.SetLocalZ(0.0f);
			button.Text.text = "";
			button.Text.fontSizeMin = button.Text.fontSizeMax = 2.2f;
			button.Text.font = UnityObject.Instantiate(Prefab.Text.font);
			button.Text.GetComponent<RectTransform>().sizeDelta = rectSize;

			button.name = menuType.ToString();
			button.gameObject.SetActive(true);
			button.gameObject.transform.SetAsFirstSibling();

			var passiveButton = button.GetComponent<PassiveButton>();
			var colliderButton = button.GetComponent<BoxCollider2D>();

			colliderButton.size = buttonSize;

			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnMouseOut.RemoveAllPersistentAndListeners();
			passiveButton.OnMouseOver.RemoveAllPersistentAndListeners();

			var builder = createButtonActionBuilder(button, menuType);

			button.onState = builder.CurState;
			changeButtonColor(button);

			passiveButton.OnClick.AddListener(builder.OnClick);
			passiveButton.OnMouseOver.AddListener(() =>
				{
					button.Background.color = mouseColor;
				});
			passiveButton.OnMouseOut.AddListener(() =>
				{
					changeButtonColor(button);
				});

			foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				spr.size = buttonSize;
			}
			optionButton.Add(button);
		}
		return optionButton;
	}

	private ButtonActionBuilder createButtonActionBuilder(
		ToggleButtonBehaviour button, in MenuButton menu)
		=> menu switch
		{
			MenuButton.GhostsSeeTasksButton => createPassiveButtonActionBuilder(
				button, clientOpt.GhostsSeeTask),
			MenuButton.GhostsSeeVotesButton => createPassiveButtonActionBuilder(
				button, clientOpt.GhostsSeeVote),
			MenuButton.GhostsSeeRolesButton => createPassiveButtonActionBuilder(
				button, clientOpt.GhostsSeeRole),
			MenuButton.ShowRoleSummaryButton => createPassiveButtonActionBuilder(
				button, clientOpt.ShowRoleSummary),
			MenuButton.HideNamePlateButton => createPassiveButtonActionBuilder(
				button, clientOpt.HideNamePlate,
				() =>
				{
					NamePlateHelper.NameplateChange = true;
				}),
			MenuButton.PublicBetaMode => new ButtonActionBuilder(
				PublicBeta.Instance.Enable,
				() =>
				{
					if (confirmMenu != null)
					{
						UnityObject.Destroy(confirmMenu);
						confirmMenu = null;
					}

					var beta = PublicBeta.Instance;
					var pos = new Vector3(0.0f, 0.0f, -20.0f);
					bool target = !beta.Enable;

					string targetStr = Translation.GetString(
						target ? "EnableKey" : "DisableKey");

					confirmMenu = Prefab.CreateConfirmMenu(
						() =>
						{
							beta.SwitchMode();
							var popUp = UnityObject.Instantiate(
								Prefab.Prop, this.popUp!.transform);

							popUp.destroyOnClose = true;

							popUp.transform.localScale = new Vector3(1.25f, 1.0f, 1.0f);
							popUp.transform.Find("ExitGame").localScale = new Vector3(0.8f, 1.0f, 1.0f);
							popUp.TextAreaTMP.transform.localScale = new Vector3(0.8f, 1.0f, 1.0f);
							popUp.TextAreaTMP.fontSize *= 0.75f;
							popUp.TextAreaTMP.enableAutoSizing = false;

							popUp.transform.localPosition = pos;

							string showText = Translation.GetString("PublicBetaReboot");
							popUp.Show(
								string.Format(showText, targetStr));
						},
						StringNames.Accept);
					confirmMenu.transform.SetParent(popUp!.transform);
					confirmMenu.transform.localPosition = pos;

					string func = TranslationControllerExtension.GetString(
						BetaContentManager.TransKey);

					string warnText = Translation.GetString("PublicBetaWarning");
					confirmMenu.Show(
						$"{string.Format(warnText, targetStr)}\n{func}");

					button.onState = target;
					changeButtonColor(button);
				}),
			_ => throw new ArgumentException("NoDef ModMenu"),
		};

	private static GameObject createCustomMenu(
		in OptionsMenuBehaviour prefab)
	{
		GameObject popUp = UnityObject.Instantiate(
			prefab.gameObject, prefab.transform);

		popUp.transform.position = new Vector3(0, 0, -800f);
		popUp.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);

		popUp.layer = 17;
		popUp.name = "modMenu";
		UnityObject.DontDestroyOnLoad(popUp);
		UnityObject.Destroy(
			popUp.GetComponent<OptionsMenuBehaviour>());
		foreach (var gObj in getAllChilds(popUp))
		{
			if (gObj.name != "Background" &&
				gObj.name != "CloseButton")
			{
				UnityObject.Destroy(gObj);
			}
		}

		popUp.SetActive(false);

		return popUp;
	}

	private static ToggleButtonBehaviour createButtonPrefab(
		in OptionsMenuBehaviour instance)
	{
		ToggleButtonBehaviour buttonPrefab = UnityObject.Instantiate(
			instance.CensorChatButton);
		buttonPrefab.name = "censorChatPrefab";
		buttonPrefab.gameObject.SetActive(false);

		return buttonPrefab;
	}

	private static ButtonActionBuilder createPassiveButtonActionBuilder(
		ToggleButtonBehaviour button,
		ConfigEntry<bool> config,
		Action? pressAct = null)
		=> new ButtonActionBuilder(
			config.Value,
			() =>
			{
				bool newValue = !config.Value;
				config.Value = newValue;
				pressAct?.Invoke();

				button.onState = newValue;
				changeButtonColor(button);
			});


	private static IEnumerable<GameObject> getAllChilds(
		GameObject go)
	{
		for (int i = 0; i < go.transform.childCount; ++i)
		{
			yield return go.transform.GetChild(i).gameObject;
		}
	}
	private static void changeButtonColor(in ToggleButtonBehaviour button)
	{
		button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
	}
}
