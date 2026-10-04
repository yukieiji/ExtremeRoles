using AmongUs.Data;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Extension.Task;
using ExtremeRoles.Module.CustomMonoBehaviour.WithAction;
using ExtremeRoles.Performance;
using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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

	protected record PanelPlayerInfo(NetworkedPlayerInfo Player, bool Flip = false, bool Name = false);

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
		shapeshifterPanel.NameText.color = color.HasValue ? color.Value: Color.white;
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
public sealed class DefaultButtonShapeshifterMinigameShapeshiftOverride(IntPtr ptr) : ModdedShapeShifterMinigameBase(ptr)
{
	public Action<PlayerControl>? SelectedAction { private get; set; }

	protected override IEnumerable<ShapeshifterPanel> CreateButton(ShapeShifterGameProp prop)
	{
		var bodies = FindObjectsOfType<DeadBody>().Select(b => b.ParentId).ToHashSet();

		var list = PlayerCache.AllPlayerControl.Where(
			(PlayerControl p) => 
				p.PlayerId != PlayerControl.LocalPlayer.PlayerId && 
				(!p.Data.IsDead || (p.Data.IsDead && bodies.Contains(p.PlayerId))));

		foreach (var player in list)
		{
			bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;

			yield return CreatePanel(
				player.Data.PlayerName,
				() => OnSelect(player),
				flag ? player.Data.Role.NameColor : Color.white,
				new(player.Data, false, false));
		}
	}

	public void OnSelect(PlayerControl target)
	{
		if (PlayerControl.LocalPlayer.inVent)
		{
			this.Hide();
			return;
		}
		if (target != null)
		{
			this.SelectedAction?.Invoke(target);
		}
		else
		{
			Logger.GlobalInstance.Warning("Shapeshift: target is null", null);
		}
		this.Hide();
	}
}

[Il2CppRegister]
public sealed class FilteredShapeshifterMinigameShapeshiftOverride(IntPtr ptr) : ModdedShapeShifterMinigameBase(ptr)
{
	public IReadOnlySet<byte>? Target { private get; set; }
	public Action<PlayerControl>? SelectedAction { private get; set; }

	protected override IEnumerable<ShapeshifterPanel> CreateButton(ShapeShifterGameProp prop)
	{
		foreach (var player in PlayerCache.AllPlayerControl)
		{
			if (this.Target == null ||
				player.IsInValid() ||
				!this.Target.Contains(player.PlayerId))
			{
				continue;
			}

			yield return CreatePanel(
				player.Data.PlayerName,
				() => OnSelect(player),
				Color.white,
				new(player.Data, false, false));
		}
	}

	public void OnSelect(PlayerControl target)
	{
		if (PlayerControl.LocalPlayer.inVent)
		{
			this.Hide();
			return;
		}
		if (target != null)
		{
			this.SelectedAction?.Invoke(target);
		}
		else
		{
			Logger.GlobalInstance.Warning("Shapeshift: target is null", null);
		}
		this.Hide();
	}
}
