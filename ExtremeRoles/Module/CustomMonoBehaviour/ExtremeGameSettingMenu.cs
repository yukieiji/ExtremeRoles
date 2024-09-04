using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.CustomMonoBehaviour.View;



#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeGameSettingMenu(IntPtr ptr) : MonoBehaviour(ptr)
{
	public sealed class Initializer : IDisposable
	{
		public PassiveButton Button { get; init; }
		public GameOptionsMenu Tab { get; init; }

		private Vector3 offset = new(0.0f, 0.25f, 0.0f);

		public Initializer(GameSettingMenu menu)
		{
			/* まずは画像とか文章を変える */
			menu.MenuDescriptionText.transform.parent.transform.localPosition += offset;

			var label = menu.transform.Find("GameSettingsLabel");
			if (label != null)
			{
				label.localPosition += new Vector3(0.0f, 0.15f, 0.0f);
			}

			// var infoImage = whatIsThis.GetChild(0);
			// infoImage.localPosition = new Vector3(-2.0f, 0.25f, -1.0f);

			/* ボタン部分調整 */
			var buttonGroup = menu.GamePresetsButton.transform.parent;
			buttonGroup.localPosition += offset;
			buttonGroup.localScale = new Vector3(1.0f, 0.9f, 1.0f);

			this.Button = Instantiate(menu.GameSettingsButton);
			this.Button.transform.SetParent(menu.GameSettingsButton.transform.parent);
			this.Button.transform.localPosition = new Vector3(-2.95f, -2.325f, -2.0f);
			this.Button.transform.localScale = new Vector3(0.84f, 0.9f, 0.9f);
			this.Button.OnClick.RemoveAllListeners();
			this.Button.OnMouseOver.RemoveAllListeners();


			this.Tab = Instantiate(menu.GameSettingsTab);
			this.Tab.transform.SetParent(menu.GameSettingsTab.transform.parent);
			this.Tab.transform.localPosition = menu.GameSettingsTab.transform.localPosition;
		}

		public void Dispose()
		{ }
	}

	private PassiveButton? button;
	private GameSettingMenu? menu;
	private ExtremeGameOptionsMenuView? tab;

	[HideFromIl2Cpp]
	public static void Initialize(in GameSettingMenu menu)
	{
		using var init = new Initializer(menu);

		var exrMenu = menu.gameObject.AddComponent<ExtremeGameSettingMenu>();

		exrMenu.button = init.Button;
		exrMenu.tab = init.Tab.gameObject.AddComponent<ExtremeGameOptionsMenuView>();
		exrMenu.menu = menu;

		init.Button.ChangeButtonText("EXTREME ROLES");

		exrMenu.button.OnClick.AddListener(() =>
		{
			exrMenu.ChangeExRTab(false);
		});
		exrMenu.button.OnMouseOver.AddListener(() =>
		{
			exrMenu.ChangeExRTab(true);
		});
	}

	[HideFromIl2Cpp]
	public static void SwitchTabPrefix(in GameSettingMenu menu, bool previewOnly)
	{
		if (!(
				menu.TryGetComponent<ExtremeGameSettingMenu>(out var comp) &&
				comp.tab != null &&
				comp.button != null &&
				previewIfCond(previewOnly)
			))
		{
			return;
		}

		comp.button.SelectButton(false);
		comp.tab.gameObject.SetActive(false);
	}

	public void ChangeExRTab(bool isPreviewOnly)
	{
		if (this.menu == null ||
			this.tab == null ||
			this.button == null)
		{
			return;
		}

		if (previewIfCond(isPreviewOnly))
		{
			this.menu.GamePresetsButton.SelectButton(false);
			this.menu.GameSettingsButton.SelectButton(false);
			this.menu.RoleSettingsButton.SelectButton(false);

			this.menu.PresetsTab.gameObject.SetActive(false);
			this.menu.GameSettingsTab.gameObject.SetActive(false);
			this.menu.RoleSettingsTab.gameObject.SetActive(false);

			this.tab.gameObject.SetActive(true);
		}

		if (isPreviewOnly)
		{
			this.menu.ToggleLeftSideDarkener(false);
			this.menu.ToggleRightSideDarkener(true);
			return;
		}
		this.menu.ToggleLeftSideDarkener(true);
		this.menu.ToggleRightSideDarkener(false);

		this.button.SelectButton(true);
		this.tab.Open();
	}

	private static bool previewIfCond(bool isPreviewOnly)
		=> (isPreviewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !isPreviewOnly;
}
