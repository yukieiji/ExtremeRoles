using AmongUs.GameOptions;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Extension.Player;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.CustomMonoBehaviour.Overrider;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.Solo.Impostor.RemoteKiller;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#nullable enable

namespace ExtremeRoles.Module;

public sealed class DefaultPanelBuilder(System.Action<PlayerControl> onSelect) : IPanelBuilder
{
	private readonly System.Action<PlayerControl> onSelect = onSelect;
	public IEnumerable<PanelArgs> Args
	{
		get
		{
			var bodies = Object.FindObjectsOfType<DeadBody>().Select(b => b.ParentId).ToHashSet();

			var list = PlayerCache.AllPlayerControl.Where(
				(PlayerControl p) =>
					p.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
					(!p.Data.IsDead || (p.Data.IsDead && bodies.Contains(p.PlayerId))));

			foreach (var player in list)
			{
				bool flag = PlayerControl.LocalPlayer.Data.Role.NameColor == player.Data.Role.NameColor;

				yield return new PanelArgs(
					player.Data.PlayerName,
					() => this.onSelect.Invoke(player),
					flag ? player.Data.Role.NameColor : Color.white,
					new(player.Data, false, false));
			}
		}
	}
}

public sealed class PlayerFilltedSelectPanelBuilder(System.Action<PlayerControl> onSelect, IReadOnlySet<byte> fillter) : IPanelBuilder
{
	private readonly IReadOnlySet<byte> filter = fillter;
	private readonly System.Action<PlayerControl> onSelect = onSelect;

	public IEnumerable<PanelArgs> Args
	{
		get
		{
			foreach (var player in PlayerCache.AllPlayerControl)
			{
				if (player.IsInValid() ||
					!this.filter.Contains(player.PlayerId))
				{
					continue;
				}

				yield return new PanelArgs(
					player.Data.PlayerName,
					() => onSelect.Invoke(player),
					Color.white,
					new(player.Data, false, false));
			}
		}
	}
}


public sealed class RoomSelectPanelBuilder(System.Action<SystemTypes> onSelect) : IPanelBuilder
{
	private readonly System.Action<SystemTypes> onSelect = onSelect;

	public IEnumerable<PanelArgs> Args
	{
		get
		{
			foreach (var room in ShipStatus.Instance.AllRooms)
			{
				if (room == null ||
					room.roomArea == null)
				{
					continue;
				}
				var id = room.RoomId;
				yield return new PanelArgs(
					TranslationController.Instance.GetString(id), () => this.onSelect.Invoke(id));
			}
		}
	}
}

public sealed class ShapeShiftMinigameWrapper()
{
	public bool IsOpen { get; private set; }
	private ShapeshifterMinigame? prefab = null;

	public bool OpenUi(IPanelBuilder builder)
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

		var overider = game.gameObject.TryAddComponent<ModdedShapeShifterMinigame>();
		overider.Builder = builder;
		overider.CloseAction += () => this.IsOpen = false;
		overider.Awake();
		overider.Begin(null);

		this.IsOpen = true;

		return true;
	}

	public void Reset()
	{
		this.IsOpen = false;
	}
}

