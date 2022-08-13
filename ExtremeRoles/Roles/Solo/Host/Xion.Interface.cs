using ExtremeRoles.Module;
using ExtremeRoles.Roles.API.Interface;
using System;
using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Host
{
    public sealed partial class Xion : IRoleResetMeeting, IRoleMeetingButtonAbility, IRoleUpdate
    {
        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            throw new NotImplementedException();
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {
            throw new NotImplementedException();
        }

        public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance)
        {
            throw new NotImplementedException();
        }

        public void ResetOnMeetingEnd()
        {
            throw new NotImplementedException();
        }

        public void ResetOnMeetingStart()
        {
            throw new NotImplementedException();
        }

        public void SetSprite(SpriteRenderer render)
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            throw new NotImplementedException();
        }
    }
}
