using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using Il2CppInterop.Runtime.InteropTypes.Arrays;

using ExtremeRoles.Helper;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAssign;


namespace ExtremeRoles.Patches.Meeting.Hud;

#nullable enable

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
public static class MeetingHudCheckForEndVotingPatch
{
	public static bool Prefix(
		MeetingHud __instance)
	{
		if (!RoleAssignState.Instance.IsRoleSetUpEnd) { return true; }

		if (!ExtremeRolesPlugin.ShipState.AssassinMeetingTrigger)
		{
			normalMeetingVote(__instance);
		}
		else
		{
			assassinMeetingVote(__instance);
		}

		return false;
	}

	private static void assassinMeetingVote(MeetingHud instance)
	{
		var (isVoteEnd, voteFor) = assassinVoteState(instance);

		if (!isVoteEnd) { return; }

		//GameData.PlayerInfo exiled = Helper.Player.GetPlayerControlById(voteFor).Data;
		Il2CppStructArray<MeetingHud.VoterState> array =
			new Il2CppStructArray<MeetingHud.VoterState>(
				instance.playerStates.Length);

		if (voteFor == 254 || voteFor == byte.MaxValue)
		{
			bool targetImposter;
			do
			{
				int randomPlayerIndex = UnityEngine.Random.RandomRange(
					0, instance.playerStates.Length);
				voteFor = instance.playerStates[randomPlayerIndex].TargetPlayerId;

				targetImposter = ExtremeRoleManager.GameRole[voteFor].IsImpostor();

			}
			while (targetImposter);
		}

		Logging.Debug($"IsSuccess?:{ExtremeRoleManager.GameRole[voteFor].Id == ExtremeRoleId.Marlin}");

		using (var caller = RPCOperator.CreateCaller(
			RPCOperator.Command.AssasinVoteFor))
		{
			caller.WriteByte(voteFor);
		}
		RPCOperator.AssasinVoteFor(voteFor);

		for (int i = 0; i < instance.playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = instance.playerStates[i];
			if (playerVoteArea.TargetPlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId)
			{
				playerVoteArea.VotedFor = voteFor;
			}
			else
			{
				playerVoteArea.VotedFor = 254;
			}
			instance.SetDirtyBit(1U);

			array[i] = new MeetingHud.VoterState
			{
				VoterId = playerVoteArea.TargetPlayerId,
				VotedForId = playerVoteArea.VotedFor
			};

		}
		instance.RpcVotingComplete(array, null, true);
	}

	private static (bool, byte) assassinVoteState(MeetingHud instance)
	{
		bool isVoteEnd = false;
		byte voteFor = byte.MaxValue;

		foreach (PlayerVoteArea playerVoteArea in instance.playerStates)
		{
			if (playerVoteArea.TargetPlayerId == ExtremeRolesPlugin.ShipState.ExiledAssassinId)
			{
				isVoteEnd = playerVoteArea.DidVote;
				voteFor = playerVoteArea.VotedFor;
				break;
			}
		}

		return (isVoteEnd, voteFor);
	}

	private static void addVoteModRole(
		IRoleVoteModifier role, byte rolePlayerId,
		ref SortedList<int, (IRoleVoteModifier, byte)> voteModifier)
	{
		if (role == null) { return; }

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
		if (!instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote)) { return; }

		Dictionary<byte, int> result = calculateVote(instance);

		bool isExiled = true;

		KeyValuePair<byte, int> exiledResult = new KeyValuePair<byte, int>(
			byte.MaxValue, int.MinValue);
		foreach (KeyValuePair<byte, int> item in result)
		{
			if (item.Value > exiledResult.Value)
			{
				exiledResult = item;
				isExiled = false;
			}
			else if (item.Value == exiledResult.Value)
			{
				isExiled = true;
			}
		}

		GameData.PlayerInfo? exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(
			(GameData.PlayerInfo v) => !isExiled && v.PlayerId == exiledResult.Key);

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

		instance.RpcVotingComplete(array, exiled, isExiled);
	}
}
