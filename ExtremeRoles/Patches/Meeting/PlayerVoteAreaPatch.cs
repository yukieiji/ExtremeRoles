using System;
using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

using ExtremeRoles.Extension.UnityEvents;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

using ExtremeRoles.Module.CustomOption;

using Il2CppActionFloat = Il2CppSystem.Action<float>;
using Il2CppIEnumerator = Il2CppSystem.Collections.IEnumerator;

namespace ExtremeRoles.Patches.Meeting;

public static class NamePlateHelper
{
	public static bool NameplateChange = true;

	public static void UpdateNameplate(
		PlayerVoteArea pva, byte playerId = byte.MaxValue)
	{
		var playerInfo = GameData.Instance.GetPlayerById(
			playerId != byte.MaxValue ?
			playerId : pva.TargetPlayerId);
		if (playerInfo == null) { return; }

		var cache = CachedShipStatus.Instance.CosmeticsCache;
		string id = playerInfo.DefaultOutfit.NamePlateId;
		if (ClientOption.Instance.HideNamePlate.Value ||
			!cache.nameplates.TryGetValue(id, out var np) ||
			np == null)
		{
			np = cache.nameplates["nameplate_NoPlate"];
		}
		if (np == null) { return; }
		pva.Background.sprite = np.GetAsset().Image;
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaCosmetics
{
	public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
	{
		NamePlateHelper.UpdateNameplate(
			__instance, playerInfo.PlayerId);
	}
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class PlayerVoteAreaSelectPatch
{
	private static Dictionary<byte, UiElement> meetingAbilityButton =
		new Dictionary<byte, UiElement>();

	public static void Reset()
    {
		meetingAbilityButton.Clear();
    }

	public static bool Prefix(PlayerVoteArea __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd ||
			ExtremeRoleManager.GameRole.Count == 0) { return true; }

		var state = ExtremeRolesPlugin.ShipState;
		var (buttonRole, anotherButtonRole) = ExtremeRoleManager.GetInterfaceCastedLocalRole<
			IRoleMeetingButtonAbility>();

		if (!state.AssassinMeetingTrigger)
		{
			if (buttonRole != null && anotherButtonRole != null)
            {
				return true; // TODO:Can use both role ability
            }
			else if (buttonRole != null && anotherButtonRole == null)
            {
				return meetingButtonAbility(__instance, buttonRole);
			}
			else if (buttonRole == null && anotherButtonRole != null)
			{
				return meetingButtonAbility(__instance, anotherButtonRole);
			}
			else
			{
				return true;
			}
		}

		if (CachedPlayerControl.LocalPlayer.PlayerId != ExtremeRolesPlugin.ShipState.ExiledAssassinId)
		{
			return false;
		}

		if (!__instance.Parent)
		{
			return false;
		}
		if (!__instance.voteComplete &&
			__instance.Parent.Select((int)__instance.TargetPlayerId))
		{
			__instance.Buttons.SetActive(true);
			float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
			__instance.StartCoroutine(
				Effects.All(
					wrappedEffectsLerp(0.25f, (float t) =>
					{
						__instance.CancelButton.transform.localPosition = Vector2.Lerp(
							Vector2.right * startPos,
							Vector2.right * 1.3f,
							Effects.ExpOut(t));
					}),
					wrappedEffectsLerp(0.35f, (float t) =>
					{
						__instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
							Vector2.right * startPos,
							Vector2.right * 0.65f,
							Effects.ExpOut(t));
					})
				)
			);

			Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new Il2CppSystem.Collections.Generic.List<
				UiElement>();
			selectableElements.Add(__instance.CancelButton);
			selectableElements.Add(__instance.ConfirmButton);
			ControllerManager.Instance.OpenOverlayMenu(
				__instance.name,
				__instance.CancelButton,
				__instance.ConfirmButton, selectableElements, false);
		}

		return false;
	}

	private static bool meetingButtonAbility(
		PlayerVoteArea instance,
		IRoleMeetingButtonAbility role)
	{
		byte target = instance.TargetPlayerId;

        if (instance.AmDead)
		{
			return true;
		}
		if (!instance.Parent)
		{
			return false;
		}
		if (role.IsBlockMeetingButtonAbility(instance))
        {
			return true;
        }

		if (!instance.voteComplete &&
			instance.Parent.Select((int)target))
		{

			if (!meetingAbilityButton.TryGetValue(target, out UiElement abilitybutton) ||
				abilitybutton == null)
			{
				UiElement newAbilitybutton = GameObject.Instantiate(
					instance.CancelButton, instance.ConfirmButton.transform.parent);
				var passiveButton = newAbilitybutton.GetComponent<PassiveButton>();
				passiveButton.OnClick.RemoveAllPersistentAndListeners();
				passiveButton.OnClick.AddListener(instance.Cancel);
                passiveButton.OnClick.AddListener(
                    () => { newAbilitybutton.gameObject.SetActive(false); });
				passiveButton.OnClick.AddListener(role.CreateAbilityAction(instance));

                var render = newAbilitybutton.GetComponent<SpriteRenderer>();

				role.ButtonMod(instance, newAbilitybutton);
				role.SetSprite(render);

				meetingAbilityButton[target] = newAbilitybutton;
				abilitybutton = newAbilitybutton;
			}

			if (abilitybutton == null) { return true; }

			abilitybutton.gameObject.SetActive(true);
			instance.Buttons.SetActive(true);

			float startPos = instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;

			instance.StartCoroutine(
				Effects.All(
					wrappedEffectsLerp(0.25f, (float t) =>
					{
						instance.CancelButton.transform.localPosition = Vector2.Lerp(
							Vector2.right * startPos,
							Vector2.right * 1.3f,
							Effects.ExpOut(t));
					}),
					wrappedEffectsLerp(0.35f, (float t) =>
					{
						instance.ConfirmButton.transform.localPosition = Vector2.Lerp(
							Vector2.right * startPos,
							Vector2.right * 0.65f,
							Effects.ExpOut(t));
					}),
					wrappedEffectsLerp(0.45f, (float t) =>
					{
						abilitybutton.transform.localPosition = Vector2.Lerp(
							Vector2.right * startPos,
							Vector2.right * -0.01f,
							Effects.ExpOut(t));
					})
				)
			);

			Il2CppSystem.Collections.Generic.List<UiElement> selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
			selectableElements.Add(instance.CancelButton);
			selectableElements.Add(instance.ConfirmButton);
			selectableElements.Add(abilitybutton);

			ControllerManager.Instance.OpenOverlayMenu(
				instance.name,
				instance.CancelButton,
				instance.ConfirmButton, selectableElements, false);
		}

		return false;

	}

	private static Il2CppIEnumerator wrappedEffectsLerp(float t, Delegate del)
		=> Effects.Lerp(t, (Il2CppActionFloat)(del));
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaSetCosmeticsPatch
{
	public static void Postfix(PlayerVoteArea __instance)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.Meeting.IsFixedVoteAreaPlayerLevel)
        {
			__instance.LevelNumberText.text = "99";
		}
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetDead))]
public static class PlayerVoteAreaSetDeadPatch
{
	public static bool Prefix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
	{
		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger) { return true; }

		__instance.AmDead = false;
		__instance.DidReport = didReport;
		__instance.Megaphone.enabled = didReport;
		__instance.Overlay.gameObject.SetActive(false);
		__instance.XMark.gameObject.SetActive(false);

		return false;
	}

	public static void Postfix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
        {
		if (ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsRemoveAngleIcon)
		{
			__instance.GAIcon.gameObject.SetActive(false);
		}
		else
		{
			bool isGhostRole = isGuardian ||
				ExtremeGhostRoleManager.GameRole.ContainsKey(__instance.TargetPlayerId);

			__instance.GAIcon.gameObject.SetActive(isGhostRole);
		}
	}
}
