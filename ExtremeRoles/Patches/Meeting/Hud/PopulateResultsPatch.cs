using System;
using System.Collections.Generic;
using System.Linq;

using AmongUs.GameOptions;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Extension.Il2Cpp;
using ExtremeRoles.GhostRoles;
using ExtremeRoles.Module.CustomMonoBehaviour;
using ExtremeRoles.Module.Event;
using ExtremeRoles.Module.Meeting;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Roles.Combination.Avalon;

using CommomSystem = ExtremeRoles.Roles.API.Systems.Common;
using UnityObject = UnityEngine.Object;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

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
		if (!GameProgressSystem.IsGameNow)
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
		foreach (var pva in __instance.playerStates)
		{
			pva.ClearForResults();
			playerAreaMap[pva.TargetPlayerId] = pva;
		}

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
			var info = role.GetModdedVoteInfo(voteInfo, player);
			voteInfo.AddRange(info);
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

		VoteSwapSystem.AnimateSwap(__instance, playerAreaMap);

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

		var targetTransform =
			playerAreaMap.TryGetValue(vote.TargetId, out var targetArea) ?
			targetArea.transform : instance.SkippedVoting.transform;

		var swapTarget =
			VoteSwapSystem.TryGetSwapTarget(vote.TargetId, out byte newTarget) &&
			playerAreaMap.TryGetValue(newTarget, out var swapTargetArea) ?
			swapTargetArea.transform : null;

		for (int i = 0; i < vote.Count; i++)
		{
			int index = startIndex + i;

			// swapTargetがある場合
			if (swapTarget != null)
			{
				// SwapTargetからアニメーションを開始させる
				var voteRend = createVoteRenderer(instance, voterInfo, index, targetTransform);
				var swapper = targetTransform.gameObject.TryAddComponent<VoteSwapSpreader>();
				swapper.Add(voteRend, swapTarget);
			}
			else if (targetTransform.TryGetComponent<VoteSpreader>(out var spreader))
			{
				var voteRend = createVoteRenderer(instance, voterInfo, index, targetTransform);
				spreader.AddVote(voteRend);
			}
		}
	}

	private static SpriteRenderer createVoteRenderer(MeetingHud instance, NetworkedPlayerInfo voter, int index, Transform target)
	{
		var spriteRenderer = UnityObject.Instantiate(instance.PlayerVotePrefab);

		var role = ExtremeRoleManager.GetLocalPlayerRole();

		bool canSeeVote =
			(role is Marlin marlin && marlin.CanSeeVote) ||
			(role is Assassin assassin && assassin.CanSeeVote);

		if (!GameManager.Instance.LogicOptions.GetAnonymousVotes() ||
			canSeeVote ||
			(
				PlayerControl.LocalPlayer.Data.IsDead &&
				ClientOption.Instance.GhostsSeeRole.Value &&
				!isVoteSeeBlock(role)
			))
		{
			PlayerMaterial.SetColors(voter.DefaultOutfit.ColorId, spriteRenderer);
		}
		else
		{
			PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
		}

		spriteRenderer.transform.SetParent(target);
		spriteRenderer.transform.localScale = Vector3.zero;

		if (target.TryGetComponent<PlayerVoteArea>(out var component))
		{
			spriteRenderer.material.SetInt(PlayerMaterial.MaskLayer, component.MaskLayer);
		}

		instance.StartCoroutine(
			Effects.Bloop(
				(float)index * 0.3f,
				spriteRenderer.transform, 1f, 0.5f));

		return spriteRenderer;
	}

	private static bool isVoteSeeBlock(SingleRoleBase role)
	{
		if (ExtremeGhostRoleManager.GameRole.ContainsKey(
				PlayerControl.LocalPlayer.PlayerId) ||
			PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel)
		{
			return true;
		}
		else if (CommomSystem.IsForceInfoBlockRoleWithoutAssassin(role))
		{
			return ExtremeRolesPlugin.ShipState.IsAssassinAssign;
		}
		return role.IsBlockShowMeetingRoleInfo();
	}
}
