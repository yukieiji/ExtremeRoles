using HarmonyLib;
using UnityEngine;


namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
public static class MeetingHudVotingCompletedPatch
{
	public static void Postfix()
	{
		InfoOverlay.Instance.Hide();

		foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
		{
			if (!body) { continue; }

			Object.Destroy(body.gameObject);
		}
	}
}
