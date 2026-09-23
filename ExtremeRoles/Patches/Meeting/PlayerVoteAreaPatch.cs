using AmongUs.GameOptions;
using ExtremeRoles.Core.Abstract;
using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.GameMode;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

#nullable enable


namespace ExtremeRoles.Patches.Meeting;

public class PlayerVoteAreaSelectPatchBody(IGameProgress progress, IGameRuntime runtime, IModLogger logger)
{
	private readonly IGameProgress _progress = progress;
	private readonly IGameRuntime _runtime = runtime;
	private readonly IModLogger _logger = logger;

	private readonly Dictionary<byte, PlayerVoteAreaButtonContainer> meetingButton = new Dictionary<byte, PlayerVoteAreaButtonContainer>(PlayerCache.AllPlayerControl.Count);

	public bool Prefix(PlayerVoteArea __instance)
	{
		if (!_progress.IsGameNow || !_runtime.TryGetGameContext(out var ctx))
		{
			return true;
		}

		if (!TryGetMeetingButton(__instance, ctx, out var buttonEnumerable) ||
			__instance.VoteComplete ||
			__instance.Parent == null ||
			!__instance.Parent.Select((int)__instance.PlayerId))
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

	public bool TryGetMeetingButton(
		PlayerVoteArea pva,
		IGameContext gameContext,
		out IEnumerable<IPlayerVoteAreaButtonPostionComputer>? result)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte targetPlayerId = pva.PlayerId;
		result = null;
		if (localPlayer == null)
		{
			_logger.LogTrace($"LocalPlayer is null");
			return true;
		}

		if (!this.meetingButton.TryGetValue(targetPlayerId, out var button) ||
			button.IsRecreate)
		{
			button = new PlayerVoteAreaButtonContainer(pva);
			this.meetingButton[targetPlayerId] = button;
		}

		float startPos = pva.AnimateButtonsFromLeft ? 0.2f : 1.95f;
		var overruleButton = pva.JudgeOverruleButton;

		if (overruleButton != null)
		{
			overruleButton.gameObject.SetActive(false);
			pva.JudgeOverruleButtonCommsDisable.SetActive(false);
		}
		button.HideAllButton();

		var group = button.Group;
		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			result = group.DefaultFlatten(startPos);
			bool isVotor = system.Caller == localPlayer.PlayerId;
			_logger.LogTrace($"Is oneman meeting votor : {isVotor}");
			return isVotor;
		}

		var role = gameContext.Roles.GetLocalPlayerRole();
		if (MonikaTrashSystem.TryGet(out var monika) &&
			monika.InvalidPlayer(localPlayer))
		{
			result = null;
			_logger.LogTrace($"LocalPlayerId : {localPlayer.PlayerId} is Monika trash now");
			return false;
		}

