using ExtremeRoles.Module;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExtremeRoles.Module.NewOption;
using ExtremeRoles.Module.NewOption.Implemented;

using UnityEngine;
using ExtremeRoles.Extension.Option;
using ExtremeRoles.Module.CustomOption;
using ExtremeRoles.Module.NewOption.View;

using ExtremeRoles.Extension.UnityEvents;

using UnityObject = UnityEngine.Object;
using Il2CppUiElementList = Il2CppSystem.Collections.Generic.List<UiElement>;
using System.Runtime.CompilerServices;
using TMPro;
using ExtremeRoles.Module.NewOption.Interfaces;

using AuController = Controller;
using ExtremeRoles.Extension.Il2Cpp;


#nullable enable

namespace ExtremeRoles.Patches.Option;

public sealed class GameSettingMenuDecorator : IDisposable
{
	private const float yScale = 0.85f;

	public PassiveButton NewTagButton
	{
		get
		{
			var newButton = UnityObject.Instantiate(this.buttonPrefab);

			newButton.transform.SetParent(this.buttonPrefab.transform.parent);
			newButton.transform.localPosition = this.buttonPrefab.transform.localPosition;
			newButton.transform.localScale = new Vector3(0.35f, 0.7f, 1.0f);
			rescaleText(newButton, 0.7f, 0.4f);

			return newButton;
		}
	}

	public ExtremeGameOptionsMenu NewMenu
	{
		get
		{
			var newMenu = UnityObject.Instantiate(this.tagPrefab);
			newMenu.transform.SetParent(this.tagPrefab.transform.parent);
			newMenu.transform.localPosition = this.tagPrefab.transform.localPosition;
			return newMenu.gameObject.AddComponent<ExtremeGameOptionsMenu>();
		}
	}
	public GameSettingMenu Menu { get; }
	public Vector3 FirstButtonPos { get; } = new Vector3(-3.875f, -2.5f, -2.0f);

	private readonly PassiveButton buttonPrefab;
	public readonly GameOptionsMenu tagPrefab;

	public GameSettingMenuDecorator(GameSettingMenu menu)
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

		this.buttonPrefab = UnityObject.Instantiate(menu.GameSettingsButton);
		this.buttonPrefab.transform.SetParent(menu.GameSettingsButton.transform.parent);
		this.buttonPrefab.transform.localPosition = this.FirstButtonPos;
		this.buttonPrefab.transform.localScale = new Vector3(0.4f, 0.8f, 1.0f);
		rescaleText(this.buttonPrefab, 0.8f, 0.4f);
		this.buttonPrefab.OnClick.RemoveAllListeners();
		this.buttonPrefab.OnMouseOver.RemoveAllListeners();


		this.tagPrefab = UnityObject.Instantiate(menu.GameSettingsTab);
		this.tagPrefab.transform.SetParent(menu.GameSettingsTab.transform.parent);
		this.tagPrefab.transform.localPosition = menu.GameSettingsTab.transform.localPosition;



		// 一時的なもの
		var exrMenu = this.tagPrefab.gameObject.AddComponent<ExtremeGameOptionsMenu>();
		if (NewOptionManager.Instance.TryGetTab(OptionTab.General, out var container))
		{
			exrMenu.AllCategory = container.Category.ToArray();
		}
		exrMenu.Awake();
		this.buttonPrefab.OnClick.AddListener(() =>
		{
			menu.GamePresetsButton.SelectButton(false);
			menu.GameSettingsButton.SelectButton(false);
			menu.RoleSettingsButton.SelectButton(false);

			this.buttonPrefab.SelectButton(true);

			menu.ToggleLeftSideDarkener(true);
			menu.ToggleRightSideDarkener(false);

			menu.PresetsTab.gameObject.SetActive(false);
			menu.GameSettingsTab.gameObject.SetActive(false);
			menu.RoleSettingsTab.gameObject.SetActive(false);

			exrMenu.gameObject.SetActive(true);
			exrMenu.Open();

		});

	}

	private static void rescaleText(in PassiveButton button, in float xScale, float yScale = 1.0f)
	{
		var scale = button.buttonText.transform.localScale;
		button.buttonText.transform.localScale = new Vector3(scale.x * xScale, scale.y * yScale, scale.z);
	}


	public void Dispose()
	{
		UnityObject.Destroy(this.buttonPrefab.gameObject);
		UnityObject.Destroy(this.tagPrefab.gameObject);
	}
}

[Il2CppRegister]
public sealed class ExtremeOptionView(IntPtr ptr) : OptionBehaviour(ptr)
{
	private TextMeshPro? titleText;
	private TextMeshPro? valueText;

	public IOption? OptionModel { private get; set; }
	public OptionCategory? OptionCategoryModel { private get; set; }

