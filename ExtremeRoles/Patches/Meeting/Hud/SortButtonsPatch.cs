using System;
using System.Text;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType.Roles;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;

#nullable enable

namespace ExtremeRoles.Patches.Meeting.Hud;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SortButtons))]
public static class MeetingHudSortButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		IOrderedEnumerable<PlayerVoteArea>? orderLinq = null;
		var curPlayerState = __instance.playerStates;

		bool isChangeVoteAreaButtonSort = ExtremeGameModeManager.Instance.ShipOption.Meeting.IsChangeVoteAreaButtonSortArg;
		bool monikaOn = MonikaTrashSystem.TryGet(out var monikaSystem);

		IMeetingButtonInitialize? initializer = null;
		bool isSpecialMeeting =
			OnemanMeetingSystemManager.TryGetActiveSystem(out var onemanMeeting) &&
			onemanMeeting.TryGetOnemanMeeting<IMeetingButtonInitialize>(out initializer);

		if (isChangeVoteAreaButtonSort && monikaOn)
		{
			orderLinq = curPlayerState
				.OrderBy(DefaultSort)
				.ThenBy(monikaSystem!.GetVoteAreaOrder)
				.ThenBy(playerName2Int);
		}
		else if (monikaOn)
		{
			orderLinq = curPlayerState
				.OrderBy(DefaultSort)
				.ThenBy(monikaSystem!.GetVoteAreaOrder);
		}
		else if (isChangeVoteAreaButtonSort)
		{
			orderLinq = curPlayerState
				.OrderBy(DefaultSort)
				.ThenBy(playerName2Int);
		}
		else if (initializer != null)
		{
			orderLinq = curPlayerState.OrderBy(DefaultSort);
		}
		else
		{
			return true;
		}

		var array = orderLinq.ToArray();

		for (int i = 0; i < array.Length; i++)
		{
			int num = i % 3;
			int num2 = i / 3;
			array[i].transform.localPosition = __instance.VoteOrigin + new Vector3(
				__instance.VoteButtonOffsets.x * (float)num,
				__instance.VoteButtonOffsets.y * (float)num2, -0.9f - (float)num2 * 0.01f);
		}
		if (monikaOn)
		{
			monikaSystem!.InitializeButton(array);
		}

		initializer?.InitializeButon(
			__instance.VoteOrigin,
			__instance.VoteButtonOffsets,
			array);

		return false;
	}

	public static void Postfix(MeetingHud __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

		var player = PlayerControl.LocalPlayer;
		bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			player);

		var system = ExtremeGameModeManager.Instance.ShipOption.Meeting.UseRaiseHand ? IRaiseHandSystem.Get() : null;
		var trashMeeting = MonikaTrashSystem.TryGet(out var monikaSystem) ? monikaSystem : null;

		foreach (var pva in __instance.playerStates)
		{
			var obj = pva.gameObject;

			VoteAreaInfo playerInfoUpdater =
				pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId ?
				obj.AddComponent<LocalPlayerVoteAreaInfo>() :
				obj.AddComponent<OtherPlayerVoteAreaInfo>();

			playerInfoUpdater.Init(pva, isHudOverrideTaskActive);

			if (system is not null &&
				(trashMeeting is null || !trashMeeting.InvalidPlayer(pva)))
			{
				system.AddHand(pva);
			}
		}
	}

	public static int DefaultSort(PlayerVoteArea pva)
		=> pva.AmDead ? 50 : 0;

	private static int playerName2Int(PlayerVoteArea pva)
	{
		var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
		if (player == null) { return 0; }

		byte[] bytedPlayerName = Encoding.UTF8.GetBytes(player.DefaultOutfit.PlayerName.Trim());

		if (bytedPlayerName.Length >= 4)
		{
			return BitConverter.ToInt32(bytedPlayerName, 0);
		}
		else
		{
			int sum = 0;
			foreach (byte b in bytedPlayerName)
			{
				sum += b;
			}
			return sum;
		}
	}
}
