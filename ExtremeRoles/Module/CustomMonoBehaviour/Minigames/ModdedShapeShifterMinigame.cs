using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour.Minigames;

#nullable enable

public class ModdedShapeShifterMinigameWrapperBase
{
	public bool IsOpen { get; private set; }
	private ShapeshifterMinigame? prefab = null;

	public bool Open<T>() where T : ModdedShapeShifterMinigameBase
	{
		if (this.prefab == null)
		{
			var shapeShifterBase = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);
			if (!shapeShifterBase.IsTryCast<ShapeshifterRole>(out var shapeShifter))
			{
				return false;
			}
			this.prefab = UnityEngine.Object.Instantiate(
				shapeShifter.ShapeshifterMenu,
				PlayerControl.LocalPlayer.transform);
			this.prefab.gameObject.SetActive(false);
		}

		var minigameBase = MinigameSystem.Create(this.prefab);
		var minigame = minigameBase.gameObject.TryAddComponent<T>();
		minigame.Begin(null);
		minigame.CloseAction += () => this.IsOpen = false;
		this.IsOpen = true;
		return true;
	}

	public void Reset()
	{
		this.IsOpen = false;
	}
}

public record ShapeShifterGameProp(
	ShapeshifterPanel PanelPrefab,
	UiElement BackButton,
	UiElement DefaultButtonSelected,
	float XStart,
	float XOffset,
	float YStart,
	float YOffset);

[Il2CppRegister]
public class ModdedShapeShifterMinigameBase(IntPtr ptr) : Minigame(ptr)
{
	private ShapeShifterGameProp? prop = null;
	public event Action CloseAction
	{
		add
		{
			if (action == null)
			{
				action = value;
			}
			else
			{
				action += value;
			}
		}
		remove
		{
			if (action == null)
			{
				return;
			}
			action -= value;
		}
	}
	private Action? action;
	private int index;

	protected record PanelPlayerInfo(NetworkedPlayerInfo Player, bool Flip=false, bool Name=false);

	public void Awake()
	{
		this.prop = getProp();
	}

	public override void Begin(PlayerTask? task)
	{
		var prop = getProp();
		this.index = 0;
		this.AbstractBegin(task);

		var list = new Il2CppSystem.Collections.Generic.List<UiElement>();
		foreach (var button in CreateButton(prop))
		{
			list.Add(button.Button);
		}

		ControllerManager.Instance.OpenOverlayMenu(base.name, prop.BackButton, prop.DefaultButtonSelected, list, false);
	}

	protected ShapeshifterPanel CreatePanel(
		string text,
		Action onSelect,
		PanelPlayerInfo? panel = null)
	{
		var prop = getProp();

		int num = index % 3;
		int num2 = index / 3;
		ShapeshifterPanel shapeshifterPanel = Instantiate(prop.PanelPrefab, base.transform);
		shapeshifterPanel.transform.localPosition = new Vector3(
			prop.XStart + num * prop.XOffset,
			prop.YStart + num2 * prop.YOffset, -1f);

		shapeshifterPanel.shapeshift = onSelect;
		SpriteRenderer[] componentsInChildren = shapeshifterPanel.GetComponentsInChildren<SpriteRenderer>();
		foreach (var comp in shapeshifterPanel.GetComponentsInChildren<SpriteRenderer>())
		{
			comp.material.SetInt(PlayerMaterial.MaskLayer, index + 2);
		}

		shapeshifterPanel.NameText.text = text;
		shapeshifterPanel.shapeshift = onSelect;

		if (panel != null)
		{
			shapeshifterPanel.PlayerIcon.SetMaskLayer(index + 2);
			shapeshifterPanel.PlayerIcon.SetFlipX(panel.Flip);
			shapeshifterPanel.PlayerIcon.ToggleName(panel.Name);
			shapeshifterPanel.PlayerIcon.UpdateFromEitherPlayerDataOrCache(
				panel.Player, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, false, null);
			shapeshifterPanel.LevelNumberText.text = ProgressionManager.FormatVisualLevel(
				panel.Player.PlayerLevel);
			shapeshifterPanel.Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate(
				panel.Player.DefaultOutfit.NamePlateId).Image;
			shapeshifterPanel.SetColorblindText();
			DataManager.Settings.Accessibility.OnColorBlindModeChanged += new Action(shapeshifterPanel.SetColorblindText);
		}
		else
		{
			shapeshifterPanel.NameText.transform.localPosition = Vector3.zero;
			shapeshifterPanel.Background.sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
			shapeshifterPanel.PlayerIcon.gameObject.SetActive(false);
			shapeshifterPanel.LevelNumberText.gameObject.SetActive(false);
		}
		this.index++;
		return shapeshifterPanel;
	}

	protected void Hide()
	{
		this.action?.Invoke();
		this.AbstractClose();
	}

	public void OnDisable()
	{
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	protected virtual IEnumerable<ShapeshifterPanel> CreateButton(ShapeShifterGameProp prop)
	{
		throw new NotImplementedException();
	}

	private ShapeShifterGameProp getProp()
	{
		if (!TryGetComponent<ShapeshifterMinigame>(out var shapeshifter) &&
			this.prop == null)
		{
			throw new InvalidOperationException("Can't SetUp");
		}
		else if (this.prop != null)
		{
			return this.prop;
		}

		return new(
			shapeshifter.PanelPrefab,
			shapeshifter.BackButton,
			shapeshifter.DefaultButtonSelected,
			shapeshifter.XStart,
			shapeshifter.XOffset,
			shapeshifter.YStart,
			shapeshifter.YOffset);
	}
}


[Il2CppRegister]
public class ScannerScanSelectorMinigame(IntPtr ptr) : ModdedShapeShifterMinigameBase(ptr)
{
	protected override IEnumerable<ShapeshifterPanel> CreateButton(ShapeShifterGameProp prop)
	{
		foreach (var room in ShipStatus.Instance.AllRooms)
		{
			var id = room.RoomId;
			yield return CreatePanel(
				TranslationController.Instance.GetString(id), () => this.ScanPlayer(id));
		}
	}

	private void ScanPlayer(SystemTypes roomId)
	{
		// ここにロジックを書く
	}
}
