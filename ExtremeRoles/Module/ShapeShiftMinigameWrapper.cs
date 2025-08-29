using UnityEngine;
using AmongUs.GameOptions;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Ability;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace ExtremeRoles.Module;

public sealed class ShapeShiftMinigameWrapper
{
	public bool IsOpen { get; private set; }
	private ShapeshifterMinigame? prefab = null;

	public bool OpenUi(System.Action<PlayerControl> playerSelectAction)
	{
		if (this.prefab == null)
		{
			var shapeShifterBase = RoleManager.Instance.AllRoles.FirstOrDefault(
				x => x.Role is RoleTypes.Shapeshifter);
			if (!shapeShifterBase.IsTryCast<ShapeshifterRole>(out var shapeShifter))
			{
				return false;
			}
			this.prefab = Object.Instantiate(
				shapeShifter.ShapeshifterMenu,
				PlayerControl.LocalPlayer.transform);
			this.prefab.gameObject.SetActive(false);
		}

		var game = MinigameSystem.Open(this.prefab);
		var overider = game.gameObject.TryAddComponent<ShapeshifterMinigameShapeshiftOverride>();
		overider.AddSelectPlayerAction(playerSelectAction);
		overider.AddCloseAction(() => this.IsOpen = false);
		this.IsOpen = true;

		return true;
	}

	public void Reset()
	{
		this.IsOpen = false;
	}
}
