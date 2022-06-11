using HarmonyLib;

using TMPro;

namespace ExtremeRoles.Patches.Meeting
{

    [HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
    public static class MeetingIntroAnimationInitPatch
    {
        public static void Postfix(
            MeetingIntroAnimation __instance)
        {
			if (OptionHolder.Ship.IsBlockGhostRoleAbilityReport)
            {
				__instance.ProtectedRecently.SetActive(false);
				return;
			}
			if (!ExtremeRolesPlugin.GameDataStore.AbilityManager.IsUseAbility()) { return; }

			bool someoneWasProtected = false;
			foreach(var pc in PlayerControl.AllPlayerControls)
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

			string abilityText = ExtremeRolesPlugin.GameDataStore.AbilityManager.CreateAbilityReport();

			if (someoneWasProtected)
			{
				string showText = string.Concat(text.text, "\n", abilityText);
				text.text = showText;
			}
			else
            {
				text.text = abilityText;
				SoundManager.Instance.PlaySound(__instance.ProtectedRecentlySound, false, 1f);
				__instance.ProtectedRecently.SetActive(true);
			}
		}
    }
}
