using HarmonyLib;

using TMPro;

using ExtremeRoles.Performance;

namespace ExtremeRoles.Patches.Meeting
{

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
				if (pc.protectedByGuardianThisRound)
				{
					pc.protectedByGuardianThisRound = false;
					if (pc.Data != null && !pc.Data.IsDead)
					{
						someoneWasProtected = true;
					}
				}
			}

			TMP_Text text = __instance.ProtectedRecently.GetComponentInChildren<TMP_SubMesh>().textComponent;

			string gaAbilityText = string.Empty;

			if (someoneWasProtected && !OptionHolder.Ship.IsBlockGAAbilityReport)
			{
				gaAbilityText = text.text;
			}

			string exrAbiltyText = ExtremeRolesPlugin.GameDataStore.GetGhostAbilityReport();

			if (gaAbilityText != string.Empty || exrAbiltyText != string.Empty)
            {
				string showText = gaAbilityText == string.Empty ? gaAbilityText : string.Empty;
				showText = showText == string.Empty ? exrAbiltyText : string.Concat(showText, "\n", exrAbiltyText);

				text.text = showText;
				SoundManager.Instance.PlaySound(__instance.ProtectedRecentlySound, false, 1f);
				__instance.ProtectedRecently.SetActive(true);
			}
		}
    }
}
