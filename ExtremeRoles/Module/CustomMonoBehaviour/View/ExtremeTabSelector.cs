using System;
using System.Collections.Generic;
using System.Linq;

using Il2CppInterop.Runtime.Attributes;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class ExtremeTabSelector(IntPtr ptr) : OptionBehaviour(ptr)
{
	[HideFromIl2Cpp]
	public IReadOnlyList<MapSelectButton> All => tabSelectButton;

	private List<MapSelectButton> tabSelectButton = new List<MapSelectButton>(8);
	private MapSelectButton? origin;
	private Collider2D? clickMask;

	private MapSelectButton? selectedButton;
	private SpriteRenderer? imgName;

	private float startX;
	private float spaceX;

	private Il2CppSystem.Collections.Generic.List<MapIconByName>? AllMapIcons;


	public void Awake()
	{
		if (!base.gameObject.TryGetComponent<GameOptionsMapPicker>(out var picker))
		{
			return;
		}

		Destroy(picker.Labeltext.transform.parent.gameObject);

		this.AllMapIcons = picker.AllMapIcons;
		this.origin = picker.MapButtonOrigin;
		this.imgName = picker.MapName;

		this.startX = picker.StartPosX - 1.9f;
		this.spaceX = picker.SpacingX * 1.25f;

		if (picker.mapButtons != null)
		{
			foreach (var button in picker.mapButtons)
			{
				Destroy(button.gameObject);
			}
			picker.mapButtons.Clear();
		}

		this.clickMask = picker.ButtonClickMask;

		Destroy(picker);
	}

	[HideFromIl2Cpp]
	public void Initialize(int maskLayer, Action<OptionTab> onTabChange)
	{
		foreach(var rend in  base.GetComponentsInChildren<SpriteRenderer>(true))
		{
			rend.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
		}

		if (this.tabSelectButton.Count != 0)
		{
			foreach (var button in this.tabSelectButton)
			{
				Destroy(button.gameObject);
			}
		}
		this.tabSelectButton.Clear();

		if (this.origin == null || this.clickMask == null)
		{
			return;
		}

		this.selectedButton = null;

		foreach (var (index, tab) in Enum.GetValues<OptionTab>().Select((x, index) => (index, x)))
		{
			string tabName = tab.ToString().ToLower();
			var img = UnityObjectLoader.LoadFromResources<Sprite>(
				ObjectPath.SettingTabAsset,
				string.Format(ObjectPath.SettingTabImage, tabName));

			MapSelectButton mapButton = Instantiate(this.origin, base.transform);

			mapButton.SetImage(img, maskLayer);

			var trans = mapButton.transform;
			trans.localScale *= 1.15f;
			trans.localPosition = new Vector3(this.startX + (float)index * this.spaceX, 0.74f, -2f);

			var button = mapButton.Button;
			button.ClickMask = this.clickMask;
			button.SelectButton(false);
			button.OnClick.AddListener(() =>
			{
				if (this.selectedButton != null)
				{
					this.selectedButton.Button.SelectButton(false);
				}
				this.selectedButton = mapButton;
				this.selectedButton.Button.SelectButton(true);

				if (this.imgName != null)
				{
					this.imgName.sprite = UnityObjectLoader.LoadFromResources<Sprite>(
						ObjectPath.SettingTabAsset,
						string.Format(ObjectPath.SettingTabImage, $"{tabName}header"));
				}

				onTabChange.Invoke(tab);
			});

			if (index > 0)
			{
				var prevButton = this.tabSelectButton[index - 1].Button;
				button.ControllerNav.selectOnLeft = prevButton;
				prevButton.ControllerNav.selectOnRight = button;
			}
			this.tabSelectButton.Add(mapButton);
		}
	}

	public void SelectDefault()
	{
		if (this.tabSelectButton.Count == 0)
		{
			return;
		}
		this.tabSelectButton[0].Button.OnClick.Invoke();
	}
}
