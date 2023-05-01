using System.Collections.Generic;

namespace ExtremeRoles.Roles.API.Interface
{

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
            GameData.PlayerInfo rolePlayer,
            ref Dictionary<byte, int> voteIndex);
        
        public void ResetModifier();
    }
}
