using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using AmongUs.GameOptions;

using Il2CppInterop.Runtime.Attributes;
using UnityEngine;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.Helper;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Solo;

#nullable enable

namespace ExtremeRoles.Module.CustomMonoBehaviour;

[Il2CppRegister]
public sealed class ExtremePlayerVoteAreaButton(IntPtr ptr) : MonoBehaviour(ptr)
{
	private readonly Dictionary<byte, PlayerVoteAreaButtonContainer> meetingButton = new Dictionary<byte, PlayerVoteAreaButtonContainer>(PlayerCache.AllPlayerControl.Count);

	[HideFromIl2Cpp]
	public bool TryGetMeetingButton(
		PlayerVoteArea pva,
		out IEnumerable<IPlayerVoteAreaButtonPostionComputer>? result)
	{
		var localPlayer = PlayerControl.LocalPlayer;
		byte targetPlayerId = pva.PlayerId;
		result = null;
		if (localPlayer == null)
		{
			Logging.Debug($"LocalPlayer is null");
			return true;
		}

		if (!this.meetingButton.TryGetValue(targetPlayerId, out var button))
		{
			button = new PlayerVoteAreaButtonContainer(pva);
			this.meetingButton[targetPlayerId] = button;
		}

		float startPos = pva.AnimateButtonsFromLeft ? 0.2f : 1.95f;
		var overruleButton = pva.JudgeOverruleButton;

		var group = button.Group;
		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			if (overruleButton != null)
			{
				overruleButton.gameObject.SetActive(false);
				pva.JudgeOverruleButtonCommsDisable.SetActive(false);
			}

			result = group.DefaultFlatten(startPos);
			bool isVotor = system.Caller == localPlayer.PlayerId;
			Logging.Debug($"Is oneman meeting votor : {isVotor}");
			return isVotor;
		}

		var singleRole = ExtremeRoleManager.GetLocalPlayerRole();
		if (MonikaTrashSystem.TryGet(out var monika) &&
			monika.InvalidPlayer(localPlayer))
		{
			result = null;
			Logging.Debug($"LocalPlayerId : {localPlayer.PlayerId} is Monika trash now");
			return false;
		}

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		var multiRole = role as MultiAssignRoleBase;

		if (overruleButton != null &&
			IsMultiedJudgeRole(pva, role, out var judge, out var vanillaRole, out var exrMeetingButtonRole))
		{
			Logging.Debug($"LocalPlayer is dual meeting ability button with judge");

			// 2つ目のボタンをリセットしてから、JudgeOverruleButtonを追加する
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

				Logging.Debug("Judge can't use overrule button");

				// ジャッジのオーバールールボタンが使えないということは会議能力ボタンは1つ
				if (button.IsRecreateButtn(role.Core.Id, exrMeetingButtonRole, out var element1))
				{
					group.ResetFirst();
					group.AddFirstRow(element1);
				}
			}
			result = group.Flatten(startPos);
		}
		else if (
			role is IRoleMeetingButtonAbility buttonRole &&
			multiRole?.AnotherRole is IRoleMeetingButtonAbility anotherButtonRole &&
			isOkRoleAbilityButton(pva, buttonRole) &&
			isOkRoleAbilityButton(pva, anotherButtonRole))
		{
			Logging.Debug($"LocalPlayer has dual meeting ability button");

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
			isOkRoleAbilityButton(pva, mainButtonRole))
		{
			Logging.Debug($"LocalPlayer has one meeting ability button");

			group.ResetSecond();

            if (button.IsRecreateButtn(role.Core.Id, mainButtonRole, out var element1))
			{
				group.ResetFirst();
                group.AddFirstRow(element1);
            }
			result = group.Flatten(startPos);
		}
		else if (
			multiRole?.AnotherRole is IRoleMeetingButtonAbility subButtonRole &&
			isOkRoleAbilityButton(pva, subButtonRole))
		{
			Logging.Debug($"LocalPlayer has one meeting ability button");

			group.ResetSecond();

			if (button.IsRecreateButtn(multiRole.AnotherRole.Core.Id, subButtonRole, out var element1))
			{
				group.ResetFirst();
                group.AddFirstRow(element1);
            }
			result = group.Flatten(startPos);
		}
		else
		{
			result = null;
		}
		return true;
	}

	[HideFromIl2Cpp]
	private static bool isOkRoleAbilityButton(
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

	[HideFromIl2Cpp]
	private static bool IsMultiedJudgeRole(
		PlayerVoteArea pva,	SingleRoleBase role,
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
			isOkRoleAbilityButton(pva, mt))
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
			isOkRoleAbilityButton(pva, mt2) &&
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