	public void Awake()
	{
		if (!base.TryGetComponent(out StringOption opt))
		{
			return;
		}
		this.titleText = opt.TitleText;
		this.valueText = opt.ValueText;

		if (base.transform.Find("MinusButton (1)").TryGetComponent(out PassiveButton minus))
		{
			minus.OnClick.RemoveAllListeners();
			minus.OnClick.AddListener(this.Decrease);
		}
		if (base.transform.Find("PlusButton (1)").TryGetComponent(out PassiveButton plus))
		{
			plus.OnClick.RemoveAllListeners();
			plus.OnClick.AddListener(this.Increase);
		}

		Destroy(opt);
	}

	public void Decrease()
	{
		if (OptionModel is null ||
			OptionCategoryModel is null)
		{
			return;
		}
		NewOptionManager.Instance.Update(OptionCategoryModel, OptionModel, -1);
	}
	public void Increase()
	{
		if (OptionModel is null ||
			OptionCategoryModel is null)
		{
			return;
		}
		NewOptionManager.Instance.Update(OptionCategoryModel, OptionModel, 1);
	}

	public void SetMaterialLayer(int maskLayer)
	{
		var rends = base.GetComponentsInChildren<SpriteRenderer>(true);
		foreach (var rend in rends)
		{
			rend.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		}

		var textMeshPros = base.GetComponentsInChildren<TextMeshPro>(true);
		foreach (TextMeshPro textMeshPro in textMeshPros)
		{
			textMeshPro.fontMaterial.SetFloat("_StencilComp", 3f);
			textMeshPro.fontMaterial.SetFloat("_Stencil", maskLayer);
		}
	}

	public void Refresh()
	{
		if (this.OptionModel is null)
		{
			return;
		}

		if (this.titleText != null)
		{
			this.titleText.text = this.OptionModel.Title;
		}
		if (this.valueText != null)
		{
			this.valueText.text = this.OptionModel.ValueString;
		}
	}
}


// TabのViewクラス
[Il2CppRegister]
public sealed class ExtremeGameOptionsMenu(IntPtr ptr) : MonoBehaviour(ptr)
{
	public OptionCategory[]? AllCategory { private get; set; }

	private readonly List<OptionGroupViewObject<ExtremeOptionView>> optionGroupViewObject = new ();
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
		this.optionPrefab = menu.stringOptionOrigin.gameObject.AddComponent<ExtremeOptionView>();
		this.buttonClickMask = menu.ButtonClickMask;

		Destroy(menu.MapPicker.gameObject);
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
			var categoObj = groupViewObj.Category;
			categoObj.transform.localPosition = new Vector3(-0.903f, yPos, -2f);
			categoObj.ReplaceExRText(catego.Name, 20);

			yPos -= 0.63f;

			foreach (var (option, optionObj) in Enumerable.Zip(catego.Options, groupViewObj.Options))
			{
				bool isActive = option.IsActiveAndEnable;

				optionObj.gameObject.SetActive(isActive);
				if (!isActive)
				{
					continue;
				}

				optionObj.transform.localPosition = new Vector3(0.952f, yPos, -2f);
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
				ExtremeOptionView opt = Instantiate(
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

[Il2CppRegister]
public sealed class ExtremeGameSettingMenu(IntPtr ptr) : MonoBehaviour(ptr)
{
	private readonly Dictionary<OptionTab, ExtremeGameOptionsMenu> allMenu = new(8);
	private readonly Dictionary<OptionTab, PassiveButton> allButton = new(8);

	private GameSettingMenu? menu;

	public void Initialize(in GameSettingMenuDecorator decorator)
	{
		this.menu = decorator.Menu;

		var firstPos = decorator.FirstButtonPos;
		int xIndex = 0;
		int yIndex = 0;

		foreach (var tab in Enum.GetValues<OptionTab>())
		{
			var menu = decorator.NewMenu;
			var button = decorator.NewTagButton;

			button.gameObject.name = $"{tab}Button";
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
		=> (isPreviewOnly && AuController.currentTouchType == AuController.TouchType.Joystick) || !isPreviewOnly;
}


[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
public static class GameSettingMenuStartPatch
{
	private const float yScale = 0.85f;

	public static void Postfix(GameSettingMenu __instance)
	{
		using var dec = new GameSettingMenuDecorator(__instance);
		var menu = __instance.gameObject.TryAddComponent<ExtremeGameSettingMenu>();
		menu.Initialize(dec);
	}
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab))]
public static class GameSettingMenuChangeTabPatch
{
	public static void Prefix(
		GameSettingMenu __instance,
		[HarmonyArgument(0)] int tabNum,
		[HarmonyArgument(1)] bool previewOnly)
	{
		if (__instance.TryGetComponent(out ExtremeGameSettingMenu menu))
		{
			menu.SwitchTabPrefix(previewOnly);
		}
	}
}
