using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using System;
using UnityEngine;
using static ExtremeRoles.Roles.Solo.Impostor.Scavenger;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ScavengerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ScavengerAsset Test -----");

		foreach (var ab in Enum.GetValues<Ability>())
		{
			if (ab is Ability.ScavengerNull)
			{
				continue;
			}
			LoadSprite(ab);

			if (ab is
					Ability.ScavengerHandGun or
					Ability.ScavengerSword or
					Ability.ScavengerFlame)
			{
				LoadObj<Sprite>($"{ab}.{ObjectPath.MapIcon}.png");
			}
		}

		LoadObj<GameObject>(ObjectPath.ScavengerFlame);
		LoadObj<GameObject>(ObjectPath.ScavengerFlameFire);
		LoadObj<Sprite>($"{ObjectPath.ScavengerBulletImg}.png");
		LoadObj<Sprite>($"{ObjectPath.ScavengerBeamImg}.png");
		LoadObj<Sprite>($"{Ability.ScavengerSword}.png");

		LoadFromExR(ExtremeRoleId.Scavenger, ObjectPath.ScavengerBulletImg);
		yield break;
	}

	private void LoadObj<T>(string name) where T : UnityEngine.Object
	{
		try
		{
			var sprite = GetFromAsset<T>(name);
			Log.LogInfo($"Scavenger asset Loaded:{name}");
			NullCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Scavenger asset Loaded:{name} not load   {ex.Message}");
		}
	}

	private void LoadSprite(Ability abilityType)
	{
		try
		{
			var sprite = IWeapon.GetSprite(abilityType);
			Log.LogInfo($"Scavenger sprite asset Loaded:{abilityType}");
			NullCheck(sprite);
		}
		catch (Exception ex)
		{
			Log.LogError(
				$"Scavenger sprite Loaded:{abilityType} not load   {ex.Message}");
		}
	}
}
