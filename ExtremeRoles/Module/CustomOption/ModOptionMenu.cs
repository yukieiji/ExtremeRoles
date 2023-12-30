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


#nullable enable

namespace ExtremeRoles.Module.CustomOption;

public sealed class ModOptionMenu
{
	public bool IsReCreate =>
		this.modOptionButton == null ||
		this.popUp == null ||
		this.creditText == null ||
		this.titleText == null ||
		this.menuButtons.Any(x => x == null) ||
		this.csvButton.Any(x => x.IsReCreate);

	private sealed record SelectionBehaviour(
		string TitleKey, Func<bool> OnClick, bool CurValue);
	private sealed record OptionButton(
		ToggleButtonBehaviour? Button,
		SelectionBehaviour Behaviour);

	public enum MenuButton : byte
	{
		GhostsSeeTasksButton,
		GhostsSeeVotesButton,
		GhostsSeeRolesButton,
		ShowRoleSummaryButton,
		HideNamePlateButton,

		PublicBetaMode,
	}

	private readonly record struct ButtonData(
		bool CurState,
		Action OnClick);

	private sealed class PopupActionButton
	{
		public bool IsReCreate => this.popUp == null || this.button == null;

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
			this.button = UnityObject.Instantiate(
				prefab, parent);
			this.button.transform.localPosition = pos;
			this.button.Text.enableWordWrapping = false;
			this.button.Background.color = color;
			this.button.Text.fontSizeMin =
				this.button.Text.fontSizeMax = 2.2f;
			this.button.Text.transform.SetLocalZ(0.0f);

			var passiveButton = this.button.GetComponent<PassiveButton>();
			passiveButton.gameObject.SetActive(true);
			passiveButton.OnClick.RemoveAllPersistentAndListeners();
			passiveButton.OnClick.AddListener(this.excute);

			this.popUp = UnityObject.Instantiate(
				Prefab.Prop, passiveButton.transform);

			var popUpPos = Prefab.Prop.transform.position;
			popUpPos.z = -2048f;

			this.popUp.transform.position = popUpPos;
			this.popUp.TextAreaTMP.fontSize *= 0.75f;
			this.popUp.TextAreaTMP.enableAutoSizing = false;
		}

		public void UpdateTranslation(string postfix)
		{
			if (this.button != null)
			{
				this.button.Text.text = Translation.GetString(
					$"{this.key}{postfix}");
			}
		}

		private void excute()
		{
			if (this.popUp == null) { return; }

			foreach (var sr in this.popUp.GetComponentsInChildren<SpriteRenderer>())
			{
				sr.sortingOrder = 8;
			}
			foreach (var mr in this.popUp.GetComponentsInChildren<MeshRenderer>())
			{
				mr.sortingOrder = 9;
			}

			string info = Translation.GetString(
				$"{this.key}PleaseWait");
			this.popUp.Show(info); // Show originally
			bool result = onClick.Invoke();

			string transKey = result ?
				$"{this.key}Success" : $"{this.key}Error";
			info = Translation.GetString(transKey);

			this.popUp.StartCoroutine(
				Effects.Lerp(0.01f,
				new Action<float>((p) => { setPopupText(info); })));
		}

		private void setPopupText(string message)
		{
			if (this.popUp == null)
			{
				return;
			}
			if (this.popUp.TextAreaTMP != null)
			{
				this.popUp.TextAreaTMP.text = message;
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
		this.popUp = createCustomMenu(optionMenu);

		var buttonPrefab = createButtonPrefab(optionMenu);
		this.menuButtons = initializeCustomMenu(buttonPrefab);
		this.csvButton = initializeCsvLogic(buttonPrefab);
		this.modOptionButton = initializeModButton(buttonPrefab, optionMenu);

		this.creditText = initializeCreditText();
		this.titleText = initializeMenuTitle();
		this.popUp.SetActive(false);
	}

	public void Hide()
	{
		if (this.popUp != null)
		{
			this.popUp.SetActive(false);
		}
	}

	public void UpdateTranslation()
	{
		if (this.modOptionButton != null)
		{
			this.modOptionButton.Text.text = Translation.GetString(
				"modOptionText");
		}
		if (this.titleText != null)
		{
			this.titleText.text = Translation.GetString("moreOptionText");
		}

		foreach (var button in this.menuButtons)
		{
			if (button != null)
			{
				button.Text.text = Translation.GetString(
					button.name);
			}
		}
		foreach (var button in this.csvButton)
		{
			button.UpdateTranslation("Csv");
		}
		updateCreditText();
	}

	private void updateCreditText()
	{
		if (this.creditText == null) { return; }

		this.creditText.transform.localPosition = new Vector3(0.0f, -2.0f);

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
			this.creditText.transform.localPosition = new Vector3(0.0f, -1.895f);
			showTextBuilder
				.Append($"<align=left>")
				.Append(Translation.GetString("langTranslate"))
				.Append(Translation.GetString("translatorMember"));
		}

		this.creditText.text = showTextBuilder.ToString();
	}

