using System.Collections.Generic;
using ExtremeRoles.Module.Meeting;

namespace ExtremeRoles.Roles.API.Interface;


public interface IRoleVoteModifier
{
    public enum ModOrder : int
    {
        CaptainSpecialVote = 0,
        GamblerAddVote,

        DelusionerCheckVote = 88659,
    }

    public int Order { get; }

    public void ModifiedVote(
        byte rolePlayerId,
        ref Dictionary<byte, byte> voteTarget,
        ref Dictionary<byte, int> voteResult);

    public IEnumerable<VoteModification> GetVoteModifications(
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
