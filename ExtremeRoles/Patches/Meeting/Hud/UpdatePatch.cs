using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Extension.Player;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;


namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
public static class MeetingHudUpdatePatch
{
	public static void Prefix(MeetingHud __instance)
	{
		// 探偵の能力をアサマリ会議中等に設定
		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var _) &&
			__instance.MeetingAbilityButton != null)
		{
			__instance.MeetingAbilityButton.gameObject.SetActive(false);
		}
	}

	public static void Postfix(MeetingHud __instance)
	{
		MeetingHudUpdateButtonsPatchHelper.Patch(__instance);
		infoOverlayBlockUpdate(__instance);
		changeNamePlate(__instance);

		if (__instance.state == MeetingHud.MeetingStates.Animating)
		{
			return;
		}

		fixMeetingDeadBug( __instance );
		disableSkip(__instance);
		meetingReportUpdate();
		updateButtons(__instance);
	}

	private static void infoOverlayBlockUpdate(MeetingHud __instance)
	{
		if (InfoOverlay.Instance.IsBlock &&
			__instance.state != MeetingHud.MeetingStates.Animating)
		{
			InfoOverlay.Instance.IsBlock = false;
		}
	}

	private static void changeNamePlate(MeetingHud hud)
	{
		if (!NamePlateHelper.NameplateChange)
		{
			return;
		}
		foreach (var pva in hud.playerStates)
		{
			NamePlateHelper.UpdateNameplate(pva);
		}
		NamePlateHelper.NameplateChange = false;
	}

	private static void disableSkip(MeetingHud hud)
	{
		// Deactivate skip Button if skipping on emergency meetings is disabled
		if (ExtremeGameModeManager.Instance.ShipOption.Meeting.IsBlockSkipInMeeting)
		{
			hud.SkipVoteButton.gameObject.SetActive(false);
		}
	}

	private static void fixMeetingDeadBug(MeetingHud hud)
	{
		// From TOR
		// This fixes a bug with the original game where pressing the button and a kill happens simultaneously
		// results in bodies sometimes being created *after* the meeting starts, marking them as dead and
		// removing the corpses so there's no random corpses leftover afterwards
		bool isDirty = false;

		foreach (var b in Object.FindObjectsOfType<DeadBody>())
		{
			if (b == null)
			{
				continue;
			}

			foreach (var pva in hud.playerStates)
			{
				if (pva == null || pva.AmDead)
				{
					continue;
				}

				if (pva.VotedForId == b.ParentId)
				{
					hud.ClearVote(
						pva.PlayerId,
						PlayerControl.LocalPlayer != null &&
						PlayerControl.LocalPlayer.PlayerId == pva.PlayerId);
					isDirty = true;
				}

				if (pva.PlayerId == b.ParentId)
				{
					pva.SetDead(true);
					pva.Overlay.gameObject.SetActive(true);
				}
			}
		}

		if (isDirty && AmongUsClient.Instance.AmHost)
		{
			hud.SetDirtyBit(1U);
		}
	}

	private static void meetingReportUpdate()
	{
		if (MeetingReporter.IsExist &&
			MeetingReporter.Instance.HasChatReport)
		{
			MeetingReporter.Instance.ReportMeetingChat();
		}
	}

	private static void updateButtons(MeetingHud hud)
	{
		if (!(
				OnemanMeetingSystemManager.TryGetActiveSystem(out var system) &&
				system.TryGetMeetingTitle(out string title)
			))
		{


			monikaTrashLayerSystemUpdate(hud);
			return;
		}

		var localPlayer = PlayerControl.LocalPlayer;
		hud.TitleText.text = title;
		hud.SkipVoteButton.gameObject.SetActive(
			system.IsSkipButtonActive &&
			system.Caller == localPlayer.PlayerId &&
			hud.state == MeetingHud.MeetingStates.NotVoted);

		if (!system.ActivateChatOverride)
		{
			return;
		}

		HudManager.Instance.Chat.gameObject.SetActive(
			(
				localPlayer != null
				&&
				(
					localPlayer.PlayerId == system.Caller ||
					system.CanChatPlayer(localPlayer)
				)
			));

	}

	private static void monikaTrashLayerSystemUpdate(MeetingHud hud)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!(
				localPlayer != null &&
				MonikaTrashSystem.TryGet(out var system) &&
				system.InvalidPlayer(localPlayer.PlayerId)
			))
		{
			tryCreateHandRaiseButton();
			return;
		}
		hud.SkipVoteButton.gameObject.SetActive(false);
	}

	private static void tryCreateHandRaiseButton()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;

		if (localPlayer.IsInValid() ||
			!ExtremeSystemTypeManager.Instance.TryGet<RaiseHandSystem>(
				ExtremeSystemType.RaiseHandSystem, out var raiseHand))
		{
			return;
		}

		if (raiseHand.IsInit)
		{
			raiseHand.SetActive(Minigame.Instance == null);
		}
		else
		{
			raiseHand.CreateRaiseHandButton();
		}
	}
}
