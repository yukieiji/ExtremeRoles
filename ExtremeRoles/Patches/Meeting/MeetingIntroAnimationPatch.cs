using HarmonyLib;

using TMPro;

using ExtremeRoles.Module;
using ExtremeRoles.Performance;
using ExtremeRoles.GameMode;
using System.Text;

namespace ExtremeRoles.Patches.Meeting;

#nullable enable

[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class MeetingIntroAnimationInitPatch
{
	public static void Prefix()
	{
		// バニラのベント掃除が残り続けるバグの修正
		// これより前だとベントに入ってる状態が残ってる可能性があるのでここでやる
		if (ShipStatus.Instance == null ||
			!ShipStatus.Instance.enabled ||
			!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out ISystemType? system))
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
		foreach(PlayerControl pc in PlayerCache.AllPlayerControl)
		{
			if (pc == null || !pc.protectedByGuardianThisRound) { continue; }

			pc.protectedByGuardianThisRound = false;
			if (pc.Data != null && !pc.Data.IsDead)
			{
				someoneWasProtected = true;
			}
		}

		TMP_SubMesh textSubMesh = __instance.ProtectedRecently.GetComponentInChildren<TMP_SubMesh>();

		if (textSubMesh == null) { return; }

		TMP_Text text = textSubMesh.textComponent;

		var builder = new StringBuilder();

		if (someoneWasProtected && !ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsBlockGAAbilityReport)
		{
            builder.Append(text.text);
		}

		if (MeetingReporter.IsExist)
		{
			if (builder.Length > 0)
			{
				builder.AppendLine();
			}

            builder.Append(
				MeetingReporter.Instance.GetMeetingStartReport());
		}

		if (builder.Length <= 0)
		{
			return;
		}

		text.text = builder.ToString();
		SoundManager.Instance.PlaySound(__instance.ProtectedRecentlySound, false, 1f);
		__instance.ProtectedRecently.SetActive(true);
    }
}
