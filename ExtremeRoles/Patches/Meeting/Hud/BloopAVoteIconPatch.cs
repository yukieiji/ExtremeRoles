using HarmonyLib;
using UnityEngine;

using AmongUs.GameOptions;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.Combination;
using ExtremeRoles.Performance;

using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class MeetingHudBloopAVoteIconPatch
{
	public static bool Prefix(
		MeetingHud __instance,
		[HarmonyArgument(0)] GameData.PlayerInfo voterPlayer,
		[HarmonyArgument(1)] int index, [HarmonyArgument(2)] Transform parent)
	{

		if (ExtremeRoleManager.GameRole.Count == 0) { return true; }

		SpriteRenderer spriteRenderer = Object.Instantiate(__instance.PlayerVotePrefab);

		var role = ExtremeRoleManager.GetLocalPlayerRole();

		bool canSeeVote = false;

		var mariln = role as Marlin;
		var assassin = role as Assassin;

		if (mariln != null)
		{
			canSeeVote = mariln.CanSeeVote;
		}
		if (assassin != null)
		{
			canSeeVote = assassin.CanSeeVote;
		}


		if (!GameManager.Instance.LogicOptions.GetAnonymousVotes() ||
			canSeeVote ||
			(
				CachedPlayerControl.LocalPlayer.Data.IsDead &&
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

		PlayerVoteArea component = parent.GetComponent<PlayerVoteArea>();
		if (component != null)
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
				CachedPlayerControl.LocalPlayer.PlayerId) ||
			CachedPlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
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
