using System.Linq;


using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.Helper;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using System.Collections.Generic;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class ShapeShiftMinigameWrapper
{
	public bool IsOpen { get; private set; }
	private ShapeshifterMinigame? prefab = null;

	public bool OpenUi(System.Action<PlayerControl> playerSelectAction, IReadOnlySet<byte>? playerTagets = null)
	{
		if (this.prefab == null)
		{
			var shapeShifterBase = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);
			if (!shapeShifterBase.IsTryCast<ShapeshifterRole>(out var shapeShifter))
			{
				return false;
			}
			this.prefab = Object.Instantiate(
				shapeShifter.ShapeshifterMenu,
				PlayerControl.LocalPlayer.transform);
			this.prefab.gameObject.SetActive(false);
		}

		var game = MinigameSystem.Create(this.prefab);
		Minigame newMinigame;
		if (playerTagets is null)
		{
			var overider = game.gameObject.TryAddComponent<DefaultButtonShapeshifterMinigameShapeshiftOverride>();
			overider.SelectedAction = playerSelectAction;
			overider.CloseAction += () => this.IsOpen = false;
			overider.Awake();
			newMinigame = overider;
		}
		else
		{
			var overider = game.gameObject.TryAddComponent<FilteredShapeshifterMinigameShapeshiftOverride>();
			overider.SelectedAction = playerSelectAction;
			overider.Target = playerTagets;
			overider.CloseAction += () => this.IsOpen = false;
			overider.Awake();
			newMinigame = overider;
		}
		newMinigame.Begin(null);

		this.IsOpen = true;

		return true;
	}

	public void Reset()
	{
		this.IsOpen = false;
	}
}

