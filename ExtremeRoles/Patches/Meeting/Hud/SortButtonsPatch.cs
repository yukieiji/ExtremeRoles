﻿using System;
using System.Text;
using System.Linq;

using HarmonyLib;
using UnityEngine;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles;
using ExtremeRoles.Module.SystemType;



using ExtremeRoles.Module.Interface;

namespace ExtremeRoles.Patches.Meeting.Hud;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.SortButtons))]
public static class MeetingHudSortButtonsPatch
{
	public static bool Prefix(MeetingHud __instance)
	{
		if (!ExtremeGameModeManager.Instance.ShipOption.Meeting.IsChangeVoteAreaButtonSortArg ||
			ExtremeRoleManager.GameRole.Count == 0) { return true; }

		PlayerVoteArea[] array = __instance.playerStates.OrderBy(delegate (PlayerVoteArea p)
		{
			if (!p.AmDead)
			{
				return 0;
			}
			return 50;
		}).ThenBy(playerName2Int).ToArray();

		for (int i = 0; i < array.Length; i++)
		{
			int num = i % 3;
			int num2 = i / 3;
			array[i].transform.localPosition = __instance.VoteOrigin + new Vector3(
				__instance.VoteButtonOffsets.x * (float)num,
				__instance.VoteButtonOffsets.y * (float)num2, -0.9f - (float)num2 * 0.01f);
		}

		return false;
	}

	public static void Postfix(MeetingHud __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return; }

		var player = PlayerControl.LocalPlayer;
		bool isHudOverrideTaskActive = PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(
			player);

		var system = ExtremeGameModeManager.Instance.ShipOption.Meeting.UseRaiseHand ? IRaiseHandSystem.Get() : null;

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			var obj = __instance.gameObject;

			VoteAreaInfo playerInfoUpdater =
				playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId ?
				obj.AddComponent<LocalPlayerVoteAreaInfo>() :
				obj.AddComponent<OtherPlayerVoteAreaInfo>();

			playerInfoUpdater.Init(playerVoteArea, isHudOverrideTaskActive);

			if (system != null)
			{
				system.AddHand(playerVoteArea);
			}
		}
	}

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
