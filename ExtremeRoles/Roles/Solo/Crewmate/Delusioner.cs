using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Module.AbilityButton.Roles;
using ExtremeRoles.Resources;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;
using ExtremeRoles.Roles.API.Extension.State;
using System.Collections.Generic;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Delusioner : 
        SingleRoleBase, 
        IRoleAbility,
        IRoleAwake<RoleTypes>,
        IRoleVoteModifier
    {
        public int Order => throw new System.NotImplementedException();

        public bool IsAwake => throw new System.NotImplementedException();

        public RoleTypes NoneAwakeRole => throw new System.NotImplementedException();

        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException(); 
            set => throw new System.NotImplementedException(); 
        }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public string GetFakeOptionString()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new System.NotImplementedException();
        }

        public void ModifiedVote(byte rolePlayerId, ref Dictionary<byte, byte> voteTarget, ref Dictionary<byte, int> voteResult)
        {
            throw new System.NotImplementedException();
        }

        public void ModifiedVoteAnime(MeetingHud instance, GameData.PlayerInfo rolePlayer, ref Dictionary<byte, int> voteIndex)
        {
            throw new System.NotImplementedException();
        }

        public void ResetModifier()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
