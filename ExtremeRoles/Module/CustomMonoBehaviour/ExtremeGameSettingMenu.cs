using System;
using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;


using ExtremeRoles.Module.CustomMonoBehaviour.View;

using ExtremeRoles.Patches.Option;

using System.Linq;
using ExtremeRoles.Helper;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeGameSettingMenu(IntPtr ptr) : MonoBehaviour(ptr)
{
	private readonly Dictionary<OptionTab, ExtremeGameOptionsMenuView> allMenu = new(8);
	private readonly Dictionary<OptionTab, PassiveButton> allButton = new(8);

	private GameSettingMenu? menu;

	public sealed class Initializer : IDisposable
	{
		private const float yScale = 0.85f;

		public PassiveButton NewTagButton
		{
			get
			{
				var newButton = Instantiate(this.buttonPrefab);

				newButton.transform.SetParent(this.buttonPrefab.transform.parent);
				newButton.transform.localPosition = this.buttonPrefab.transform.localPosition;
				newButton.transform.localScale = new Vector3(0.35f, 0.7f, 1.0f);
				rescaleText(newButton, 0.7f, 0.4f);

				if (newButton.buttonText.TryGetComponent(out TextTranslatorTMP text))
				{
					Destroy(text);
				}

				return newButton;
			}
		}

		public ExtremeGameOptionsMenuView NewMenu
		{
			get
			{
				var newMenu = Instantiate(this.tagPrefab);
				newMenu.transform.SetParent(this.tagPrefab.transform.parent);
				newMenu.transform.localPosition = this.tagPrefab.transform.localPosition;
				return newMenu.gameObject.AddComponent<ExtremeGameOptionsMenuView>();
			}
		}
		public GameSettingMenu Menu { get; }
		public Vector3 FirstButtonPos { get; } = new Vector3(-3.875f, -2.5f, -2.0f);

		private readonly PassiveButton buttonPrefab;
		public readonly GameOptionsMenu tagPrefab;

		public Initializer(GameSettingMenu menu)
		{

			this.Menu = menu;
			/* まずは画像とか文章を変える */
			var whatIsThis = menu.MenuDescriptionText.transform.parent.transform;
			whatIsThis.localPosition = new Vector3(-0.5f, 2.0f, -1.0f);

			var infoImage = whatIsThis.GetChild(0);
			infoImage.localPosition = new Vector3(-2.0f, 0.25f, -1.0f);
			if (infoImage.TryGetComponent(out SpriteRenderer spriteRenderer))
			{
				spriteRenderer.flipX = true;
			}

			/* ボタン部分調整 */
			var buttonGroup = menu.GamePresetsButton.transform.parent;
			buttonGroup.localPosition = new Vector3(0.0f, 1.37f, -1.0f);
			buttonGroup.localScale = new Vector3(1.0f, 0.85f, 1.0f);

			rescaleText(menu.GamePresetsButton, yScale);
			rescaleText(menu.GameSettingsButton, yScale);
			rescaleText(menu.RoleSettingsButton, yScale);

			this.buttonPrefab = Instantiate(menu.GameSettingsButton);
			this.buttonPrefab.transform.SetParent(menu.GameSettingsButton.transform.parent);
			this.buttonPrefab.transform.localPosition = this.FirstButtonPos;
			this.buttonPrefab.transform.localScale = new Vector3(0.4f, 0.8f, 1.0f);
			rescaleText(this.buttonPrefab, 0.8f, 0.4f);
			this.buttonPrefab.OnClick.RemoveAllListeners();
			this.buttonPrefab.OnMouseOver.RemoveAllListeners();


			this.tagPrefab = Instantiate(menu.GameSettingsTab);
			this.tagPrefab.transform.SetParent(menu.GameSettingsTab.transform.parent);
			this.tagPrefab.transform.localPosition = menu.GameSettingsTab.transform.localPosition;
		}

		private static void rescaleText(in PassiveButton button, in float xScale, float yScale = 1.0f)
		{
			var scale = button.buttonText.transform.localScale;
			button.buttonText.transform.localScale = new Vector3(scale.x * xScale, scale.y * yScale, scale.z);
		}


		public void Dispose()
		{
			Destroy(this.buttonPrefab.gameObject);
			Destroy(this.tagPrefab.gameObject);
		}
	}

	public void Initialize(in Initializer initialize)
	{
		this.menu = initialize.Menu;

		var firstPos = initialize.FirstButtonPos;
		int xIndex = 0;
		int yIndex = 0;

		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			var menu = initialize.NewMenu;
			var button = initialize.NewTagButton;
			if (OptionManager.Instance.TryGetTab(tab, out var optionTab))
			{
				menu.AllCategory = optionTab.Category.ToArray();
			}

			button.gameObject.name = $"{tab}Button";
			button.ChangeButtonText(Translation.GetString(string.Format(
				"ExtremeRoles_{0}Settings", tab)));
			menu.gameObject.name = $"{tab}Menu";

			button.transform.localPosition = new Vector3(
				firstPos.x + xIndex * 1.5f,
				firstPos.y - yIndex * 0.5f,
				firstPos.z);
			xIndex++;
			if (xIndex == 2)
			{
				xIndex = 0;
				yIndex++;
			}

			button.OnClick.AddListener(() =>
			{
				this.SwitchTab(tab, false);
			});
			button.OnMouseOver.AddListener(() =>
			{
				this.SwitchTab(tab, true);
			});

			this.allMenu.Add(tab, menu);
			this.allButton.Add(tab, button);
		};
	}

	public void SwitchTab(OptionTab tab, bool isPreviewOnly)
	{
		if (!this.allMenu.TryGetValue(tab, out var targetMenu) ||
			!this.allButton.TryGetValue(tab, out var targetButton))
		{
			return;
		}

		if (this.menu != null)
		{
			if (previewIfCond(isPreviewOnly))
			{
				this.menu.GamePresetsButton.SelectButton(false);
				this.menu.GameSettingsButton.SelectButton(false);
				this.menu.RoleSettingsButton.SelectButton(false);

				this.menu.PresetsTab.gameObject.SetActive(false);
				this.menu.GameSettingsTab.gameObject.SetActive(false);
				this.menu.RoleSettingsTab.gameObject.SetActive(false);

				foreach (var menu in this.allMenu.Values)
				{
					menu.gameObject.SetActive(false);
				}

				this.menu.MenuDescriptionText.text = tab.ToString();

				unselectButton();
				targetMenu.gameObject.SetActive(true);
			}

			if (isPreviewOnly)
			{
				this.menu.ToggleLeftSideDarkener(false);
				this.menu.ToggleRightSideDarkener(true);
				return;
			}
			this.menu.ToggleLeftSideDarkener(true);
			this.menu.ToggleRightSideDarkener(false);
		}

		targetButton.SelectButton(true);
		targetMenu.Open();
	}
	public void SwitchTabPrefix(bool previewOnly)
	{
		if (previewIfCond(previewOnly))
		{
			unselectButton();
			foreach (var menu in this.allMenu.Values)
			{
				menu.gameObject.SetActive(false);
			}
		}
	}

	private void unselectButton()
	{
		foreach (var button in this.allButton.Values)
		{
			button.SelectButton(false);
		}
	}

	private static bool previewIfCond(bool isPreviewOnly)
		=> (isPreviewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !isPreviewOnly;
}
