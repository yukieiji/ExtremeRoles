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

#nullable enable
namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremeLobbyViewSettingsTabManager(IntPtr ptr) : MonoBehaviour(ptr)
{
	private LobbyViewSettingsPane? vanillaSettings;

	private PassiveButton? testButton;
	private const float initPos = 1.44f;

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

		foreach (var obj in this.vanillaSettings.settingsInfo)
		{
			Destroy(obj);
		}
		this.vanillaSettings.settingsInfo.Clear();

		if (NewOptionManager.Instance.TryGetTab(OptionTab.General, out var container))
		{
			foreach (var group in container.Category)
			{
				var categoryHeaderMasked = Instantiate(
					this.vanillaSettings.categoryHeaderOrigin);
				categoryHeaderMasked.transform.SetParent(
					this.vanillaSettings.settingsContainer);
				categoryHeaderMasked.transform.localScale = Vector3.one;
				this.vanillaSettings.settingsInfo.Add(categoryHeaderMasked.gameObject);

				foreach (var option in group.AllOption)
				{
					ViewSettingsInfoPanel viewSettingsInfoPanel = Instantiate(
						this.vanillaSettings.infoPanelOrigin);
					viewSettingsInfoPanel.transform.SetParent(
						this.vanillaSettings.settingsContainer);
					viewSettingsInfoPanel.transform.localScale = Vector3.one;
					this.vanillaSettings.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
				}
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
		int settingInfoIndex = 0;
		int activeOptionRowNum = 0;
		int groupNum = 0;
		foreach (var catego in container.Category)
		{
			var categoryObj = this.vanillaSettings.settingsInfo[settingInfoIndex];
			categoryObj.transform.localPosition = new Vector3(-9.77f, yPos, -2f);
			if (categoryObj.TryGetComponent<CategoryHeaderMasked>(out var categoryHeaderMasked))
			{
				setText(categoryHeaderMasked, catego.Name);
			}

			yPos -= 0.85f;

			++groupNum;
			++settingInfoIndex;

			int activeIndex = 0;
			foreach (var option in catego.AllOption)
			{
				var optionObj = this.vanillaSettings.settingsInfo[settingInfoIndex];
				++settingInfoIndex;
				bool isActive = option.IsActiveAndEnable;

				optionObj.SetActive(isActive);
				if (!isActive)
				{
					continue;
				}
				if (optionObj.TryGetComponent<ViewSettingsInfoPanel>(out var viewSettingsInfoPanel))
				{
					setInfo(viewSettingsInfoPanel, option.Title, option.ValueString);
				}
				float x;
				if (activeIndex % 2 == 0)
				{
					++activeOptionRowNum;
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
				optionObj.transform.localPosition = new Vector3(x, yPos, -2f);
			}
			yPos -= 0.59f;
		}

		this.vanillaSettings.scrollBar.SetYBoundsMax(-yPos);
		this.vanillaSettings.scrollBar.ScrollToTop();
	}

	private static void setText(CategoryHeaderMasked masked, string txt)
	{
		masked.Title.text = txt;
		masked.Background.material.SetInt(PlayerMaterial.MaskLayer, 61);
		masked.Title.fontMaterial.SetFloat("_StencilComp", 3f);
		masked.Title.fontMaterial.SetFloat("_Stencil", (float)61);
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
