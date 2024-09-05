using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;
using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.Option;
using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;

using IReadOnlyCategoryViews = System.Collections.Generic.IReadOnlyList<ExtremeRoles.Module.CustomOption.View.OptionCategoryViewObject<ViewSettingsInfoPanel>>;
using CategoryViews = System.Collections.Generic.List<ExtremeRoles.Module.CustomOption.View.OptionCategoryViewObject<ViewSettingsInfoPanel>>;
using ExtremeRoles.Module.CustomOption.View;

#nullable enable
namespace ExtremeRoles.Module.CustomMonoBehaviour.View;


public sealed class AllOptionObject : IEnumerable<ValueTuple<OptionTab, CategoryHeaderMasked, IReadOnlyCategoryViews>>
{
	private readonly Dictionary<OptionTab, CategoryHeaderMasked> tabCategory = new Dictionary<OptionTab, CategoryHeaderMasked>(8);
	private readonly Dictionary<OptionTab, CategoryViews> optionObj = new Dictionary<OptionTab, CategoryViews>();
	public int Count => this.tabCategory.Count;

	public void Clear()
	{
		this.tabCategory.Clear();
		this.optionObj.Clear();
	}

	public bool TryGetTab(
		in OptionTab tab,
		[NotNullWhen(true)] out IReadOnlyCategoryViews? tabOptions)
	{
	 	bool result = this.optionObj.TryGetValue(tab, out var opt);
		tabOptions = opt;
		return result;
	}

	public CategoryViews CreateTab(in OptionTab tab, in CategoryHeaderMasked tabObj)
	{
		tabCategory.Add(tab, tabObj);
		var tabOption = new CategoryViews();
		optionObj.Add(tab, tabOption);
		return tabOption;
	}

