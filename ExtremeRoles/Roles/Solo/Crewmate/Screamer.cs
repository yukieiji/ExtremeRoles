using System;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Implemented;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScreamerAbilityHandler(IUnityObjectFactory? factory = null) : IAbility, IExiledAnimationOverrideWhenExiled
{
	private readonly IUnityObjectFactory factory = factory ?? new DefaultUnityObjectFactory();

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

	public GameObject? CreateScreamEffect(byte rolePlayerId, float imageSize)
	{
		DeadBody? deadBody = GameSystem.GetDeadBody(rolePlayerId);
		if (deadBody == null)
		{
			return null;
		}

		int index = GetRandomScreamIndex();
		string imagePath = string.Format(ObjectPath.ScreamerScreamFormat, index);
		Sprite screamSprite = UnityObjectLoader.LoadSpriteFromResources(imagePath);

		GameObject screamObj = this.factory.CreateGameObject("ScreamerScreamEffect");

		SpriteRenderer renderer = screamObj.TryAddComponent<SpriteRenderer>();
		renderer.sprite = screamSprite;

		screamObj.transform.SetParent(deadBody.transform, false);
		screamObj.transform.localPosition = new Vector3(0f, 0f, -1f);
		screamObj.transform.localScale = new Vector3(imageSize, imageSize, 1f);

		return screamObj;
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

		if (this.AbilityClass is ScreamerAbilityHandler handler)
		{
			handler.CreateScreamEffect(rolePlayer.PlayerId, this.screamImageSize);
		}
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
