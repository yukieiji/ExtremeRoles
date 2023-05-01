using HarmonyLib;

using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches.Meeting;


[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class MeetingIntroAnimationInitPatch
{
    public static void Postfix(
        MeetingIntroAnimation __instance)
    {

		__instance.ProtectedRecently.SetActive(false);
		SoundManager.Instance.StopSound(__instance.ProtectedRecentlySound);

		bool someoneWasProtected = false;
		foreach(PlayerControl pc in CachedPlayerControl.AllPlayerControls)
		{
			if (pc == null) { continue; }

			if (pc.protectedByGuardianThisRound)
			{
				pc.protectedByGuardianThisRound = false;
				if (pc.Data != null && !pc.Data.IsDead)
				{
					someoneWasProtected = true;
				}
			}
		}

		TMP_SubMesh textSubMesh = __instance.ProtectedRecently.GetComponentInChildren<TMP_SubMesh>();

		if (textSubMesh == null) { return; }

		TMP_Text text = textSubMesh.textComponent;

		string gaProtectText = string.Empty;

		if (someoneWasProtected && !ExtremeGameModeManager.Instance.ShipOption.IsBlockGAAbilityReport)
		{
			gaProtectText = text.text;
		}

		string exrAbiltyText =
			MeetingReporter.IsExist ?
			MeetingReporter.Instance.GetMeetingStartReport() : string.Empty;

        bool isGaAbilityTextEmpty = string.IsNullOrEmpty(gaProtectText);
		bool isExrAbilityTextEmpty = string.IsNullOrEmpty(exrAbiltyText);

		if (isGaAbilityTextEmpty && isExrAbilityTextEmpty)
		{
			return;
		}

		text.text = isGaAbilityTextEmpty ?
			exrAbiltyText : string.Concat(gaProtectText, "\n", exrAbiltyText);
		SoundManager.Instance.PlaySound(__instance.ProtectedRecentlySound, false, 1f);
		__instance.ProtectedRecently.SetActive(true);
    }
}
