using System;

using UnityEngine;

namespace ExtremeRoles.Roles.API.Interface
{
    public interface IRoleMeetingButtonAbility
    {
        public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance);

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton);

        public Action CreateAbilityAction(PlayerVoteArea instance);

        public void SetSprite(SpriteRenderer render);
    }
}