	public IEnumerator<(OptionTab, CategoryHeaderMasked, IReadOnlyCategoryViews)> GetEnumerator()
	{
		foreach (var (tab, tabObj) in this.tabCategory)
		{
			if (this.TryGetTab(tab, out var tabOptions))
			{
				yield return (tab, tabObj, tabOptions);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}
}



[Il2CppRegister]
public sealed class ExtremeLobbyViewSettingsTabView(IntPtr ptr) : MonoBehaviour(ptr)
{
	private LobbyViewSettingsPane? vanillaSettings;

	private const float initPos = 1.44f;
	private const float categoryOffset = 0.85f;
	private readonly AllOptionObject optionObj = new();
	private readonly Dictionary<OptionTab, Vector3> tabPos = new(8);
	private PassiveButton? exrButton;

	private const float blockTime = 0.25f;
	private float blockTimer = blockTime;

	public void Awake()
	{
		if (!gameObject.TryGetComponent<LobbyViewSettingsPane>(out var lobby))
		{
			return;
		}

		vanillaSettings = lobby;

		var targetButton = vanillaSettings.taskTabButton;

		targetButton.gameObject.SetActive(true);
		int size = 1;
		if (vanillaSettings.rolesTabButton.gameObject.activeSelf)
		{
			++size;
			targetButton = vanillaSettings.rolesTabButton;
		}

		exrButton = Instantiate(targetButton, targetButton.transform.parent);

		string modName = Tr.GetString("MODNAME_TRANS");
		exrButton.ChangeButtonText(modName);

		if (exrButton.buttonText.TryGetComponent<TextTranslatorTMP>(out var tmp))
		{
			Destroy(tmp);
		}

		exrButton.OnClick.RemoveAllListeners();
		exrButton.OnClick.AddListener(changeExRTab);

		// x = 2.1f
		var pos = exrButton.transform.localPosition;
		exrButton.transform.localPosition = pos + new Vector3(1.75f * size, 0.0f);
	}

	public void FixedUpdate()
	{
		if (!(
				exrButton != null &&
				exrButton.selected
			))
		{
			return;
		}

		if (Input.GetKey(KeyCode.Tab))
		{
			if (Key.IsShiftDown())
			{
				changeTab(-1);
			}
			else
			{
				changeTab(1);
			}
		}

		if (blockTimer >= 0.0f)
		{
			blockTimer -= Time.fixedDeltaTime;
			return;
		}

		resetTimer();
		bool isRefresh = false;
		foreach (var (tab, container) in OptionManager.Instance)
		{
			foreach (var cat in container.Category)
			{
				isRefresh = isRefresh || cat.IsDirty;
				cat.IsDirty = false;
			}
		}
		if (isRefresh)
		{
			updateTextAndPos();
		}
	}

	public void ChangeTabPrefix()
	{
		if (exrButton == null)
		{
			return;
		}
		exrButton.SelectButton(false);
	}

	[HideFromIl2Cpp]
	private void changeExRTab()
	{
		if (vanillaSettings == null || exrButton == null)
		{
			return;
		}

		vanillaSettings.rolesTabButton.SelectButton(false);
		vanillaSettings.taskTabButton.SelectButton(false);

		exrButton.SelectButton(true);

		foreach (var obj in vanillaSettings.settingsInfo)
		{
			Destroy(obj);
		}

		vanillaSettings.settingsInfo.Clear();
		optionObj.Clear();

		foreach (var (tab, container) in OptionManager.Instance)
		{
			var tabCategoryHeaderMasked = Instantiate(vanillaSettings.categoryHeaderOrigin);
			tabCategoryHeaderMasked.transform.SetParent(
				vanillaSettings.settingsContainer);
			tabCategoryHeaderMasked.transform.localScale = Vector3.one;
			tabCategoryHeaderMasked.Background.transform.localScale = new Vector3(3.0f, 1.0f);
			tabCategoryHeaderMasked.ReplaceExRText(tab.ToString(), 61);

			vanillaSettings.settingsInfo.Add(tabCategoryHeaderMasked.gameObject);

			var tabOption = optionObj.CreateTab(tab, tabCategoryHeaderMasked);

			foreach (var cate in container.Category)
			{
				var categoryHeaderMasked = Instantiate(
					vanillaSettings.categoryHeaderOrigin);
				categoryHeaderMasked.transform.SetParent(
					vanillaSettings.settingsContainer);
				categoryHeaderMasked.transform.localScale = Vector3.one;
				if (cate.Color.HasValue)
				{
					categoryHeaderMasked.Background.color = cate.Color.Value;
				}
				categoryHeaderMasked.ReplaceExRText(cate.TransedName, 61);
				vanillaSettings.settingsInfo.Add(categoryHeaderMasked.gameObject);

				var groupViewObj = new OptionCategoryViewObject<ViewSettingsInfoPanel>.Builder(
					categoryHeaderMasked, cate.Count);
				foreach (var option in cate.Options)
				{
					ViewSettingsInfoPanel viewSettingsInfoPanel = Instantiate(
						vanillaSettings.infoPanelOrigin);
					viewSettingsInfoPanel.transform.SetParent(
						vanillaSettings.settingsContainer);
					viewSettingsInfoPanel.transform.localScale = Vector3.one;
					vanillaSettings.settingsInfo.Add(viewSettingsInfoPanel.gameObject);

					groupViewObj.Options.Add(viewSettingsInfoPanel);
				}
				tabOption.Add(groupViewObj.Build());
			}
		}
		updateTextAndPos();
	}

	[HideFromIl2Cpp]
	private void updateTextAndPos()
	{
		if (vanillaSettings == null || optionObj.Count == 0)
		{
			return;
		}

		float yPos = initPos;

		IReadOnlySet<int>? validOptionId = default;
		var instance = ExtremeGameModeManager.Instance;
		this.tabPos.Clear();

		foreach (var (tab, tabObj, tabOption) in optionObj)
		{
			var tabPos = new Vector3(-9.77f, yPos, -2f);
			this.tabPos.Add(tab, tabPos);
			tabObj.transform.localPosition = tabPos;
			yPos -= 0.9f;
			if (!OptionManager.Instance.TryGetTab(tab, out var container))
			{
				continue;
			}

			foreach (var (catego, optionGroupView) in container.Category.Zip(tabOption))
			{
				int id = catego.Id;
				if (!OptionSplitter.TryGetValidOption(catego, out validOptionId))
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

				yPos -= categoryOffset;

				int activeIndex = 0;
				int activeObjNum = 0;
				foreach (var (option, optionView) in catego.Options.Zip(optionGroupView.View))
				{
					if (!OptionSplitter.IsValidOption(validOptionId, option.Info.Id))
					{
						continue;
					}

					bool isActive = option.IsActiveAndEnable;

					optionView.gameObject.SetActive(isActive);
					if (!isActive)
					{
						continue;
					}
					++activeObjNum;
					setInfo(optionView, option.Title, option.ValueString);

					// ジェネレラルタブ以外 = 役職周りで、最初のオプション = 役職のスポーンレート
					if (tab is not OptionTab.GeneralTab &&
						activeObjNum == 1 && option.Selection <= 0)
					{
						// その役職使わないので非表示、breakではない理由として、子オプションを非表示するため
						optionView.gameObject.SetActive(false);
						continue;
					}

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

				if (tab is not OptionTab.GeneralTab && activeObjNum == 1)
				{
					//　非表示にして引いたCategoryのオフセットを戻す
					yPos += categoryOffset;
					category.gameObject.SetActive(false);
					continue;
				}
				category.gameObject.SetActive(true);
				yPos -= 0.59f;
			}
			yPos -= 0.5f;
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
	private void changeTab(int offset)
	{
		if (vanillaSettings == null ||
			vanillaSettings.scrollBar == null)
		{
			return;
		}

		var scroller = vanillaSettings.scrollBar;

		float scrollerMin = scroller.ContentYBounds.min;
		float scrollerMax = scroller.ContentYBounds.max;

		var curPos = scroller.Inner.localPosition;

		var curTab = tabPos
			.Where(x => -(x.Value.y - 1.0f) >= curPos.y)
			.Select(x => x.Key)
			.First();

		var nextTab = (OptionTab)(
			((byte)curTab + (byte)OptionTab.GhostNeutralTab + offset) % (byte)OptionTab.GhostNeutralTab);

		Helper.Logging.Debug($"Y:{curPos.y}, Tab{curTab}");

		if (tabPos.TryGetValue(nextTab, out var pos))
		{
			scroller.Inner.transform.localPosition = new Vector3(
				curPos.x,
				Mathf.Clamp(
					-(pos.y - 1.0f),
					scrollerMin,
					scrollerMax),
				curPos.z);
		}

		scroller.UpdateScrollBars();
	}

}
