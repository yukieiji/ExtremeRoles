using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Performance;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
public static class MeetingHudUpdateButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			monikaSystemHudUpdate(__instance);
			return true;
		}

		var meeting = FastDestroyableSingleton<HudManager>.Instance.MeetingPrefab;

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

	private static void monikaSystemHudUpdate(MeetingHud hud)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (MonikaTrashSystem.TryGet(out var monika) &&
			localPlayer != null &&
			monika.Meeting.InvalidPlayer(localPlayer.PlayerId))
		{

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
