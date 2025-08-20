using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;


namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

public sealed class VoteInfoCollector()
{
	public IEnumerable<VoteInfo> Vote =>
		vote
			// VoterIdとTargetIdの組み合わせでグループ化
			.GroupBy(vote => new { vote.VoterId, vote.TargetId })
			// 各グループから新しいVoteInfoを作成
			.Select(
				group => new VoteInfo(
					group.Key.VoterId,
					group.Key.TargetId,
					group.Sum(vote => vote.Count) // グループ内のContを合計
				)
			);

	private readonly List<VoteInfo> vote = new List<VoteInfo>();

	public void AddSkip(byte voter)
	{
		this.vote.Add(new VoteInfo(voter, PlayerVoteArea.SkippedVote, 1));
	}

	public void AddTo(byte voter, byte to)
	{
		this.vote.Add(new VoteInfo(voter, to, 1));
	}

	public void AddRange(IEnumerable<VoteInfo> votes)
	{
		this.vote.AddRange(votes);
	}
}

public sealed class PlayerRoleInfo(int size)
{
	public IEnumerable<(IRoleVoteModifier, NetworkedPlayerInfo)> Modifier => this.voteModifier.Values;
	public IEnumerable<(IRoleHookVoteEnd, NetworkedPlayerInfo)> Hook => this.voteHook;
	public IReadOnlyDictionary<byte, NetworkedPlayerInfo> Player => this.playerCache;

	private readonly SortedList<int, (IRoleVoteModifier, NetworkedPlayerInfo)> voteModifier = [];
	private readonly List<(IRoleHookVoteEnd, NetworkedPlayerInfo)> voteHook = [];
	private readonly Dictionary<byte, NetworkedPlayerInfo> playerCache = new Dictionary<byte, NetworkedPlayerInfo>(size);

	public void Add(NetworkedPlayerInfo player)
	{
		byte playerId = player.PlayerId;
		this.playerCache.Add(playerId, player);

		if (!ExtremeRoleManager.GameRole.ContainsKey(playerId))
		{
			return;
		}
		var (voteModRole, voteAnotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(playerId);
		addVoteModRole(voteModRole, player);
		addVoteModRole(voteAnotherRole, player);

		var (voteHook, voteHookAnotherRole) = ExtremeRoleManager.GetInterfaceCastedRole<IRoleHookVoteEnd>(playerId);
		addVoteHookRole(voteHook, player);
		addVoteHookRole(voteHookAnotherRole, player);
	}

	private void addVoteModRole(IRoleVoteModifier? role, NetworkedPlayerInfo playerInfo)
	{
		if (role is null)
		{
			return;
		}

		int order = role.Order;
		while (this.voteModifier.ContainsKey(order))
		{
			++order;
		}
		this.voteModifier.Add(order, (role, playerInfo));
	}

	private void addVoteHookRole(
		IRoleHookVoteEnd? role, NetworkedPlayerInfo playerInfo)
	{
		if (role is null)
		{
			return;
		}

		this.voteHook.Add((role, playerInfo));
	}
}

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
		var voteInfo = new VoteInfoCollector();
		var playerAreaMap = new Dictionary<byte, PlayerVoteArea>(__instance.playerStates.Length);
		int playerNum = GameData.Instance.AllPlayers.Count;
		var playerRoleInfo = new PlayerRoleInfo(GameData.Instance.AllPlayers.Count);

		// プレイヤーの情報
		foreach (var player in GameData.Instance.AllPlayers)
		{
			playerRoleInfo.Add(player);
		}

		// 表の情報を初期化
		for (int i = 0; i < __instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			playerVoteArea.ClearForResults();
			playerAreaMap[playerVoteArea.TargetPlayerId] = playerVoteArea;
		}
		playerAreaMap[PlayerVoteArea.SkippedVote] = __instance.playerStates[0];

		// 各種表の情報
		foreach (var voter in states)
		{
			byte voterId = voter.VoterId;
			byte voteTo = voter.VotedForId;
			if (voter.SkippedVote)
			{
				voteInfo.AddSkip(voterId);
			}
			else if (
				voter.AmDead || 
				voteTo == PlayerVoteArea.MissedVote || 
				voteTo == PlayerVoteArea.HasNotVoted)
			{
				continue;
			}
			else
			{
				voteInfo.AddTo(voterId, voteTo);
			}
		}

		// --- Phase 2: Vote Modification ---
		foreach (var (role, player) in playerRoleInfo.Modifier)
		{
			voteInfo.AddRange(role.GetModdedVoteInfo(player));
			role.ResetModifier();
		}

		// --- Phase 3: Animation and Final Count Calculation ---
		var finalVoteCount = new Dictionary<byte, int>(playerNum);
		// .Voteで重複は削除されている
		foreach (var vote in voteInfo.Vote)
		{
			byte target = vote.TargetId;
			int curTargetCount = finalVoteCount.GetValueOrDefault(target, 0);
			animateVote(__instance, vote, curTargetCount, playerAreaMap, playerRoleInfo.Player);
			finalVoteCount[target] = curTargetCount + vote.Count;
		}

		// --- Final Hook Call ---
		foreach (var (role, player) in playerRoleInfo.Hook)
		{
			role.HookVoteEnd(__instance, player, finalVoteCount);
		}

		return false;
	}

	private static void animateVote(
		MeetingHud instance,
		in VoteInfo vote,
		int startIndex,
		IReadOnlyDictionary<byte, PlayerVoteArea> playerAreaMap,
		IReadOnlyDictionary<byte, NetworkedPlayerInfo> playerInfoMap)
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
		}

		for (int i = 0; i < vote.Count; i++)
		{
			instance.BloopAVoteIcon(voterInfo, startIndex + i, targetTransform);
		}
	}
}
