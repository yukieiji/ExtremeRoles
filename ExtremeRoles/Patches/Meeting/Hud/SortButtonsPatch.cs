using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.Extension.Vector;
using ExtremeRoles.GameMode;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;


#nullable enable

namespace ExtremeRoles.Patches.Meeting.Hud;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SortButtons))]
public static class MeetingHudSortButtonsPatch
{
	public static Vector3 HideOffset => new Vector3(1000.0f, 1000.0f, 1000.0f);

	public static bool Prefix(MeetingHud __instance)
	{
		if (!GameProgressSystem.IsGameNow)
		{
			return true;
		}
		var offset = HideOffset;
		var curPlayerState = __instance.playerStates;

		IOnemanMeeting? onemanMeeting = null;
		bool activeMeeting =
			OnemanMeetingSystemManager.TryGetActiveSystem(out var onemanMeetingMng) &&
			onemanMeetingMng.TryGetOnemanMeeting(out onemanMeeting);

		if (activeMeeting && onemanMeeting is IVoterValidtor validtor)
		{
			var validPlayer = new HashSet<byte>(validtor.ValidPlayer);
			var validPva = new List<PlayerVoteArea>(curPlayerState.Count);
			foreach (var pva in curPlayerState)
			{
				if (!validPlayer.Contains(pva.TargetPlayerId))
				{
					pva.transform.localPosition = offset;
				}
			}
		}

		bool isChangeVoteAreaButtonSort = ExtremeGameModeManager.Instance.ShipOption.Meeting.IsChangeVoteAreaButtonSortArg;
		bool monikaOn = MonikaTrashSystem.TryGet(out var monikaSystem);

		var orderLinq = curPlayerState.OrderBy(DefaultSort);
		if (monikaOn)
		{
			orderLinq = orderLinq.ThenBy(monikaSystem!.GetVoteAreaOrder);
		}
		if (isChangeVoteAreaButtonSort)
		{
			orderLinq = orderLinq.ThenBy(playerName2Int);
		}

		var array = orderLinq.ToArray();

		if (activeMeeting && onemanMeeting is IVoterShiftor shiftor)
		{
			shiftor.Shift(
				__instance.VoteOrigin,
				__instance.VoteButtonOffsets,
				array);
		}
		else
		{
			int index = 0;
			foreach (var pva in array)
			{
				var transform = pva.transform;
				if (transform.localPosition.IsCloseTo(offset))
				{
					continue;
				}
				int num = index % 3;
				int num2 = index / 3;
				transform.localPosition = __instance.VoteOrigin + new Vector3(
					__instance.VoteButtonOffsets.x * (float)num,
					__instance.VoteButtonOffsets.y * (float)num2, -0.9f - (float)num2 * 0.01f);
				index++;
			}
		}

		__instance.playerStates = array;

		return false;
	}

	public static void Postfix(MeetingHud __instance)
	{
		if (!GameProgressSystem.IsGameNow)
		{
			return;
		}

		var player = PlayerControl.LocalPlayer;
		bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			player);

		var system = ExtremeGameModeManager.Instance.ShipOption.Meeting.UseRaiseHand ? RaiseHandSystem.Get() : null;
		var trashMeeting = MonikaTrashSystem.TryGet(out var monikaSystem) ? monikaSystem : null;

		foreach (var pva in __instance.playerStates)
		{
			var obj = pva.gameObject;

			var status = new MeetingStatus(pva, isHudOverrideTaskActive);

			ISubscriber subscriber =
				pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId ?
				new LocalPlayerMeetingVisualUpdateEvent(status) :
				new OtherPlayerMeetingVisualUpdateEvent(
					GameData.Instance.GetPlayerById(pva.TargetPlayerId), status);

			EventManager.Instance.Register(subscriber, ModEvent.VisualUpdate);

			if (system is not null &&
				(trashMeeting is null || !trashMeeting.InvalidPlayer(pva)))
			{
				system.AddHand(pva);
			}
		}
		EventManager.Instance.Invoke(ModEvent.VisualUpdate);
	}

	public static int DefaultSort(PlayerVoteArea pva)
		=> pva.AmDead ? 50 : 0;

	private static int playerName2Int(PlayerVoteArea pva)
	{
		var player = GameData.Instance.GetPlayerById(pva.TargetPlayerId);
		if (player == null)
		{
			return 0;
		}

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
