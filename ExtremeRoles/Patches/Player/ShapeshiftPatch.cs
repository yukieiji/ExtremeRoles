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
		if (roles.Count == 0 ||
			!roles.ContainsKey(__instance.PlayerId))
		{
			return true;
		}

		var role = roles[__instance.PlayerId];
		if (role.TryGetVanillaRoleId(out RoleTypes roleId) &&
			roleId == RoleTypes.Shapeshifter) { return true; }

		if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup) { return false; }


		NetworkedPlayerInfo targetPlayerInfo = targetPlayer.Data;

		bool isSame = targetPlayerInfo.PlayerId == __instance.Data.PlayerId;

		var outfits = __instance.Data.Outfits;

		GameData.PlayerOutfit instancePlayerOutfit = outfits[PlayerOutfitType.Default];
		GameData.PlayerOutfit newOutfit = instancePlayerOutfit;

		if (!isSame)
		{
			newOutfit = targetPlayerInfo.Outfits[PlayerOutfitType.Default];
		}

		Action changeOutfit = delegate ()
		{
			if (isSame)
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

			var myPhysics = __instance.MyPhysics;
			var anim = myPhysics.Animations;

			myPhysics.SetNormalizedVelocity(Vector2.zero);
			bool amOwner = __instance.AmOwner;
			if (amOwner && Minigame.Instance == null)
			{
				PlayerControl.HideCursorTemporarily();
			}
			RoleEffectAnimation roleEffectAnimation = UnityEngine.Object.Instantiate(
				FastDestroyableSingleton<RoleManager>.Instance.shapeshiftAnim,
				__instance.gameObject.transform);
			roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(amOwner);
			roleEffectAnimation.SetMaterialColor(instancePlayerOutfit.ColorId);
			if (__instance.cosmetics.FlipX)
			{
				roleEffectAnimation.transform.position -= new Vector3(0.14f, 0f, 0f);
			}

			Action changeAction = () =>
			{
				changeOutfit.Invoke();
				__instance.cosmetics.SetScale(
					anim.DefaultPlayerScale,
					__instance.defaultCosmeticsScale);
			};

			roleEffectAnimation.MidAnimCB = changeAction;

			bool shoudLongAround = AprilFoolsMode.ShouldLongAround();

			if (shoudLongAround)
			{
				__instance.cosmetics.ShowLongModeParts(false);
				__instance.cosmetics.SetHatVisorVisible(false);
			}

			__instance.StartCoroutine(
				__instance.ScalePlayer(anim.ShapeshiftScale, 0.25f));

			Action roleAnimation = () =>
			{
				__instance.shapeshifting = false;
				if (shoudLongAround)
				{
					__instance.cosmetics.ShowLongModeParts(true);
					__instance.cosmetics.SetHatVisorVisible(true);
				}
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
