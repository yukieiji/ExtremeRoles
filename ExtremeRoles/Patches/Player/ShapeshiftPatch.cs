using System;

using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Player;

#nullable enable


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControlShapeshiftPatch
{
	public static bool Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl targetPlayer,
		[HarmonyArgument(1)] bool animate)
	{
		var roles = ExtremeRoleManager.GameRole;
		if (roles.Count == 0 || !roles.ContainsKey(__instance.PlayerId)) { return true; }

		var role = roles[__instance.PlayerId];
		if (role.TryGetVanillaRoleId(out RoleTypes roleId) &&
			roleId == RoleTypes.Shapeshifter) { return true; }


		GameData.PlayerInfo targetPlayerInfo = targetPlayer.Data;
		GameData.PlayerOutfit newOutfit;
		if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
		{
			newOutfit = __instance.Data.Outfits[PlayerOutfitType.Default];
		}
		else
		{
			newOutfit = targetPlayer.Data.Outfits[PlayerOutfitType.Default];
		}
		Action changeOutfit = delegate ()
		{
			if (targetPlayerInfo.PlayerId == __instance.Data.PlayerId)
			{
				__instance.RawSetOutfit(newOutfit, PlayerOutfitType.Default);
				__instance.logger.Info(
					string.Format("Player {0} Shapeshift is reverting",
						__instance.PlayerId), null);
				__instance.shapeshiftTargetPlayerId = -1;
			}
			else
			{
				__instance.RawSetOutfit(newOutfit, PlayerOutfitType.Shapeshifted);
				__instance.logger.Info(
					string.Format("Player {0} is shapeshifting into {1}",
						__instance.PlayerId, targetPlayer.PlayerId), null);
				__instance.shapeshiftTargetPlayerId = targetPlayer.PlayerId;
			}
		};
		if (animate)
		{
			__instance.shapeshifting = true;
			if (__instance.AmOwner)
			{
				PlayerControl.HideCursorTemporarily();
			}
			RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate(
				FastDestroyableSingleton<RoleManager>.Instance.shapeshiftAnim, __instance.gameObject.transform);
			roleEffectAnimation.SetMaterialColor(
				__instance.Data.Outfits[PlayerOutfitType.Default].ColorId);
			if (__instance.cosmetics.FlipX)
			{
				roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
			}

			Action changeAction = () =>
			{
				changeOutfit.Invoke();
				__instance.cosmetics.SetScale(
					__instance.MyPhysics.Animations.DefaultPlayerScale,
					__instance.defaultCosmeticsScale);
			};

			roleEffectAnimation.MidAnimCB = changeAction;

			__instance.StartCoroutine(__instance.ScalePlayer(
				__instance.MyPhysics.Animations.ShapeshiftScale, 0.25f));

			Action roleAnimation = () =>
			{
				__instance.shapeshifting = false;
			};

			roleEffectAnimation.Play(
				__instance, roleAnimation,
				CachedPlayerControl.LocalPlayer.PlayerControl.cosmetics.FlipX,
				RoleEffectAnimation.SoundType.Local, 0f);
			return false;
		}
		changeOutfit.Invoke();
		return false;

	}
}