		var multiRole = role as MultiAssignRoleBase;
		if (overruleButton != null &&
			IsMultiedJudgeRole(pva, role, out var judge, out var vanillaRole, out var exrMeetingButtonRole))
		{
			_logger.LogTrace($"LocalPlayer is dual meeting ability button with judge");
			result = CreateJudgeButton(localPlayer, startPos, pva, button, role, exrMeetingButtonRole, judge, overruleButton);
		}
		else if (
			role is IRoleMeetingButtonAbility buttonRole &&
			multiRole?.AnotherRole is IRoleMeetingButtonAbility anotherButtonRole &&
			IsOkRoleAbilityButton(pva, buttonRole) &&
			IsOkRoleAbilityButton(pva, anotherButtonRole))
		{
			_logger.LogTrace($"LocalPlayer has dual meeting ability button");

			bool isRecreateMain = button.IsRecreateButtn(role.Core.Id, buttonRole, out var mainButton);
			bool isRecreateSub = button.IsRecreateButtn(multiRole.AnotherRole.Core.Id, anotherButtonRole, out var subButton);

			if (isRecreateMain || isRecreateSub)
			{
				group.ResetSecond();
				group.AddSecondRow(mainButton);
				group.AddSecondRow(subButton);
			}
			result = group.Flatten(startPos);
		}
		else if (
			role is IRoleMeetingButtonAbility mainButtonRole &&
			IsOkRoleAbilityButton(pva, mainButtonRole))
		{
			_logger.LogTrace($"LocalPlayer has one meeting ability button");
			result = CreateFirstRowButton(startPos, button, role.Core.Id, mainButtonRole);
		}
		else if (
			multiRole?.AnotherRole is IRoleMeetingButtonAbility subButtonRole &&
			IsOkRoleAbilityButton(pva, subButtonRole))
		{
			_logger.LogTrace($"LocalPlayer has one meeting ability another button");
			result = CreateFirstRowButton(startPos, button, multiRole.AnotherRole.Core.Id, subButtonRole);
		}
		else
		{
			result = null;
		}
		return true;
	}

	private IEnumerable<IPlayerVoteAreaButtonPostionComputer> CreateFirstRowButton(
		float startPos,
		PlayerVoteAreaButtonContainer button,
		ExtremeRoleId id,
		IRoleMeetingButtonAbility buttonAbility)
	{
		var group = button.Group;
		group.ResetSecond();

		if (button.IsRecreateButtn(id, buttonAbility, out var element1))
		{
			group.ResetFirst();
			group.AddFirstRow(element1);
		}
		return group.Flatten(startPos);
	}

	private IEnumerable<IPlayerVoteAreaButtonPostionComputer> CreateJudgeButton(
		PlayerControl localPlayer,
		float startPos,
		PlayerVoteArea pva,
		PlayerVoteAreaButtonContainer button,
		SingleRoleBase role,
		IRoleMeetingButtonAbility exrMeetingButtonRole,
		JudgeRole judge,
		UiElement overruleButton)
	{
		// 2つ目のボタンをリセットしてから、JudgeOverruleButtonを追加する
		var group = button.Group;
		group.ResetSecond();

		// JudgeOverruleButtonの表示条件を判定する
		if (!judge.IsBlockedByTasks() && !judge.HasAlreadyOverruledThisMeeting && judge.HasAnOverruleUse)
		{
			overruleButton.gameObject.SetActive(true);
			// ジャッジの判定中にDataとRoleがnullになることはないはずなので、nullチェックは不要
			if (localPlayer.Data.Role.IsAffectedByComms)
			{
				pva.JudgeOverruleButtonCommsDisable.SetActive(true);
				overruleButton.enabled = false;
				overruleButton.GetComponent<SpriteRenderer>().color = Palette.DisabledClear;
			}
			else
			{
				pva.JudgeOverruleButtonCommsDisable.SetActive(false);
				overruleButton.enabled = true;
				overruleButton.GetComponent<SpriteRenderer>().color = Palette.White;
			}
			if (button.IsRecreateButtn(role.Core.Id, exrMeetingButtonRole, out var element1))
			{
				group.AddSecondRow(element1);
				group.AddSecondRow(overruleButton);
			}
		}
		else
		{
			overruleButton.gameObject.SetActive(false);

			_logger.LogTrace("Judge can't use overrule button");

			// ジャッジのオーバールールボタンが使えないということは会議能力ボタンは1つ
			if (button.IsRecreateButtn(role.Core.Id, exrMeetingButtonRole, out var element1))
			{
				group.ResetFirst();
				group.AddFirstRow(element1);
			}
		}
		return group.Flatten(startPos);
	}

	private static bool IsOkRoleAbilityButton(
		PlayerVoteArea pva,
		IRoleMeetingButtonAbility buttonRole)
		=> !(
			pva.PlayerId == PlayerVoteArea.SkippedVote ||
			pva.AmDead ||
			buttonRole.IsBlockMeetingButtonAbility(pva) ||
			pva.VoteComplete ||
			pva.Parent == null ||
			!pva.Parent.Select((int)pva.PlayerId)
		);

	private static bool IsMultiedJudgeRole(
		PlayerVoteArea pva, SingleRoleBase role,
		[NotNullWhen(true)] out JudgeRole? judge,
		[NotNullWhen(true)] out VanillaRoleWrapper? vanillaRole,
		[NotNullWhen(true)] out IRoleMeetingButtonAbility? exrMeetingButtonRole)
	{
		vanillaRole = null;
		exrMeetingButtonRole = null;
		judge = null;

		var localPlayer = PlayerControl.LocalPlayer;

		// ジャッジ + 会議能力ボタン持ち役職組み合わせの判定
		if (localPlayer.Data != null &&
			localPlayer.Data.Role != null &&
			localPlayer.Data.Role.IsTryCast<JudgeRole>(out var judgeRole1) &&
			role is VanillaRoleWrapper vr &&
			vr.VanilaRoleId is RoleTypes.Judge &&
			vr.AnotherRole is IRoleMeetingButtonAbility mt &&
			IsOkRoleAbilityButton(pva, mt))
		{
			vanillaRole = vr;
			exrMeetingButtonRole = mt;
			judge = judgeRole1;
			return true;
		}
		// 会議能力ボタン持ち役職 + ジャッジの組み合わせの判定
		else if (
			localPlayer.Data != null &&
			localPlayer.Data.Role != null &&
			localPlayer.Data.Role.IsTryCast<JudgeRole>(out var judgeRole2) &&
			role is IRoleMeetingButtonAbility mt2 &&
			IsOkRoleAbilityButton(pva, mt2) &&
			role is MultiAssignRoleBase multiRole &&
			multiRole.AnotherRole is VanillaRoleWrapper vr2 &&
			vr2.VanilaRoleId is RoleTypes.Judge)
		{
			vanillaRole = vr2;
			exrMeetingButtonRole = mt2;
			judge = judgeRole2;
			return true;
		}

		return false;
	}
}

public static class NamePlateHelper
{
	public static bool NameplateChange = true;

	public static void UpdateNameplate(
		PlayerVoteArea pva, byte playerId = byte.MaxValue)
	{
		var playerInfo = GameData.Instance.GetPlayerById(
			playerId != byte.MaxValue ?
			playerId : pva.PlayerId);
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
	private static PlayerVoteAreaSelectPatchBody? body;

	public static bool Prefix(PlayerVoteArea __instance)
	{
		if (body == null)
		{
			body = ExtremeRolesPlugin.Instance.Provider.GetRequiredService<PlayerVoteAreaSelectPatchBody>();
		}
		return body.Prefix(__instance);
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
		[HarmonyArgument(0)] bool isDead)
	{
		if (!OnemanMeetingSystemManager.IsActive)
		{
			return true;
		}

		__instance.AmDead = false;
		__instance.Overlay.gameObject.SetActive(false);
		__instance.XMark.gameObject.SetActive(false);

		return false;
	}
}
