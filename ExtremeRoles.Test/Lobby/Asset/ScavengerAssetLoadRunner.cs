using ExtremeRoles.Resources;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.Solo.Impostor;
using UnityEngine;

namespace ExtremeRoles.Test.Lobby.Asset;

public class ScavengerAssetLoadRunner
	: AssetLoadRunner
{
	public override IEnumerator Run()
	{
		Log.LogInfo($"----- Unit:ScavengerAsset Test -----");

		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerSword}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerFlame}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerBeamSaber}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerHandGun}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerHandGun}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerSniperRifle}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerBeamRifle}.{ObjectPath.ButtonIcon}.png");
		loadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			"assets/roles/scavenger.flamefire.prefab");
		loadUnityObjectFromExR<GameObject, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			"assets/roles/scavenger.scavengerflame.prefab");

		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerHandGun}.{ObjectPath.MapIcon}.png");

		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{Scavenger.Ability.ScavengerFlame}.{ObjectPath.MapIcon}.png");
		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{ObjectPath.ScavengerBulletImg}.png");

		loadUnityObjectFromExR<Sprite, ExtremeRoleId>(
			ExtremeRoleId.Scavenger,
			$"assets/roles/scavenger.{ObjectPath.ScavengerBeamImg}.png");

		LoadFromExR(ExtremeRoleId.Scavenger, ObjectPath.ScavengerBulletImg);
		yield break;
	}

	private void loadUnityObjectFromExR<T, W>(W id, string name)
		where T : UnityEngine.Object
		where W : System.Enum
		=> LoadUnityObjectFromExR<T, W>(id, name.ToLower());
}
