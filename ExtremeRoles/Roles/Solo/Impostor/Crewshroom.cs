using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using System;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Crewshroom : SingleRoleBase, IRoleAbility
{
	public ExtremeAbilityButton Button { get; set; }
	private Mushroom? prefab = null;

#pragma warning disable CS8618
	public Crewshroom() : base(
		ExtremeRoleId.Crewshroom,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Crewshroom.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }
#pragma warning restore CS8618

	public static void SetMushroom(
		byte rolePlayerId)
	{
		var role = ExtremeRoleManager.GetSafeCastedRole<Crewshroom>(rolePlayerId);
		if (role == null) { return; }
	}
	private static void setMushroom(Crewshroom role, Vector2 pos)
	{
		if (role.prefab != null)
		{
			AssetReference fungleAsset = AmongUsClient.Instance.ShipPrefabs[5];

			if (!fungleAsset.IsValid()) { return; }

			GameObject obj = fungleAsset
				.OperationHandle
				.Result
				.Cast<GameObject>();
			role.prefab = obj.GetComponentInChildren<Mushroom>();
		}
	}

	public void CreateAbility()
	{
		this.CreateAbilityCountButton(
			"crack",
			Loader.CreateSpriteFromResources(
			   Path.CrackerCrack));
	}

	public bool IsAbilityUse() => this.IsCommonUse();

	public void ResetOnMeetingEnd(GameData.PlayerInfo? exiledPlayer = null)
	{
		return;
	}

	public void ResetOnMeetingStart()
	{
		return;
	}

	public bool UseAbility()
	{
		PlayerControl localPlayer = CachedPlayerControl.LocalPlayer;
		Vector2 setPos = localPlayer.GetTruePosition();

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.CrewshroomSetMushroom))
		{
			caller.WriteByte(localPlayer.PlayerId);
			caller.WriteFloat(setPos.x);
			caller.WriteFloat(setPos.y);
		}
		setMushroom(this, setPos);
		return true;
	}

	protected override void CreateSpecificOption(
		IOptionInfo parentOps)
	{
		this.CreateCommonAbilityOption(parentOps);
	}

	protected override void RoleSpecificInit()
	{
		this.RoleAbilityInit();
	}
}
