using HarmonyLib;
using UnityEngine;


using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
public static class MeetingHudUpdateButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			monikaTrashUpdate(__instance);
			return true;
		}

		if (!system.IsActiveMeeting<MonikaLoveTargetMeeting>())
		{
			monikaTrashUpdate(__instance);
		}

		var meeting = HudManager.Instance.MeetingPrefab;

		__instance.amDead = false;
		__instance.Glass.sprite = meeting.Glass.sprite;
		__instance.Glass.color = meeting.Glass.color;

		foreach (var pva in __instance.playerStates)
		{
			NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(
				pva.TargetPlayerId);
			if (playerById == null)
			{
				pva.SetDisabled();
				continue;
			}

			switch (system.GetVoteAreaState(playerById))
			{
				case VoteAreaState.XMark:
					activeObject(pva.XMark.gameObject);
					break;
				case VoteAreaState.Overray:
					activeObject(pva.Overlay.gameObject);
					break;
				case VoteAreaState.XMarkOverray:
					activeObject(pva.XMark.gameObject);
					activeObject(pva.Overlay.gameObject);
					break;
				default:
					break;
			}
		}
		return false;
	}

	private static void monikaTrashUpdate(MeetingHud hud)
	{
		if (!MonikaTrashSystem.TryGet(out var system))
		{
			return;
		}

		var localPc = PlayerControl.LocalPlayer;
		if (localPc != null &&
			localPc.Data != null &&
			!localPc.Data.IsDead &&
			system.InvalidPlayer(localPc))
		{
			hud.Glass.sprite = system.MeetingBackground;
		}

		foreach (var pva in hud.playerStates)
		{
			if (pva == null || 
				pva.AmDead ||
				!system.InvalidPlayer(pva))
			{
				continue;
			}
			activeObject(pva.XMark.gameObject);
		}
	}

	private static void activeObject(GameObject obj)
	{
		if (obj.activeSelf)
		{
			return;
		}
		obj.SetActive(true);
	}
}
