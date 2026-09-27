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

public interface IScreamerResourceProvider
{
	public Sprite LoadScreamSprite(int index);
}

public sealed class ScreamerResourceProvider : IScreamerResourceProvider
{
	public Sprite LoadScreamSprite(int index)
	{
		return UnityObjectLoader.LoadFromResources(ExtremeRoleId.Screamer, $"Scream{index}");
	}
}

public sealed class ScreamerAbilityHandler(
	Func<float> getImageSize,
	IUnityObjectFactory? unityFactory = null,
	IScreamerResourceProvider? resourceProvider = null) : IAbility, IExiledAnimationOverrideWhenExiled
{
	private readonly Func<float> getImageSize = getImageSize;
	private readonly IUnityObjectFactory unityFactory = unityFactory ?? new DefaultUnityObjectFactory();
	private readonly IScreamerResourceProvider resourceProvider = resourceProvider ?? new ScreamerResourceProvider();

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo exiledPlayer)
	{
		int index = getRandomScreamIndex();
		return new OverrideInfo(exiledPlayer, Tr.GetString($"ScreamerExiledOverride{index}"));
	}

	private static int getRandomScreamIndex()
	{
		// 10% chance for rare index 4, otherwise 0..3
		if (RandomGenerator.Instance.Next(100) < 10)
		{
			return 4;
		}
		return RandomGenerator.Instance.Next(0, 4);
	}

	public void CreateScreamEffect(byte rolePlayerId)
	{
		DeadBody? deadBody = GameSystem.GetDeadBody(rolePlayerId);
		if (deadBody == null)
		{
			return;
		}

		int index = getRandomScreamIndex();
		Sprite screamSprite = this.resourceProvider.LoadScreamSprite(index);

		GameObject screamObj = this.unityFactory.CreateGameObject("ScreamerScreamEffect");

		SpriteRenderer renderer = screamObj.TryAddComponent<SpriteRenderer>();
		renderer.sprite = screamSprite;

		float size = this.getImageSize();

		screamObj.transform.SetParent(deadBody.transform, false);
		screamObj.transform.localPosition = new Vector3(0f, 0f, -1f);
		screamObj.transform.localScale = new Vector3(size, size, 1f);
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
			handler.CreateScreamEffect(rolePlayer.PlayerId);
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

		this.AbilityClass = new ScreamerAbilityHandler(() => this.screamImageSize);
	}
}
