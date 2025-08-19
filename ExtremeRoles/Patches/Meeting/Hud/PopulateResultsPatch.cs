using System;
using System.Collections.Generic;

using UnityEngine;

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateResults))]
public static class MeetingHudPopulateResultsPatch
{
	public static bool Prefix(
		MeetingHud __instance,
		[HarmonyArgument(0)] Il2CppStructArray<MeetingHud.VoterState> states)
	{

		if (!RoleAssignState.Instance.IsRoleSetUpEnd)
		{
			return true;
		}

		EventManager.Instance.Invoke(ModEvent.VisualUpdate);

		__instance.TitleText.text =
			TranslationController.Instance.GetString(
				StringNames.MeetingVotingResults, Array.Empty<Il2CppSystem.Object>());

		Dictionary<byte, int> voteIndex = new Dictionary<byte, int>();
		SortedList<int, (IRoleVoteModifier, NetworkedPlayerInfo)> voteModifier = new SortedList<
			int, (IRoleVoteModifier, NetworkedPlayerInfo)>();

		var allVoteHook = new List<(IRoleHookVoteEnd, NetworkedPlayerInfo)>();

		int num = 0;
		// それぞれの人に対してどんな投票があったか
		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			playerVoteArea.ClearForResults();

			byte checkPlayerId = playerVoteArea.TargetPlayerId;

			// 切断されたプレイヤーは残っている状態で役職を持たない状態になるのでキーチェックはしておく
			if (ExtremeRoleManager.GameRole.ContainsKey(checkPlayerId))
			{
				// 投票をいじる役職か？
				var (voteModRole, voteAnotherRole) =
					ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(checkPlayerId);
				addVoteModRole(voteModRole, checkPlayerId, ref voteModifier);
				addVoteModRole(voteAnotherRole, checkPlayerId, ref voteModifier);

				//　投票時のHook処理
				var (voteHook, voteHookAnotherRole) =
					ExtremeRoleManager.GetInterfaceCastedRole<IRoleHookVoteEnd>(checkPlayerId);
				addVoteHookRole(voteHook, checkPlayerId, allVoteHook);
				addVoteHookRole(voteHookAnotherRole, checkPlayerId, allVoteHook);
			}

			int noneSkipVote = 0;
			foreach (MeetingHud.VoterState voterState in states)
			{
				NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(voterState.VoterId);
				if (playerById == null)
				{
					Debug.LogError(
						string.Format("Couldn't find player info for voter: {0}",
						voterState.VoterId));
				}
				else if (i == 0 && voterState.SkippedVote)
				{
					// スキップのアニメーション
					__instance.BloopAVoteIcon(playerById, num, __instance.SkippedVoting.transform);
					num++;
				}
				else if (voterState.VotedForId == checkPlayerId)
				{
					// 投票された人のアニメーション
					__instance.BloopAVoteIcon(playerById, noneSkipVote, playerVoteArea.transform);
					noneSkipVote++;
				}
			}
			voteIndex.Add(playerVoteArea.TargetPlayerId, noneSkipVote);
		}

		voteIndex[__instance.playerStates[0].TargetPlayerId] = num;

		foreach (var (role, player) in voteModifier.Values)
		{
			role.ModifiedVoteAnime(
				__instance, player, ref voteIndex);
			role.ResetModifier();
		}

		foreach (var (role, player) in allVoteHook)
		{
			role.HookVoteEnd(__instance, player, voteIndex);
		}

		return false;
	}
	private static void addVoteModRole(
		IRoleVoteModifier? role, byte rolePlayerId,
		ref SortedList<int, (IRoleVoteModifier, NetworkedPlayerInfo)> voteModifier)
	{
		if (role is null)
		{
			return;
		}

		NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(rolePlayerId);

		int order = role.Order;
		// 同じ役職は同じ優先度になるので次の優先度になるようにセット
		while (voteModifier.ContainsKey(order))
		{
			++order;
		}
		voteModifier.Add(order, (role, playerById));
	}

	private static void addVoteHookRole(
		IRoleHookVoteEnd? role, byte rolePlayerId,
		in List<(IRoleHookVoteEnd, NetworkedPlayerInfo)> voteHook)
	{
		if (role is null)
		{
			return;
		}

		NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(rolePlayerId);
		voteHook.Add((role, playerById));
	}
}
