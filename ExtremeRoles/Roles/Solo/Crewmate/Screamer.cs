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
	IUnityObjectFactory unityObjectFactory,
	IResourcesProvider resourcesProvider) : IAbility, IExiledAnimationOverride
{
	private readonly IUnityObjectFactory unityObjectFactory = unityObjectFactory;
	private readonly IResourcesProvider resourcesProvider = resourcesProvider;

	public OverrideInfo? GetOverrideInfo(NetworkedPlayerInfo exiledPlayer)
	{
		string text = getNextExileText();
		return new OverrideInfo(exiledPlayer, text);
	}

	public void SpawnScreamImage(
		PlayerControl rolePlayer,
		float scalePercentage)
	{
		int imageIndex = getNextImageIndex();
		DeadBody targetBody = findDeadBody(rolePlayer.PlayerId);

		GameObject screamObj = this.unityObjectFactory.CreateGameObject("ScreamerScreamImage");
		if (targetBody != null)
		{
			screamObj.transform.SetParent(targetBody.transform, false);
			screamObj.transform.localPosition = Vector3.zero;
		}
		else
		{
			screamObj.transform.position = rolePlayer.transform.position;
		}

		float scale = scalePercentage / 100.0f;
		screamObj.transform.localScale = new Vector3(scale, scale, 1.0f);

		SpriteRenderer renderer = screamObj.AddComponent<SpriteRenderer>();
		if (renderer != null)
		{
			renderer.sortingOrder = 100;

			string imageName = imageIndex == 5 ? "ScreamerRare" : $"Screamer{imageIndex}";
			Sprite sprite = this.resourcesProvider.LoadRoleSprite(ExtremeRoleId.Screamer, imageName);
			if (sprite != null)
			{
				renderer.sprite = sprite;
			}
		}
	}

	private static DeadBody findDeadBody(byte playerId)
	{
		DeadBody[] array = Object.FindObjectsOfType<DeadBody>();
		if (array != null)
		{
			for (int i = 0; i < array.Length; ++i)
			{
				if (array[i].ParentId == playerId)
				{
					return array[i];
				}
			}
		}
		return null!;
	}

	private static string getNextExileText()
	{
		int rand = RandomGenerator.Instance.Next(100);
		if (rand < 10)
		{
			return Tr.GetString("ScreamerExileRare");
		}
		int commonIdx = ((rand - 10) % 4) + 1;
		return Tr.GetString($"ScreamerExile{commonIdx}");
	}

	private static int getNextImageIndex()
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

	public bool IsScreamOnKill { get; private set; }
	public float ScreamImageScale { get; private set; }

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
		this.AbilityClass = new ScreamerAbilityHandler(unityObjectFactory, resourcesProvider);
	}

	public override void RolePlayerKilledAction(
		PlayerControl rolePlayer,
		PlayerControl killerPlayer)
	{
		if (!this.IsScreamOnKill)
		{
			return;
		}

		if (this.AbilityClass is ScreamerAbilityHandler handler)
		{
			handler.SpawnScreamImage(rolePlayer, this.ScreamImageScale);
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
		this.IsScreamOnKill = this.Loader.GetValue<Option, bool>(
			Option.IsScreamOnKill);
		this.ScreamImageScale = this.Loader.GetValue<Option, int>(
			Option.ScreamImageSize);
	}
}
