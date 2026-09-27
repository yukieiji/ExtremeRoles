using System;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScreamerAbilityHandler : IAbility, IExiledAnimationOverrideWhenExiled
{
	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo exiledPlayer)
	{
		int index = GetRandomScreamIndex();
		return new OverrideInfo(exiledPlayer, Tr.GetString($"ScreamerExiledOverride{index}"));
	}

	public static int GetRandomScreamIndex()
	{
		// 10% chance for rare index 4, otherwise 0..3
		if (RandomGenerator.Instance.Next(100) < 10)
		{
			return 4;
		}
		return RandomGenerator.Instance.Next(0, 4);
	}
}

public sealed class Screamer : SingleRoleBase
{
	public enum Option
	{
		IsScreamWhenKilled,
		ScreamImageSize,
	}

	private bool isScreamWhenKilled;
	private float screamImageSize;

	public Screamer() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Screamer,
			ColorPalette.ScreamerSaffron))
	{
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (!this.isScreamWhenKilled)
		{
			return;
		}

		DeadBody? deadBody = GameSystem.GetDeadBody(rolePlayer.PlayerId);
		if (deadBody == null)
		{
			return;
		}

		int index = ScreamerAbilityHandler.GetRandomScreamIndex();
		string imagePath = string.Format(ObjectPath.ScreamerScreamFormat, index);
		Sprite screamSprite = UnityObjectLoader.LoadSpriteFromResources(imagePath);

		GameObject screamObj = new GameObject("ScreamerScreamEffect");

		SpriteRenderer renderer = screamObj.AddComponent<SpriteRenderer>();
		renderer.sprite = screamSprite;

		screamObj.transform.SetParent(deadBody.transform, false);
		screamObj.transform.localPosition = new Vector3(0f, 0f, -1f);
		screamObj.transform.localScale = new Vector3(this.screamImageSize, this.screamImageSize, 1f);
	}

	protected override void CreateSpecificOption(
		AutoParentSetOptionCategoryFactory factory)
	{
		var isScreamOpt = factory.CreateBoolOption(
			Option.IsScreamWhenKilled,
			true);

		factory.CreateIntOption(
			Option.ScreamImageSize,
			100, 10, 1000, 10,
			activator: new ParentActive(isScreamOpt),
			format: OptionUnit.Percentage);
	}

	protected override void RoleSpecificInit()
	{
		var loader = this.Loader;
		this.isScreamWhenKilled = loader.GetValue<Option, bool>(Option.IsScreamWhenKilled);
		this.screamImageSize = loader.GetValue<Option, int>(Option.ScreamImageSize) / 100.0f;

		this.AbilityClass = new ScreamerAbilityHandler();
	}
}