	private TextMeshPro initializeCreditText()
	{
		var text = UnityObject.Instantiate(
			Prefab.Text, this.popUp!.transform);
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
			Prefab.Text, this.popUp!.transform);
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
			if (this.popUp == null) { return; }
			this.popUp.SetActive(false);
			this.popUp.SetActive(true);
		});
		return button;
	}

	private IReadOnlyList<PopupActionButton> initializeCsvLogic(
		in ToggleButtonBehaviour prefab)
		=> new List<PopupActionButton>(2)
		{
			new PopupActionButton(
				"import",
				prefab, this.popUp!.transform,
				Color.green,  new Vector3(-1.35f, -0.9f),
				CustomOptionCsvProcessor.Import),
			new PopupActionButton(
				"export",
				prefab, this.popUp!.transform,
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
				prefab, this.popUp!.transform);
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

			var data = createModMenuButton(button, menuType);

			button.onState = data.CurState;
			button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;

			passiveButton.OnClick.AddListener(data.OnClick);
			passiveButton.OnMouseOver.AddListener(() =>
				{
					button.Background.color = mouseColor;
				});
			passiveButton.OnMouseOut.AddListener(() =>
				{
					button.Background.color = button.onState ? Color.green : Palette.ImpostorRed;
				});

			foreach (var spr in button.gameObject.GetComponentsInChildren<SpriteRenderer>())
			{
				spr.size = buttonSize;
			}
			optionButton.Add(button);
		}
		return optionButton;
	}

	private ButtonData createModMenuButton(
		in ToggleButtonBehaviour button, in MenuButton menu)
		=> menu switch
		{
			MenuButton.GhostsSeeTasksButton => createPassiveButtonData(
				button, clientOpt.GhostsSeeTask),
			MenuButton.GhostsSeeVotesButton => createPassiveButtonData(
				button, clientOpt.GhostsSeeVote),
			MenuButton.GhostsSeeRolesButton => createPassiveButtonData(
				button, clientOpt.GhostsSeeRole),
			MenuButton.ShowRoleSummaryButton => createPassiveButtonData(
				button, clientOpt.ShowRoleSummary),
			MenuButton.HideNamePlateButton => createPassiveButtonData(
				button, clientOpt.HideNamePlate,
				() =>
				{
					NamePlateHelper.NameplateChange = true;
				}),
			MenuButton.PublicBetaMode => new ButtonData(
				false,
				() =>
				{
					if (this.confirmMenu == null)
					{
						this.confirmMenu = Prefab.CreateConfirmMenu(
							() =>
							{

							},
						StringNames.Accept);
					}
					this.confirmMenu.transform.SetParent(this.popUp!.transform);
					this.confirmMenu.transform.SetLocalZ(-10.0f);
					this.confirmMenu.Show("パブリックベータモードを有効にします\n云々かんぬん");
				}),
			_ => throw new ArgumentException("NoDef ModMenu"),
		};

	private static GameObject createCustomMenu(
		in OptionsMenuBehaviour prefab)
	{
		GameObject popUp = UnityObject.Instantiate(
			prefab.gameObject, prefab.transform);

		popUp.transform.position = new Vector3(0, 0, -800f);
		popUp.transform.localPosition = new Vector3(0.0f, 0.0f, -100.0f);

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

	private static ButtonData createPassiveButtonData(
		ToggleButtonBehaviour button,
		ConfigEntry<bool> config,
		Action? pressAct = null)
		=> new ButtonData(
			config.Value,
			() =>
			{
				bool newValue = !config.Value;
				config.Value = newValue;
				pressAct?.Invoke();

				button.onState = newValue;
				button.Background.color =
					button.onState ? Color.green : Palette.ImpostorRed;
			});


	private static IEnumerable<GameObject> getAllChilds(
		GameObject go)
	{
		for (int i = 0; i < go.transform.childCount; ++i)
		{
			yield return go.transform.GetChild(i).gameObject;
		}
	}
}
