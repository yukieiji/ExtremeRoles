using HarmonyLib;
using UnityEngine;

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
			return true;
		}

		var meeting = FastDestroyableSingleton<HudManager>.Instance.MeetingPrefab;

		__instance.amDead = false;
		__instance.Glass.sprite = meeting.Glass.sprite;
		__instance.Glass.color = meeting.Glass.color;

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(
				playerVoteArea.TargetPlayerId);
			if (playerById == null)
			{
				playerVoteArea.SetDisabled();
				continue;
			}

			switch (system.GetVoteAreaState(playerById))
			{
				case VoteAreaState.XMark:
					activeObject(playerVoteArea.XMark.gameObject);
					break;
				case VoteAreaState.Overray:
					activeObject(playerVoteArea.Overlay.gameObject);
					break;
				case VoteAreaState.XMarkOverray:
					activeObject(playerVoteArea.XMark.gameObject);
					activeObject(playerVoteArea.Overlay.gameObject);
					break;
				default:
					break;
			}
		}
		return false;
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
