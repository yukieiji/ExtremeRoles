using System.Text;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAssign;
using ExtremeRoles.Module.SystemType.OnemanMeetingSystem;
using ExtremeRoles.Module.SystemType.Roles;

namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class MeetingHudCheckForEndVotingPatch
{
	private readonly record struct ExiledPlayer(byte PlayerId = byte.MaxValue, int VoteNum = int.MinValue);

	public static bool Prefix(
		MeetingHud __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		if (!OnemanMeetingSystemManager.TryGetActiveSystem(out var system))
		{
			normalMeetingVote(__instance);
		}
		else
		{
			system.OverrideMeetingHudCheckForEndVoting(__instance);
		}

		return false;
	}

	private static void addVoteModRole(
		IRoleVoteModifier? role, byte rolePlayerId,
		ref SortedList<int, (IRoleVoteModifier, byte)> voteModifier)
	{
		if (role is null) { return; }

		int order = role.Order;
		// 同じ役職は同じ優先度になるので次の優先度になるようにセット
		while (voteModifier.ContainsKey(order))
		{
			++order;
		}
		voteModifier.Add(order, (role, rolePlayerId));
	}

	private static Dictionary<byte, int> calculateVote(MeetingHud instance)
	{

		RPCOperator.Call(RPCOperator.Command.CloseMeetingVoteButton);
		RPCOperator.CloseMeetingButton();

		Dictionary<byte, int> voteResult = new Dictionary<byte, int>();
		Dictionary<byte, byte> voteTarget = new Dictionary<byte, byte>();

		SortedList<int, (IRoleVoteModifier, byte)> voteModifier = new SortedList<int, (IRoleVoteModifier, byte)>();

		foreach (PlayerVoteArea playerVoteArea in instance.playerStates)
		{

			byte playerId = playerVoteArea.TargetPlayerId;

			// 切断されたプレイヤーは残っている状態で役職を持たない状態になるのでキーチェックはしておく
			if (ExtremeRoleManager.GameRole.ContainsKey(playerId))
			{
				// 投票をいじる役職か？
				var (voteModRole, voteAnotherRole) =
					ExtremeRoleManager.GetInterfaceCastedRole<IRoleVoteModifier>(playerId);
				addVoteModRole(voteModRole, playerId, ref voteModifier);
				addVoteModRole(voteAnotherRole, playerId, ref voteModifier);
			}

			// 投票先を全格納
			voteTarget.Add(playerId, playerVoteArea.VotedFor);

			if (playerVoteArea.VotedFor != 252 &&
				playerVoteArea.VotedFor != 255 &&
				playerVoteArea.VotedFor != 254)
			{
				int currentVotes;
				if (voteResult.TryGetValue(playerVoteArea.VotedFor, out currentVotes))
				{
					voteResult[playerVoteArea.VotedFor] = currentVotes + 1;
				}
				else
				{
					voteResult[playerVoteArea.VotedFor] = 1;
				}
			}
		}

		foreach (var (role, playerId) in voteModifier.Values)
		{
			role.ModifiedVote(playerId, ref voteTarget, ref voteResult);
		}
		return voteResult;

	}

	private static void normalMeetingVote(MeetingHud instance)
	{
		var trashMeeting = MonikaTrashSystem.TryGet(out var monikaSystem) ? monikaSystem : null;
		if (!instance.playerStates.All(
				(PlayerVoteArea ps) =>
					ps.AmDead || ps.DidVote ||
					(trashMeeting is not null && trashMeeting.InvalidPlayer(ps))))
		{
			return;
		}

		var logger = ExtremeRolesPlugin.Logger;
		logger.LogInfo(" ----- Voteing is End ----- ");

		Dictionary<byte, int> voteResult = calculateVote(instance);

		bool isTie = true;
		var result = new ExiledPlayer();
		foreach (var (playerId, voteNum) in voteResult)
		{
			if (voteNum > result.VoteNum)
			{
				result = new ExiledPlayer(playerId, voteNum);
				isTie = false;
			}
			else if (voteNum == result.VoteNum)
			{
				isTie = true;
			}
		}

		NetworkedPlayerInfo? exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(
			(NetworkedPlayerInfo v) => !isTie && v.PlayerId == result.PlayerId);

		if (exiled != null)
		{
			var builder = new StringBuilder();
			builder
				.AppendLine("--- Exiled Player Info ---")
				.Append(" - PlayerId:").Append(result.PlayerId).AppendLine()
				.Append(" - PlayerName:").AppendLine(exiled.PlayerName)
				.Append(" - IsDead:").Append(exiled.IsDead).AppendLine()
				.Append(" - VoteNum:").Append(result.VoteNum);
			logger.LogInfo(builder.ToString());
		}
		else
		{
			logger.LogInfo("Exiled Player is None!!");
		}

		MeetingHud.VoterState[] array = new MeetingHud.VoterState[instance.playerStates.Length];
		for (int i = 0; i < instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = instance.playerStates[i];
			array[i] = new MeetingHud.VoterState
			{
				VoterId = playerVoteArea.TargetPlayerId,
				VotedForId = playerVoteArea.VotedFor
			};
		}

		instance.RpcVotingComplete(array, exiled, isTie);
	}
}
