using System.Collections.Generic;

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

    public void ModifiedVoteAnime(
        MeetingHud instance,
        NetworkedPlayerInfo rolePlayer,
        ref Dictionary<byte, int> voteIndex);
        
    public void ResetModifier();
}

public interface IRoleHookVoteEnd
{
	public void HookVoteEnd(
		MeetingHud instance,
		NetworkedPlayerInfo rolePlayer,
		IReadOnlyDictionary<byte, int> voteIndex);
}
