using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Performance;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Impostor;

#nullable enable

public sealed class Crewshroom : SingleRoleBase, IRoleAbility
{
	public ExtremeAbilityButton Button { get; set; }

#pragma warning disable CS8618
	public Crewshroom() : base(
		ExtremeRoleId.Crewshroom,
		ExtremeRoleType.Impostor,
		ExtremeRoleId.Crewshroom.ToString(),
		Palette.ImpostorRed,
		true, false, true, true)
	{ }
#pragma warning restore CS8618

	public void CreateAbility()
	{
		this.CreateNormalAbilityButton(
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

		ModedMushroomSystem.RpcSetModMushroom(setPos);
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
		ExtremeSystemTypeManager.Instance.TryAdd(
			ModedMushroomSystem.Type,
			new ModedMushroomSystem());
	}
}
