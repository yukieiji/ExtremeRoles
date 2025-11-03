using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Il2CppUiElementList = Il2CppSystem.Collections.Generic.List<UiElement>;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Extension.Option;

using ExtremeRoles.Helper;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour.UIPart;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Resources;

using CategoryView = ExtremeRoles.Module.CustomOption.View.OptionCategoryViewObject<ExtremeRoles.Module.CustomMonoBehaviour.View.ExtremeOptionView>;
using ExtremeRoles.Module.CustomOption.OLDS;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;


public class TabView(TabView.Builder builder)
{
	public class Builder()
	{
		public IReadOnlyList<OptionBehaviour> AllOption => allOption;
		private readonly List<OptionBehaviour> allOption = new List<OptionBehaviour>();

		public IEnumerable<CategoryView> OptionCategoryView => optionCategoryView.Select(x => x.Build());
		private readonly List<CategoryView.Builder> optionCategoryView = new List<CategoryView.Builder>();

		public CategoryView.Builder AddCategoryObject(CategoryHeaderMasked category, int num)
		{
			var optionGroupViewObject = new CategoryView.Builder(category, num);
			optionCategoryView.Add(optionGroupViewObject);

			return optionGroupViewObject;
		}
		public void AddOption(OptionBehaviour behaviour)
		{
			this.allOption.Add(behaviour);
		}
		public TabView Build()
			=> new TabView(this);
	}

	public OptionBehaviour[] AllOptionView { get; } = builder.AllOption.ToArray();
	public CategoryView[] CategoryViewGroup { get; } = builder.OptionCategoryView.ToArray();
}


[Il2CppRegister]
public sealed class ExtremeGameOptionsMenuView(IntPtr ptr) : MonoBehaviour(ptr)
{
	private Scroller? scroller;
	private UiElement? backButton;
	private Transform? settingsContainer;
	private CategoryHeaderMasked? categoryHeaderOrigin;

	private ExtremeOptionView? optionPrefab;
	private ExtremeTabSelector? tabPicker;

	private Collider2D? buttonClickMask;
	private SimpleButton? button;

	private const float initY = 0.713f;

	private const float blockTime = 0.25f;
	private float blockTimer = blockTime;
	private OptionTab curTab = (OptionTab)byte.MaxValue;

	private readonly Dictionary<OptionTab, (OptionTabContainer, TabView)> allTabView = new Dictionary<OptionTab, (OptionTabContainer, TabView)>(8);

	private readonly Il2CppUiElementList uiElements = new Il2CppUiElementList();
	private readonly List<OptionBehaviour> Children = new List<OptionBehaviour>();

	public void Awake()
	{
		if (!base.TryGetComponent<GameOptionsMenu>(out var menu))
		{
			return;
		}

		menu.MaskBg.material.SetInt(PlayerMaterial.MaskLayer, 20);
		menu.MaskArea.material.SetInt(PlayerMaterial.MaskLayer, 20);

		this.scroller = menu.scrollBar;
		this.backButton = menu.BackButton;
		this.settingsContainer = menu.settingsContainer;
		this.categoryHeaderOrigin = menu.categoryHeaderOrigin;

		this.optionPrefab = Instantiate(
			menu.stringOptionOrigin,
			base.transform).gameObject.AddComponent<ExtremeOptionView>();
		this.optionPrefab.Awake();
		this.optionPrefab.gameObject.SetActive(false);

		this.buttonClickMask = menu.ButtonClickMask;
		this.tabPicker = menu.MapPicker.gameObject.AddComponent<ExtremeTabSelector>();


		foreach (var child in menu.Children)
		{
			if (child.gameObject == this.tabPicker.gameObject)
			{
				continue;
			}
			Destroy(child.gameObject);
		}
		foreach (var cate in menu.settingsContainer.GetComponentsInChildren<CategoryHeaderMasked>())
		{
			Destroy(cate.gameObject);
		}
		menu.Children.Clear();
		Destroy(menu);

		this.button = UnityObjectLoader.CreateSimpleButton(menu.transform);
		this.button.Layer = menu.gameObject.layer;
		this.button.Scale = new Vector3(0.625f, 0.3f, 1.0f);
		this.button.gameObject.transform.localPosition = new Vector3(4.0f, 0.575f);
		this.button.Text.text = Tr.GetString("RoleAssignFilter");
		this.button.Text.fontSize =
			this.button.Text.fontSizeMax =
			this.button.Text.fontSizeMin = 1.9f;
		this.button.ClickedEvent.AddListener(
			() =>
			{
				RoleAssignFilter.Instance.OpenEditor(
					base.transform.parent.parent.gameObject);
			});
	}

	public void FixedUpdate()
	{
		if (!this.allTabView.TryGetValue(this.curTab, out var item))
		{
			return;
		}

		if (this.blockTimer >= 0.0f)
		{
			this.blockTimer -= Time.fixedDeltaTime;
			return;
		}
		this.resetTimer();
		bool isRefresh = false;
		foreach (var cat in item.Item1.Category)
		{
			isRefresh = isRefresh || cat.IsDirty;
			cat.IsDirty = false;
		}
		if (isRefresh)
		{
			this.refresh(item.Item1, item.Item2);
		}
	}

	private void onTabChange(OptionTab tab)
	{
		if (!OptionManager.Instance.TryGetTab(tab, out var tabContainer))
		{
			return;
		}

		if (!this.allTabView.TryGetValue(tab, out var item))
		{
			var view = this.initializeTab(tabContainer);
			item = (tabContainer, view);
			this.allTabView.Add(tab, item);
			initializeControllerNavigation(item.Item2.AllOptionView);
		}
		if (this.allTabView.TryGetValue(this.curTab, out var oldView))
		{
			hide(oldView.Item2);
		}

		this.curTab = tab;
		refresh(item.Item1, item.Item2);
	}

