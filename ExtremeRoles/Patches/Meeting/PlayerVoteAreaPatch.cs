using System.Linq;

using HarmonyLib;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;


namespace ExtremeRoles.Patches.Meeting;

public static class NamePlateHelper
{
	public static bool NameplateChange = true;

	public static void UpdateNameplate(
		PlayerVoteArea pva, byte playerId = byte.MaxValue)
	{
		var playerInfo = GameData.Instance.GetPlayerById(
			playerId != byte.MaxValue ?
			playerId : pva.TargetPlayerId);
		if (playerInfo == null)
		{
			return;
		}

		var cache = ShipStatus.Instance.CosmeticsCache;
		string id = playerInfo.DefaultOutfit.NamePlateId;
		if (ClientOption.Instance.HideNamePlate.Value ||
			!cache.nameplates.TryGetValue(id, out var np) ||
			np == null)
		{
			np = cache.nameplates["nameplate_NoPlate"];
		}
		if (np == null)
		{
			return;
		}
		pva.Background.sprite = np.GetAsset().Image;
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaCosmetics
{
	public static void Postfix(PlayerVoteArea __instance, NetworkedPlayerInfo playerInfo)
	{
		NamePlateHelper.UpdateNameplate(
			__instance, playerInfo.PlayerId);
	}
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
public static class PlayerVoteAreaSelectPatch
{

	public static bool Prefix(PlayerVoteArea __instance)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		if (!GameProgressSystem.IsGameNow)
		{
			return true;
		}

		float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;

		var button = __instance.gameObject.TryAddComponent<ExtremePlayerVoteAreaButton>();

		if (!button.TryGetMeetingButton(__instance, out var buttonEnumerable) ||
			__instance.voteComplete ||
			__instance.Parent == null ||
			!__instance.Parent.Select((int)__instance.TargetPlayerId))
		{
			return false;
		}

		if (buttonEnumerable is null)
		{
			return true;
		}

		__instance.Buttons.SetActive(true);
		__instance.StartCoroutine(
			Effects.All(
				buttonEnumerable.Select(
					x => x.Compute()).ToArray())
		);

		var selectableElements = new Il2CppSystem.Collections.Generic.List<UiElement>();
		foreach (var btn in buttonEnumerable)
		{
			selectableElements.Add(btn.Element);
		}
		ControllerManager.Instance.OpenOverlayMenu(
			__instance.name,
			__instance.CancelButton,
			__instance.ConfirmButton, selectableElements, false);

		return false;
	}
}

[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetCosmetics))]
public static class PlayerVoteAreaSetCosmeticsPatch
{
	public static void Postfix(PlayerVoteArea __instance)
	{
		if (ExtremeGameModeManager.Instance.ShipOption.Meeting.IsFixedVoteAreaPlayerLevel)
        {
			__instance.LevelNumberText.text = "99";
		}
	}
}


[HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetDead))]
public static class PlayerVoteAreaSetDeadPatch
{
	public static bool Prefix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
	{
		if (!OnemanMeetingSystemManager.IsActive)
		{
			return true;
		}

		__instance.AmDead = false;
		__instance.DidReport = didReport;
		__instance.Megaphone.enabled = didReport;
		__instance.Overlay.gameObject.SetActive(false);
		__instance.XMark.gameObject.SetActive(false);

		return false;
	}

	public static void Postfix(
		PlayerVoteArea __instance,
		[HarmonyArgument(0)] bool didReport,
		[HarmonyArgument(1)] bool isDead,
		[HarmonyArgument(2)] bool isGuardian = false)
    {
		var ga = __instance.GAIcon.gameObject;

		if (ExtremeGameModeManager.Instance.ShipOption.GhostRole.IsRemoveAngleIcon)
		{
			ga.SetActive(false);
		}
		else
		{
			bool isGhostRole = isGuardian ||
				ExtremeGhostRoleManager.GameRole.ContainsKey(__instance.TargetPlayerId);

			ga.SetActive(isGhostRole);
		}
	}
}
