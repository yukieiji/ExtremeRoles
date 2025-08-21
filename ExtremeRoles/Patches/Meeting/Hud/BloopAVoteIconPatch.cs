using AmongUs.GameOptions;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination.Avalon;


using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;



namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class MeetingHudBloopAVoteIconPatch
{
	public static bool Prefix(
		MeetingHud __instance,
		[HarmonyArgument(0)] NetworkedPlayerInfo voterPlayer,
		[HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
	{

		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		SpriteRenderer spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);

		var role = ExtremeRoleManager.GetLocalPlayerRole();

		bool canSeeVote =
			(role is Marlin marlin && marlin.CanSeeVote) ||
			(role is Assassin assassin && assassin.CanSeeVote);

		if (!GameManager.Instance.LogicOptions.GetAnonymousVotes() ||
			canSeeVote ||
			(
				PlayerControl.LocalPlayer.Data.IsDead &&
				ClientOption.Instance.GhostsSeeRole.Value &&
				!isVoteSeeBlock(role)
			))
		{
			PlayerMaterial.SetColors(voterPlayer.DefaultOutfit.ColorId, spriteRenderer);
		}
		else
		{
			PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
		}

		spriteRenderer.transform.SetParent(parent);
		spriteRenderer.transform.localScale = Vector3.zero;

		if (parent.TryGetComponent<PlayerVoteArea>(out var component))
		{
			spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
		}

		__instance.StartCoroutine(
			Effects.Bloop((float)index * 0.3f,
			spriteRenderer.transform, 1f, 0.5f));
		parent.GetComponent<VoteSpreader>().AddVote(spriteRenderer);

		return false;
	}

	private static bool isVoteSeeBlock(SingleRoleBase role)
	{
		if (ExtremeGhostRoleManager.GameRole.ContainsKey(
				PlayerControl.LocalPlayer.PlayerId) ||
			PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
		{
			return true;
		}
		else if (CommomSystem.IsForceInfoBlockRoleWithoutAssassin(role))
		{
			return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
		}
		return role.IsBlockShowMeetingRoleInfo();
	}
}