	public void OnEnable()
	{
		if (this.Children.Count == 0 && this.tabPicker != null)
		{
			this.tabPicker.Initialize(20, onTabChange);
		}
		if (this.tabPicker != null)
		{
			this.tabPicker.SelectDefault();
		}
	}

	public void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Open()
	{
		var element = this.tabPicker != null && this.tabPicker.All.Count != 0 ?
			this.tabPicker.All[0].Button : this.backButton;
		ControllerManager.Instance.OpenOverlayMenu(
			base.name,
			this.backButton,
			element,
			this.uiElements, false);
	}


	[HideFromIl2Cpp]
	private TabView initializeTab(OptionTabContainer tab)
	{
		if (this.tabPicker == null)
		{
			throw new ArgumentNullException();
		}

		var tabBuilder = new TabView.Builder();
		tabBuilder.AddOption(this.tabPicker);

		if (this.categoryHeaderOrigin == null ||
			this.optionPrefab == null)
		{
			return tabBuilder.Build();
		}

		foreach (var catego in tab.Category)
		{
			var categoryHeaderMasked = Instantiate(
				this.categoryHeaderOrigin, Vector3.zero, Quaternion.identity,
				this.settingsContainer);
			categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;

			var optionGroupViewObject = tabBuilder.AddCategoryObject(categoryHeaderMasked, catego.Count);

			foreach (var option in catego.Options)
			{
				var opt = Instantiate(
					this.optionPrefab, Vector3.zero, Quaternion.identity, this.settingsContainer);
				opt.SetClickMask(this.buttonClickMask);
				opt.SetMaterialLayer(20);

				opt.OptionModel = option;
				opt.OptionCategoryModel = catego;

				optionGroupViewObject.Options.Add(opt);
				tabBuilder.AddOption(opt);
			}
		}

		return tabBuilder.Build();
	}

	[HideFromIl2Cpp]
	private void initializeControllerNavigation(in OptionBehaviour[] allOpt)
	{
		for (int i = 0; i < allOpt.Length; i++)
		{
			OptionBehaviour optionBehaviour = allOpt[i];
			if (!optionBehaviour.gameObject.activeSelf)
			{
				continue;
			}
			UiElement[]? array = null;
			UiElement[]? array2 = null;
			if (i - 1 >= 0)
			{
				array = allOpt[i - 1].GetComponentsInChildren<UiElement>(true);
			}
			if (i + 1 < allOpt.Length)
			{
				array2 = allOpt[i + 1].GetComponentsInChildren<UiElement>(true);
			}
			UiElement[] componentsInChildren = optionBehaviour.GetComponentsInChildren<UiElement>(true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				var component = componentsInChildren[j];
				var nav = component.ControllerNav;
				this.uiElements.Add(component);
				nav.mode = ControllerNavigation.Mode.Explicit;
				if (array != null && array.Length != 0)
				{
					if (i == 1)
					{
						nav.selectOnUp = array[0];
					}
					else if (j < array.Length)
					{
						nav.selectOnUp = array[j];
					}
					else
					{
						nav.selectOnUp = array[0];
					}
				}
				else
				{
					nav.selectOnUp = null;
				}
				if (array2 != null && array2.Length != 0)
				{
					if (i == 0)
					{
						nav.selectOnDown = array2[0];
					}
					else if (j < array2.Length)
					{
						nav.selectOnDown = array2[j];
					}
					else
					{
						nav.selectOnDown = array2[0];
					}
				}
				else
				{
					nav.selectOnDown = null;
				}
			}
		}
	}

	[HideFromIl2Cpp]
	public void refresh(OptionTabContainer tab, TabView view)
	{
		if (this.scroller == null)
		{
			return;
		}

		float yPos = initY;

		IReadOnlySet<int>? validOptionId = default;
		var instance = ExtremeGameModeManager.Instance;

		foreach (var (catego, groupViewObj) in tab.Category.Zip(view.CategoryViewGroup))
		{
			if (!OptionSplitter.TryGetValidOption(catego, out validOptionId))
			{
				continue;
			}


			var categoObj = groupViewObj.Category;
			if (catego.Color.HasValue)
			{
				categoObj.Background.color = catego.Color.Value;
			}
			categoObj.transform.localPosition = new Vector3(-0.903f, yPos, -2f);
			categoObj.ReplaceExRText(catego.TransedName, 20);
			categoObj.gameObject.SetActive(true);

			yPos -= 0.63f;

			foreach (var (option, optionObj) in catego.Options.Zip(groupViewObj.View))
			{
				if (!OptionSplitter.IsValidOption(validOptionId, option.Info.Id))
				{
					continue;
				}

				bool isActive = option.IsViewActive;

				optionObj.gameObject.SetActive(isActive);
				if (!isActive)
				{
					continue;
				}

				optionObj.transform.localPosition = new Vector3(1.25f, yPos, -2f);
				optionObj.Refresh();
				yPos -= 0.45f;
			}
		}
		this.scroller.SetYBoundsMax(-yPos - 1.65f);
	}

	[HideFromIl2Cpp]
	public void hide(TabView view)
	{
		foreach (var cate in view.CategoryViewGroup)
		{
			cate.Category.gameObject.SetActive(false);
			foreach (var opt in cate.View)
			{
				opt.gameObject.SetActive(false);
			}
		}
	}

	private void resetTimer()
	{
		this.blockTimer = blockTime;
	}
}

