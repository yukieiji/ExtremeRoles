using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.View;

[Il2CppRegister]
public sealed class ExtremeTabSelector : OptionBehaviour
{
	public IReadOnlyList<MapSelectButton> All => tabSelectButton;

	private List<MapSelectButton> tabSelectButton = new List<MapSelectButton>(8);
	private MapSelectButton? origin;
	private Collider2D? clickMask;

	private MapSelectButton? selectedButton;

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


	public void Initialize(int maskLayer, Action<OptionTab> onTabChange)
	{
		IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;

		SpriteRenderer[] componentsInChildren = base.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
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
			string tabName = tab.ToString();
			var img = UnityObjectLoader.LoadFromResources<Sprite>(
				ObjectPath.SettingTabAsset,
				string.Format(ObjectPath.SettingTabImage, tabName.Substring(0, tabName.Length - 3)));

			MapSelectButton mapButton = Instantiate(this.origin, base.transform);

			mapButton.SetImage(img, maskLayer);

			mapButton.transform.localScale *= 1.15f;
			mapButton.transform.localPosition = new Vector3(this.startX + (float)index * this.spaceX, 0.74f, -2f);
			mapButton.Button.ClickMask = this.clickMask;

			mapButton.Button.SelectButton(false);
			mapButton.Button.OnClick.AddListener(() =>
			{
				if (this.selectedButton != null)
				{
					this.selectedButton.Button.SelectButton(false);
				}
				this.selectedButton = mapButton;
				this.selectedButton.Button.SelectButton(true);
				onTabChange.Invoke(tab);
			});

			if (index > 0)
			{
				mapButton.Button.ControllerNav.selectOnLeft = this.tabSelectButton[index - 1].Button;
				this.tabSelectButton[index - 1].Button.ControllerNav.selectOnRight = mapButton.Button;
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
