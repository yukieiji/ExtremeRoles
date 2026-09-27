using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
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

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo exiledPlayer)
	{
		int index = getRandomIndex();
		string text = index == 5 ? Tr.GetString("ScreamerExileRare") : Tr.GetString($"ScreamerExile{index}");
		return new OverrideInfo(exiledPlayer, text);
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

		string imageName = imageIndex == 5 ? "ScreamerRare" : $"Screamer{imageIndex}";
		Sprite sprite = this.resourcesProvider.LoadRoleSprite(ExtremeRoleId.Screamer, imageName);
		if (sprite != null)
		{
			renderer.sprite = sprite;
		}
	}

	private static int getRandomIndex()
	{
		int rand = RandomGenerator.Instance.Next(100);
		if (rand < 10)
		{
			return 5;
		}
		return ((rand - 10) % 4) + 1;
	}
}

public sealed class Screamer : SingleRoleBase
{
	public enum Option
	{
		IsScreamOnKill,
		ScreamImageSize,
	}

	private readonly IUnityObjectFactory unityObjectFactory;
	private readonly IResourcesProvider resourcesProvider;

	public Screamer() : this(new DefaultUnityObjectFactory(), new DefaultResourcesProvider())
	{
	}

	public Screamer(
		IUnityObjectFactory unityObjectFactory,
		IResourcesProvider resourcesProvider) : base(
		RoleArgs.BuildCrewmate(
			ExtremeRoleId.Screamer,
			ColorPalette.ScreamerColor))
	{
		this.unityObjectFactory = unityObjectFactory;
		this.resourcesProvider = resourcesProvider;
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
			this.unityObjectFactory,
			this.resourcesProvider);
	}
}
