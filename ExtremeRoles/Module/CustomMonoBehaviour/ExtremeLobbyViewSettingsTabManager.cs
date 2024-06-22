using ExtremeRoles.Extension.Controller;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Module.NewOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Extension.Option;
using ExtremeRoles.Module.NewOption.View;

#nullable enable
namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeLobbyViewSettingsTabManager(IntPtr ptr) : MonoBehaviour(ptr)
{
	private LobbyViewSettingsPane? vanillaSettings;

	private PassiveButton? testButton;
	private const float initPos = 1.44f;
	private List<OptionGroupViewObject<ViewSettingsInfoPanel>> optionGroupViewObject = new();

	public void Awake()
	{
		if (!base.gameObject.TryGetComponent<LobbyViewSettingsPane>(out var lobby))
		{
			return;
		}
		this.vanillaSettings = lobby;

		this.testButton = Instantiate(
			this.vanillaSettings.taskTabButton,
			this.vanillaSettings.rolesTabButton.transform);
		this.testButton.transform.localPosition = new Vector3(3.5f, 0, 0);
		this.testButton.OnClick.RemoveAllListeners();
		this.testButton.OnClick.AddListener(changeExRTab);
	}

	private void changeExRTab()
	{
		if (this.vanillaSettings == null)
		{
			return;
		}

		this.vanillaSettings.rolesTabButton.SelectButton(false);
		this.vanillaSettings.taskTabButton.SelectButton(false);
		this.testButton.SelectButton(true);

		if (NewOptionManager.Instance.TryGetTab(OptionTab.General, out var container))
		{
			foreach (var obj in this.vanillaSettings.settingsInfo)
			{
				Destroy(obj);
			}
			this.vanillaSettings.settingsInfo.Clear();
			this.optionGroupViewObject.Clear();
			this.optionGroupViewObject.Capacity = container.Count;

			foreach (var group in container.Category)
			{
				var categoryHeaderMasked = Instantiate(
					this.vanillaSettings.categoryHeaderOrigin);
				categoryHeaderMasked.transform.SetParent(
					this.vanillaSettings.settingsContainer);
				categoryHeaderMasked.transform.localScale = Vector3.one;
				this.vanillaSettings.settingsInfo.Add(categoryHeaderMasked.gameObject);

				var groupViewObj = new OptionGroupViewObject<ViewSettingsInfoPanel>(
					categoryHeaderMasked, group.Count);
				foreach (var option in group.Options)
				{
					ViewSettingsInfoPanel viewSettingsInfoPanel = Instantiate(
						this.vanillaSettings.infoPanelOrigin);
					viewSettingsInfoPanel.transform.SetParent(
						this.vanillaSettings.settingsContainer);
					viewSettingsInfoPanel.transform.localScale = Vector3.one;
					this.vanillaSettings.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

					groupViewObj.Options.Add(viewSettingsInfoPanel);
				}
				this.optionGroupViewObject.Add(groupViewObj);
			}
		}
		this.updateTextAndPos(OptionTab.General);
	}

	private void updateTextAndPos(OptionTab tab)
	{
		if (this.vanillaSettings == null ||
			!NewOptionManager.Instance.TryGetTab(tab, out var container))
		{
			return;
		}

		float yPos = initPos;

		foreach (var (catego, optionGroupView) in Enumerable.Zip(container.Category, this.optionGroupViewObject))
		{
			var category = optionGroupView.Category;
			category.transform.localPosition = new Vector3(-9.77f, yPos, -2f);
			category.ReplaceExRText(catego.Name, 61);

			yPos -= 0.85f;

			int activeIndex = 0;
			foreach (var (option, optionView) in Enumerable.Zip(catego.Options, optionGroupView.Options))
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

		this.vanillaSettings.scrollBar.SetYBoundsMax(-yPos);
		this.vanillaSettings.scrollBar.ScrollToTop();
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
}
