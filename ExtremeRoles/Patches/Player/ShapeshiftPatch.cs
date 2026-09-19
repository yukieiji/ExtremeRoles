using System;

using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;

using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Core.Abstract;
using Microsoft.Extensions.DependencyInjection;

namespace ExtremeRoles.Patches.Player;

#nullable enable

public class PlayerControlShapeshiftPatchBody(IGameProgress progress, IGameRuntime runtime)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;

	public bool Prefix(
		PlayerControl __instance,
		PlayerControl targetPlayer,
		bool animate)
	{
		if ((
				!_progress.IsGameNow ||
				!_runtime.TryGetGameContext(out var ctx) ||
				!ctx.Roles.TryGetRole(__instance.PlayerId, out var role) ||
				(role.TryGetVanillaRoleId(out RoleTypes roleId) && roleId is RoleTypes.Shapeshifter)
			))
		{
			return true;
		}
		Override(__instance, targetPlayer, animate);
		return false;
	}

	private void Override(
		PlayerControl __instance,
		PlayerControl targetPlayer,
		bool animate)
	{
		if (__instance.CurrentOutfitType == PlayerOutfitType.MushroomMixup)
		{
			return;
		}

		NetworkedPlayerInfo targetPlayerInfo = targetPlayer.Data;

		bool isSame = targetPlayerInfo.PlayerId == __instance.Data.PlayerId;

		var outfits = __instance.Data.Outfits;

		if (!outfits.TryGetValue(PlayerOutfitType.Default, out var instancePlayerOutfit))
		{
			return;
		}
		var newOutfit = instancePlayerOutfit;

		if (!isSame)
		{
			if (!targetPlayerInfo.Outfits.TryGetValue(PlayerOutfitType.Default, out newOutfit))
			{
				return;
			}
		}

		Action changeOutfit = () => 
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

		if (!animate)
		{
			changeOutfit.Invoke();
			return;
		}

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
			RoleManager.Instance.shapeshiftAnim,
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
			PlayerControl.LocalPlayer.cosmetics.FlipX,
			RoleEffectAnimation.SoundType.Local, 0f);
	}
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public static class PlayerControlShapeshiftPatch
{
	private static PlayerControlShapeshiftPatchBody? _body;

	public static bool Prefix(
		PlayerControl __instance,
		[HarmonyArgument(0)] PlayerControl targetPlayer,
		[HarmonyArgument(1)] bool animate)
	{
		if (_body is null)
		{
			_body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<PlayerControlShapeshiftPatchBody>();
		}
		return _body.Prefix(__instance, targetPlayer, animate);
	}
}
