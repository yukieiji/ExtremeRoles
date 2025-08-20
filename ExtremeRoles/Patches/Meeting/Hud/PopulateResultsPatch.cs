using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Performance.Il2Cpp;

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

		// --- Phase 1: Data Gathering and Caching ---

		var allVotes = new VoteCollection();
		var voteModifierRoles = new SortedList<int, (IRoleVoteModifier, NetworkedPlayerInfo)>();
		var voteHookRoles = new List<(IRoleHookVoteEnd, NetworkedPlayerInfo)>();

		var playerAreaMap = new Dictionary<byte, PlayerVoteArea>();
		var playerInfoMap = GameData.Instance.AllPlayers.GetFastEnumerator().ToDictionary(p => p.PlayerId);

		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			playerVoteArea.ClearForResults();

			byte checkPlayerId = playerVoteArea.TargetPlayerId;
			if (checkPlayerId >= 0)
			{
				playerAreaMap[checkPlayerId] = playerVoteArea;
			}

			if (ExtremeRoleManager.GameRole.ContainsKey(checkPlayerId) && playerInfoMap.TryGetValue(checkPlayerId, out var playerInfo))
			{
				var (voteModRole, voteAnotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(checkPlayerId);
				addVoteModRole(voteModRole, playerInfo, ref voteModifierRoles);
				addVoteModRole(voteAnotherRole, playerInfo, ref voteModifierRoles);

				var (voteHook, voteHookAnotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleHookVoteEnd>(checkPlayerId);
				addVoteHookRole(voteHook, playerInfo, voteHookRoles);
				addVoteHookRole(voteHookAnotherRole, playerInfo, voteHookRoles);
			}

			foreach (var voterState in states)
			{
				if (i == 0 && voterState.SkippedVote)
				{
					allVotes.Add(new VoteModification(voterState.VoterId, 255, 1));
				}
				else if (voterState.VotedForId == checkPlayerId)
				{
					allVotes.Add(new VoteModification(voterState.VoterId, checkPlayerId, 1));
				}
			}
		}

		// --- Phase 2: Vote Modification ---

		foreach (var (role, player) in voteModifierRoles.Values)
		{
			allVotes.AddRange(role.GetVoteModifications(player));
			role.ResetModifier();
		}

		// --- Phase 3: Animation and Final Count Calculation ---

		var finalVoteCounts = new Dictionary<byte, int>();
		var skipVoteTargetId = __instance.playerStates[0].TargetPlayerId;

		foreach (var vote in allVotes.GetAllVotes())
		{
			AnimateVote(__instance, vote, finalVoteCounts, playerAreaMap, playerInfoMap, skipVoteTargetId);
		}

		// --- Final Hook Call ---

		foreach (var (role, player) in voteHookRoles)
		{
			role.HookVoteEnd(__instance, player, finalVoteCounts);
		}

		return false;
	}

	private static void AnimateVote(MeetingHud instance, VoteModification vote, Dictionary<byte, int> voteIndex, IReadOnlyDictionary<byte, PlayerVoteArea> playerAreaMap, IReadOnlyDictionary<byte, NetworkedPlayerInfo> playerInfoMap, byte skipVoteTargetId)
	{
		if (!playerInfoMap.TryGetValue(vote.VoterId, out var voterInfo))
		{
			Debug.LogError($"Couldn't find player info for voter: {vote.VoterId}");
			return;
		}

		Transform targetTransform;
		byte effectiveTargetId = vote.TargetId;

		if (playerAreaMap.TryGetValue(vote.TargetId, out var targetArea))
		{
			targetTransform = targetArea.transform;
		}
		else
		{
			targetTransform = instance.SkippedVoting.transform;
			effectiveTargetId = skipVoteTargetId;
		}

		for (int i = 0; i < vote.VoteCount; i++)
		{
			if (!voteIndex.TryGetValue(effectiveTargetId, out int currentVoteCount))
			{
				currentVoteCount = 0;
			}
			instance.BloopAVoteIcon(voterInfo, currentVoteCount, targetTransform);
			voteIndex[effectiveTargetId] = currentVoteCount + 1;
		}
	}

	private static void addVoteModRole(
		IRoleVoteModifier? role, NetworkedPlayerInfo playerInfo,
		ref SortedList<int, (IRoleVoteModifier, NetworkedPlayerInfo)> voteModifier)
	{
		if (role is null)
		{
			return;
		}

		int order = role.Order;
		while (voteModifier.ContainsKey(order))
		{
			++order;
		}
		voteModifier.Add(order, (role, playerInfo));
	}

	private static void addVoteHookRole(
		IRoleHookVoteEnd? role, NetworkedPlayerInfo playerInfo,
		in List<(IRoleHookVoteEnd, NetworkedPlayerInfo)> voteHook)
	{
		if (role is null)
		{
			return;
		}

		voteHook.Add((role, playerInfo));
	}
}
