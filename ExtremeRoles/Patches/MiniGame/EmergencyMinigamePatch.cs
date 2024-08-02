using HarmonyLib;

using UnityEngine;
using ExtremeRoles.Roles.API.Extension.State;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches.MiniGame
{
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    public static class EmergencyMinigameUpdatePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (Roles.ExtremeRoleManager.GameRole.Count == 0) { return; }

            if (!Roles.ExtremeRoleManager.GetLocalPlayerRole().CanCallMeeting())
            {
                __instance.StatusText.text = Tr.GetString("youDonotUse");
                __instance.NumberText.text = string.Empty;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
                __instance.ButtonActive = false;
                return;
            }

            // Handle max number of meetings
            if (__instance.state == 1)
            {
                int localRemaining =
                    PlayerControl.LocalPlayer.RemainingEmergencies;
                int teamRemaining = Mathf.Max(
                    0, ExtremeGameModeManager.Instance.ShipOption.Meeting.MaxMeetingCount -
                        ExtremeRolesPlugin.ShipState.MeetingCount);
                int remaining = Mathf.Min(localRemaining, teamRemaining);

                __instance.StatusText.text = $"<size=100%>{Tr.GetString("meetingStatus", PlayerControl.LocalPlayer.name)}</size>";
                __instance.NumberText.text = Tr.GetString(
					"meetingCount",
					localRemaining,
					teamRemaining);
                __instance.ButtonActive = remaining > 0;
                __instance.ClosedLid.gameObject.SetActive(!__instance.ButtonActive);
                __instance.OpenLid.gameObject.SetActive(__instance.ButtonActive);
                return;
            }
        }
    }
}
