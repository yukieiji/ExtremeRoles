using System.Collections.Generic;
using System.Linq;

using ExtremeRoles.Module.Meeting;

namespace ExtremeRoles.Roles.API.Interface;

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


public interface IRoleVoteModifier
{
    public enum ModOrder : int
    {
        CaptainSpecialVote = 0,
        GamblerAddVote,

        DelusionerCheckVote = 88659,
		CEOOverrideVote,
	}

    public int Order { get; }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult);

    public IEnumerable<VoteInfo> GetModdedVoteInfo(
		VoteInfoCollector collector,
		NetworkedPlayerInfo rolePlayer);
        
    public void ResetModifier();
}

public interface IRoleHookVoteEnd
{
	public void HookVoteEnd(
		MeetingHud instance,
		NetworkedPlayerInfo rolePlayer,
		IReadOnlyDictionary<byte, int> voteIndex);
}
