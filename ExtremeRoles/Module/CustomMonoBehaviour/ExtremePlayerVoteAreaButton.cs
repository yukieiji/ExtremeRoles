using System;
using System.Collections.Generic;
using UnityEngine;

using Il2CppInterop.Runtime.Attributes;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

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
		byte targetPlayerId = pva.TargetPlayerId;
		result = null;

		if (!this.meetingButton.TryGetValue(targetPlayerId, out var button))
		{
			button = new PlayerVoteAreaButtonContainer(pva);
			this.meetingButton[targetPlayerId] = button;
		}

		float startPos = pva.AnimateButtonsFromLeft ? 0.2f : 1.95f;

		var group = button.Group;
		if (OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
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
			Logging.Debug($"LocalPlayerId:{localPlayer.PlayerId} is Monika trash now");
			return false;
		}

		var role = ExtremeRoleManager.GetLocalPlayerRole();
		var multiRole = role as MultiAssignRoleBase;
		bool reseted = false;

		if (role is IRoleMeetingButtonAbility buttonRole &&
			multiRole?.AnotherRole is IRoleMeetingButtonAbility anotherButtonRole &&
			isOkRoleAbilityButton(pva, buttonRole) &&
			isOkRoleAbilityButton(pva, anotherButtonRole))
		{
			Logging.Debug($"LocalPlayer has dual meeting ability button");
			if (button.IsRecreateButtn(role.Id, buttonRole, out var element1) &&
				!reseted)
			{
				reseted = true;
				group.ResetSecond();
			}
			if (button.IsRecreateButtn(multiRole.AnotherRole.Id, anotherButtonRole, out var element2) &&
				!reseted)
			{
				group.ResetSecond();
			}
			if (reseted)
			{
				group.AddSecondRow(element1);
				group.AddSecondRow(element2);
			}
			result = group.Flatten(startPos);
		}
		else if (
			role is IRoleMeetingButtonAbility mainButtonRole &&
			isOkRoleAbilityButton(pva, mainButtonRole))
		{
			Logging.Debug($"LocalPlayer has one meeting ability button");
			if (button.IsRecreateButtn(role.Id, mainButtonRole, out var element1) &&
				!reseted)
			{
				reseted = true;
				group.ResetFirst();
			}
			group.ResetSecond();
			if (reseted)
			{
				group.AddFirstRow(element1);
			}
			result = group.Flatten(startPos);
		}
		else if (
			multiRole?.AnotherRole is IRoleMeetingButtonAbility subButtonRole &&
			isOkRoleAbilityButton(pva, subButtonRole))
		{
			Logging.Debug($"LocalPlayer has one meeting ability button");
			if (button.IsRecreateButtn(multiRole.AnotherRole.Id, subButtonRole, out var element1) &&
				!reseted)
			{
				reseted = true;
				group.ResetFirst();
			}
			group.ResetSecond();
			if (reseted)
			{
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
	private bool isOkRoleAbilityButton(
		PlayerVoteArea pva,
		IRoleMeetingButtonAbility buttonRole)
		=> !(
			pva.AmDead ||
			buttonRole.IsBlockMeetingButtonAbility(pva) ||
			pva.voteComplete ||
			pva.Parent == null ||
			!pva.Parent.Select((int)pva.TargetPlayerId)
		);
}
