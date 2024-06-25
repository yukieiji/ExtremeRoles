using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Extension.Option;
using ExtremeRoles.Extension.UnityEvents;

using ExtremeRoles.Module.CustomOption.View;
using ExtremeRoles.GameMode;
using Il2CppInterop.Runtime.Attributes;


#nullable enable
namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class ExtremeLobbyViewSettingsTabView(IntPtr ptr) : MonoBehaviour(ptr)
{
	private LobbyViewSettingsPane? vanillaSettings;

	private const float initPos = 1.44f;
	private readonly List<OptionGroupViewObject<ViewSettingsInfoPanel>> optionGroupViewObject = new();
	private readonly Dictionary<OptionTab, PassiveButton> allButton = new(8);

	private const float blockTime = 0.25f;
	private float blockTimer = blockTime;
	private OptionTab curTab;

	public void Awake()
	{
		if (!gameObject.TryGetComponent<LobbyViewSettingsPane>(out var lobby))
		{
			return;
		}

		vanillaSettings = lobby;
		vanillaSettings.gameModeText.transform.localPosition = new Vector3(-4.0f, 3.5f, -2.0f);

		var taskButton = vanillaSettings.taskTabButton;

		taskButton.gameObject.SetActive(true);

		List<PassiveButton> allButton = [ taskButton ];

		if (vanillaSettings.rolesTabButton.gameObject.activeSelf)
		{
			allButton.Add(vanillaSettings.rolesTabButton);
		}

		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			var newButton = Instantiate(taskButton, taskButton.transform.parent);
			newButton.OnClick.RemoveAllListeners();
			newButton.OnClick.AddListener(() =>
			{
				this.changeExRTab(tab);
			});

			this.allButton.Add(tab, newButton);
			allButton.Add(newButton);
		}

		int x = -1;
		int y = -1;

		foreach (var (index, button) in allButton.Select((value, index) => (index, value)))
		{
			if (index % 5 == 0)
			{
				++y;
				x = 0;
			}
			else
			{
				++x;
			}

			button.transform.localScale = new Vector3(0.65f, 1.35f, 1.0f);
			button.transform.localPosition = new Vector3(
				-5.75f + (x * 2.5f),
				2.6f - (y * 1.1f),
				0.0f);

		}
	}

	public void FixedUpdate()
	{
		if (!(
				this.allButton.TryGetValue(this.curTab, out var button) &&
				button != null &&
				button.selected
			))
		{
			return;
		}

		if (blockTimer >= 0.0f ||
			!OptionManager.Instance.TryGetTab(this.curTab, out var container))
		{
			blockTimer -= Time.fixedDeltaTime;
			return;
		}

		resetTimer();
		bool isRefresh = false;
		foreach (var cat in container.Category)
		{
			isRefresh = isRefresh || cat.IsDirty;
			cat.IsDirty = false;
		}
		if (isRefresh)
		{
			updateTextAndPos(this.curTab);
		}
	}

	public void ChangeTabPostfix()
	{
		foreach (var button in this.allButton.Values)
		{
			button.SelectButton(false);
		}
	}

	[HideFromIl2Cpp]
	private void changeExRTab(OptionTab tab)
	{
		if (vanillaSettings == null)
		{
			return;
		}

		vanillaSettings.rolesTabButton.SelectButton(false);
		vanillaSettings.taskTabButton.SelectButton(false);

		foreach (var button in this.allButton.Values)
		{
			button.SelectButton(false);
		}

		if (!(
				this.allButton.TryGetValue(tab, out var targetButton) &&
				targetButton != null &&
				OptionManager.Instance.TryGetTab(tab, out var container)
			))
		{
			return;
		}

		targetButton.SelectButton(true);

		foreach (var obj in vanillaSettings.settingsInfo)
		{
			Destroy(obj);
		}

		vanillaSettings.settingsInfo.Clear();
		optionGroupViewObject.Clear();
		optionGroupViewObject.Capacity = container.Count;

		foreach (var group in container.Category)
		{
			var categoryHeaderMasked = Instantiate(
				vanillaSettings.categoryHeaderOrigin);
			categoryHeaderMasked.transform.SetParent(
				vanillaSettings.settingsContainer);
			categoryHeaderMasked.transform.localScale = Vector3.one;
			vanillaSettings.settingsInfo.Add(categoryHeaderMasked.gameObject);

			var groupViewObj = new OptionGroupViewObject<ViewSettingsInfoPanel>(
				categoryHeaderMasked, group.Count);
			foreach (var option in group.Options)
			{
				ViewSettingsInfoPanel viewSettingsInfoPanel = Instantiate(
					vanillaSettings.infoPanelOrigin);
				viewSettingsInfoPanel.transform.SetParent(
					vanillaSettings.settingsContainer);
				viewSettingsInfoPanel.transform.localScale = Vector3.one;
				vanillaSettings.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

				groupViewObj.Options.Add(viewSettingsInfoPanel);
			}
			optionGroupViewObject.Add(groupViewObj);
		}

		updateTextAndPos(tab);

		this.curTab = tab;
	}

	[HideFromIl2Cpp]
	private void updateTextAndPos(OptionTab tab)
	{
		if (vanillaSettings == null ||
			!OptionManager.Instance.TryGetTab(tab, out var container))
		{
			return;
		}

		float yPos = initPos;

		foreach (var (catego, optionGroupView) in container.Category.Zip(optionGroupViewObject))
		{
			if (!(
					tab is OptionTab.General ||
					ExtremeGameModeManager.Instance.RoleSelector.IsValidCategory(catego.Id)
				))
			{
				continue;
			}

			var category = optionGroupView.Category;
			if (catego.Color.HasValue)
			{
				category.Background.color = catego.Color.Value;
			}
			category.transform.localPosition = new Vector3(-9.77f, yPos, -2f);
			category.ReplaceExRText(catego.TransedName, 61);

			yPos -= 0.85f;

			int activeIndex = 0;
			foreach (var (option, optionView) in catego.Options.Zip(optionGroupView.Options))
			{
				bool isActive = option.IsActiveAndEnable;

				optionView.gameObject.SetActive(isActive);
				if (!isActive)
				{
					continue;
				}
				setInfo(optionView, option.Title, option.ValueString);
				float x;
				if (activeIndex % 2 == 0)
				{
					x = -8.95f;
					if (activeIndex > 0)
					{
						yPos -= 0.59f;
					}
				}
				else
				{
					x = -3f;
				}
				++activeIndex;
				optionView.transform.localPosition = new Vector3(x, yPos, -2f);
			}
			yPos -= 0.59f;
		}

		vanillaSettings.scrollBar.SetYBoundsMax(-yPos);
		vanillaSettings.scrollBar.ScrollToTop();
	}

	private static void setInfo(
		ViewSettingsInfoPanel panel,
		string text,
		string valueString)
	{
		panel.titleText.text = text;
		panel.settingText.text = valueString;
		panel.disabledBackground.gameObject.SetActive(false);
		panel.background.gameObject.SetActive(true);
		panel.SetMaskLayer(61);
	}

	private void resetTimer()
	{
		blockTimer = blockTime;
	}
}
