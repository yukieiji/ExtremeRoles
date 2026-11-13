using System;
using System.Collections.Generic;

using AmongUs.Data;
using UnityEngine;

using ExtremeRoles.Extension.Task;


#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour.Overrider;

public record ShapeShifterGameProp(
	ShapeshifterPanel PanelPrefab,
	UiElement BackButton,
	UiElement DefaultButtonSelected,
	float XStart,
	float XOffset,
	float YStart,
	float YOffset);

public record PanelPlayerInfo(NetworkedPlayerInfo Player, bool Flip = false, bool Name = false);

public record PanelArgs(string Text, Action OnSelect, Color? Color = null, PanelPlayerInfo? PlayerInfo = null);

public interface IPanelBuilder
{
	public IEnumerable<PanelArgs> Args { get; }
}

[Il2CppRegister]
public class ModdedShapeShifterMinigame(IntPtr ptr) : Minigame(ptr)
{
	public IPanelBuilder? Builder { private get; set; }

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

	public void Awake()
	{
		this.prop = getProp();
	}

	public override void Begin(PlayerTask? task)
	{
		if (this.Builder is null)
		{
			return;
		}

		var prop = getProp();
		this.index = 0;
		this.AbstractBegin(task);

		var list = new Il2CppSystem.Collections.Generic.List<UiElement>();
		foreach (var buttonArgs in this.Builder.Args)
		{
			var button = CreatePanel(buttonArgs.Text, () => OnSelect(buttonArgs.OnSelect), buttonArgs.Color, buttonArgs.PlayerInfo);
			list.Add(button.Button);
		}

		ControllerManager.Instance.OpenOverlayMenu(base.name, prop.BackButton, prop.DefaultButtonSelected, list, false);
	}

	protected ShapeshifterPanel CreatePanel(
		string text,
		Action onSelect,
		Color? color = null,
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
		shapeshifterPanel.NameText.color = color.HasValue ? color.Value : Color.white;
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

		this.prop = new(
			shapeshifter.PanelPrefab,
			shapeshifter.BackButton,
			shapeshifter.DefaultButtonSelected,
			shapeshifter.XStart,
			shapeshifter.XOffset,
			shapeshifter.YStart,
			shapeshifter.YOffset);
		return this.prop;
	}

	public void OnSelect(Action mainAction)
	{
		if (PlayerControl.LocalPlayer.inVent)
		{
			this.Hide();
			return;
		}
		mainAction.Invoke();
		this.Hide();
	}
}