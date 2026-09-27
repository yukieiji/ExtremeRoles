using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.API.Interface.Ability;
using ExtremeRoles.Resources;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScreamerAbilityHandler(Screamer role) : IAbility, IExiledAnimationOverride
{
	private readonly Screamer role = role;

	public OverrideInfo? OverrideInfo
	{
		get
		{
			var exiledPlayer = this.role.GetScreamerPlayerInfo();
			string text = this.role.GetRandomExileText();
			return new OverrideInfo(exiledPlayer, text);
		}
	}
}

public sealed class Screamer : SingleRoleBase
{
	public enum Option
	{
		IsScreamOnKill,
		ScreamImageSize,
	}

	public bool IsScreamOnKill => isScreamOnKill;
	public float ScreamImageScale => screamImageScale;

	private bool isScreamOnKill;
	private float screamImageScale;

	public Screamer() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Screamer,
			ColorPalette.ScreamerColor))
	{
		this.AbilityClass = new ScreamerAbilityHandler(this);
	}

	public NetworkedPlayerInfo? GetScreamerPlayerInfo()
	{
		foreach (var (playerId, role) in ExtremeRoleManager.GameRole)
		{
			if (role == this || (role is MultiAssignRoleBase multi && multi.AnotherRole == this))
			{
				if (Helper.Player.TryGetPlayerInfo(playerId, out var info))
				{
					return info;
				}
			}
		}
		return null;
	}

	public string GetRandomExileText()
	{
		int rand = RandomGenerator.Instance.Next(100);
		if (rand < 10)
		{
			return Tr.GetString("ScreamerExileRare");
		}
		int commonIdx = ((rand - 10) % 4) + 1;
		return Tr.GetString($"ScreamerExile{commonIdx}");
	}

	public int GetRandomImageIndex()
	{
		int rand = RandomGenerator.Instance.Next(100);
		if (rand < 10)
		{
			return 5; // Rare image
		}
		return ((rand - 10) % 4) + 1; // Common images 1..4
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (!this.isScreamOnKill)
		{
			return;
		}

		int imageIndex = GetRandomImageIndex();
		SpawnScreamImage(rolePlayer, imageIndex, this.screamImageScale);
	}

	public static GameObject? SpawnScreamImage(
		PlayerControl rolePlayer,
		int imageIndex,
		float scale)
	{
		DeadBody? targetBody = null;
		try
		{
			DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
			if (array != null)
			{
				for (int i = 0; i < array.Length; ++i)
				{
					if (array[i].ParentId == rolePlayer.PlayerId)
					{
						targetBody = array[i];
						break;
					}
				}
			}
		}
		catch
		{
			// Ignore in unmocked environment
		}

		try
		{
			GameObject screamObj = new GameObject("ScreamerScreamImage");
			if (targetBody != null && targetBody.transform != null)
			{
				screamObj.transform.SetParent(targetBody.transform, false);
				screamObj.transform.localPosition = Vector3.zero;
			}
			else if (rolePlayer != null && rolePlayer.transform != null)
			{
				screamObj.transform.position = rolePlayer.transform.position;
			}

			if (screamObj.transform != null)
			{
				screamObj.transform.localScale = new Vector3(scale, scale, 1.0f);
			}

			var renderer = screamObj.AddComponent<SpriteRenderer>();
			if (renderer != null)
			{
				renderer.sortingOrder = 100;

				Sprite? sprite = LoadScreamSprite(imageIndex);
				if (sprite != null)
				{
					renderer.sprite = sprite;
				}
			}

			return screamObj;
		}
		catch
		{
			return null;
		}
	}

	public static Sprite? LoadScreamSprite(int imageIndex)
	{
		try
		{
			string imageName = imageIndex == 5 ? "ScreamerRare" : $"Screamer{imageIndex}";
			return UnityObjectLoader.LoadFromResources<Sprite, ExtremeRoleId>(
				ExtremeRoleId.Screamer,
				ObjectPath.GetRoleImgPath(ExtremeRoleId.Screamer, imageName));
		}
		catch
		{
			return null;
		}
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		factory.CreateBoolOption(
			Option.IsScreamOnKill,
			true);
		factory.CreateIntOption(
			Option.ScreamImageSize,
			100, 10, 1000, 10,
			format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		this.isScreamOnKill = this.Loader.GetValue<Option, bool>(
			Option.IsScreamOnKill);
		this.screamImageScale = this.Loader.GetValue<Option, int>(
			Option.ScreamImageSize) / 100.0f;
	}
}
