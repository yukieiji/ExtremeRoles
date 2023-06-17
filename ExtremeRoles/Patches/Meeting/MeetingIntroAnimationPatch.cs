using HarmonyLib;

using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;

namespace ExtremeRoles.Patches.Meeting;


[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class MeetingIntroAnimationInitPatch
{
	// バニラのベント掃除が残り続けるバグの修正
	// これより前だとベントに入ってる状態が残ってる可能性があるのでここでやる
	public static void Prefix()
	{
		if (CachedShipStatus.Instance == null ||
			!CachedShipStatus.Instance.enabled ||
			!CachedShipStatus.Systems.TryGetValue(SystemTypes.Ventilation, out ISystemType system))
		{
			return;
		}
		var ventSystem = system.TryCast<VentilationSystem>();
		if (ventSystem == null) { return; }

		ventSystem.PlayersCleaningVents.Clear();
		ventSystem.PlayersInsideVents.Clear();
	}

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
