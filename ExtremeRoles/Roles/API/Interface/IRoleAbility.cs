using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.API.Interface
{
    interface IRoleAbility
    {
        public RoleAbilityButton Button
        {
            get => this.Button;
            set
            {
                Button = value;
            }
        }

        public void CreateAbilityButton();

        public void UseAbility();

        public bool IsAbilityUse();

        protected void AbilityButton(
            Sprite sprite,
            Vector3? positionOffset = null,
            Action abilityCleanUp = null,
            KeyCode hotkey = KeyCode.F,
            bool mirror = false)
        {

            Vector3 offset = positionOffset ?? new Vector3(-1.8f, -0.06f, 0);

            RoleAbilityButton abilityButton = new RoleAbilityButton(
                this.UseAbility,
                this.IsAbilityUse,
                sprite,
                offset,
                abilityCleanUp,
                hotkey,
                mirror);

            this.Button = abilityButton;
        }
    }
}
