using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Il2CppUiElementList = Il2CppSystem.Collections.Generic.List<UiElement>;

using ExtremeRoles.Extension.Option;
using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.View;
using ExtremeRoles.GameMode;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class ExtremeGameOptionsMenuView(IntPtr ptr) : MonoBehaviour(ptr)
{
	public OptionCategory[]? AllCategory { private get; set; }

	private readonly List<OptionGroupViewObject<ExtremeOptionView>> optionGroupViewObject = new();
	private readonly Il2CppUiElementList allUiElement = new();
	private const float initY = 2.0f;

	private Scroller? scroller;
	private UiElement? backButton;
	private UiElement? firstButton;

	private Transform? settingsContainer;
	private CategoryHeaderMasked? categoryHeaderOrigin;
	private ExtremeOptionView? optionPrefab;

	private Collider2D? buttonClickMask;
	private const float blockTime = 0.25f;
	private float blockTimer = blockTime;

	public void Awake()
	{
		if (!base.TryGetComponent(out GameOptionsMenu menu))
		{
			return;
		}
		menu.MaskBg.material.SetInt(PlayerMaterial.MaskLayer, 20);
		menu.MaskArea.material.SetInt(PlayerMaterial.MaskLayer, 20);

		this.scroller = menu.scrollBar;
		this.backButton = menu.BackButton;
		this.settingsContainer = menu.settingsContainer;
		this.categoryHeaderOrigin = menu.categoryHeaderOrigin;

		this.optionPrefab = Instantiate(menu.stringOptionOrigin).gameObject.AddComponent<ExtremeOptionView>();
		this.optionPrefab.gameObject.SetActive(false);

		this.buttonClickMask = menu.ButtonClickMask;

		Destroy(menu.MapPicker.gameObject);
		foreach (var child in menu.Children)
		{
			Destroy(child.gameObject);
		}
		var allCate = menu.settingsContainer.GetComponentsInChildren<CategoryHeaderMasked>();
		foreach (var cate in allCate)
		{
			Destroy(cate.gameObject);
		}
		menu.Children.Clear();
		Destroy(menu);
	}

	public void FixedUpdate()
	{
		if (this.AllCategory is null)
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
		foreach (var cat in this.AllCategory)
		{
			isRefresh = isRefresh || cat.IsDirty;
			cat.IsDirty = false;
		}
		if (isRefresh)
		{
			this.Refresh();
		}
	}

	public void Refresh()
	{
		if (this.scroller == null ||
			this.AllCategory is null ||
			this.AllCategory.Length == 0)
		{
			return;
		}

		float yPos = initY;
		foreach (var (catego, groupViewObj) in Enumerable.Zip(this.AllCategory, this.optionGroupViewObject))
		{
			if (!(
					catego.Tab is CustomOption.OptionTab.General ||
					ExtremeGameModeManager.Instance.RoleSelector.IsValidCategory(catego.Id)
				))
			{
				continue;
			}


			var categoObj = groupViewObj.Category;
			categoObj.transform.localPosition = new Vector3(-0.903f, yPos, -2f);
			categoObj.ReplaceExRText(
				Helper.Translation.GetString(catego.Name), 20);

			yPos -= 0.63f;

			foreach (var (option, optionObj) in Enumerable.Zip(catego.Options, groupViewObj.Options))
			{
				bool isActive = option.IsActiveAndEnable;

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

	public void OnEnable()
	{
		if (this.optionGroupViewObject.Count == 0)
		{
			var allOpt = this.initializeOption();
			this.initializeUiElement();
			this.initializeControllerNavigation(allOpt);
		}
		this.Refresh();
	}

	public void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Open()
	{
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.backButton, this.firstButton, this.allUiElement, false);
	}

	private IReadOnlyList<OptionBehaviour> initializeOption()
	{
		var result = new List<OptionBehaviour>();
		if (this.categoryHeaderOrigin == null ||
			this.optionPrefab == null ||
			this.AllCategory is null)
		{
			return result;
		}

		this.optionGroupViewObject.Capacity = this.AllCategory.Length;
		foreach (var catego in this.AllCategory)
		{
			var categoryHeaderMasked = Instantiate(
				this.categoryHeaderOrigin, Vector3.zero, Quaternion.identity,
				this.settingsContainer);
			categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;

			var optionGroupViewObject = new OptionGroupViewObject<ExtremeOptionView>(
				categoryHeaderMasked, catego.Count);

			foreach (var option in catego.Options)
			{
				var opt = Instantiate(
					this.optionPrefab, Vector3.zero, Quaternion.identity, this.settingsContainer);
				opt.SetClickMask(this.buttonClickMask);
				opt.SetMaterialLayer(20);

				opt.OptionModel = option;
				opt.OptionCategoryModel = catego;

				optionGroupViewObject.Options.Add(opt);
				result.Add(opt);
			}
			this.optionGroupViewObject.Add(optionGroupViewObject);
		}

		return result;
	}

	private void initializeUiElement()
	{
		if (this.scroller == null)
		{
			return;
		}

		var arr = this.scroller.GetComponentsInChildren<UiElement>();
		foreach (var element in arr)
		{
			this.allUiElement.Add(element);
		}
		this.firstButton = this.allUiElement[0];
	}

	private void initializeControllerNavigation(in IReadOnlyList<OptionBehaviour> allOpt)
	{
		for (int i = 0; i < allOpt.Count; i++)
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
			if (i + 1 < allOpt.Count)
			{
				array2 = allOpt[i + 1].GetComponentsInChildren<UiElement>(true);
			}
			UiElement[] componentsInChildren = optionBehaviour.GetComponentsInChildren<UiElement>(true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				var nav = componentsInChildren[j].ControllerNav;
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
	private void resetTimer()
	{
		this.blockTimer = blockTime;
	}
}
