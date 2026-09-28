using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface.Ability;

#nullable enable

namespace ExtremeRoles.Roles.Solo.Crewmate;

public sealed class ScreamerAbilityHandler(
	bool isScreamOnKill,
	float screamImageScale,
	IUnityObjectFactory unityObjectFactory,
	IResourcesProvider resourcesProvider) : IAbility, IExiledAnimationOverride
{
	private readonly bool isScreamOnKill = isScreamOnKill;
	private readonly float screamImageScale = screamImageScale;
	private readonly IUnityObjectFactory unityObjectFactory = unityObjectFactory;
	private readonly IResourcesProvider resourcesProvider = resourcesProvider;

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo? exiledPlayer)
	{
		int index = getRandomIndex();
		return new OverrideInfo(exiledPlayer, Tr.GetString($"ScreamerExile{index}"));
	}

	public void SpawnScreamImage(PlayerControl rolePlayer)
	{
		if (!this.isScreamOnKill)
		{
			return;
		}

		DeadBody? targetBody = GameSystem.GetDeadBody(rolePlayer.PlayerId);
		if (targetBody == null)
		{
			return;
		}

		int imageIndex = getRandomIndex();

		GameObject screamObj = this.unityObjectFactory.CreateGameObject("ScreamerScreamImage");
		screamObj.transform.SetParent(targetBody.transform, false);
		screamObj.transform.localPosition = Vector3.zero;
		screamObj.transform.localScale = new Vector3(this.screamImageScale, this.screamImageScale, 1.0f);

		SpriteRenderer renderer = screamObj.AddComponent<SpriteRenderer>();
		renderer.sortingOrder = 100;
		renderer.sprite = this.resourcesProvider.LoadRoleSprite(ExtremeRoleId.Screamer, $"Screamer{imageIndex}");
	}

	private static int getRandomIndex()
	{
		int rand = RandomGenerator.Instance.Next(100);
		return rand < 5 ? 4 : (rand - 5) % 4;
	}
}

public sealed class Screamer : SingleRoleBase
{
	public enum Option
	{
		IsScreamOnKill,
		ScreamImageSize,
	}

	public Screamer() : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Screamer,
			ColorPalette.ScreamerColor))
	{
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (this.AbilityClass is ScreamerAbilityHandler handler)
		{
			handler.SpawnScreamImage(rolePlayer);
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
		bool isScreamOnKill = this.Loader.GetValue<Option, bool>(Option.IsScreamOnKill);
		float scale = this.Loader.GetValue<Option, int>(Option.ScreamImageSize) / 100.0f;

		this.AbilityClass = new ScreamerAbilityHandler(
			isScreamOnKill,
			scale,
			new DefaultUnityObjectFactory(),
			new DefaultResourcesProvider());
	}
}
