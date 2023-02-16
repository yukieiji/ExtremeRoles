using System;

using UnityEngine;

using ExtremeRoles.Module.AbilityButton.Roles;


namespace ExtremeRoles.Module.AbilityButton.Mode
{
    public struct AbilityCountButtonMode
    {
        public string Text;
        public Sprite Img;
        public float ActiveTime;
    }

    public sealed class AbilityCountButtonMode<T> :
        AbilityButtonModeBase<AbilityCountButton, T, AbilityCountButtonMode>
        where T : struct, Enum
    {
        public AbilityCountButtonMode(AbilityCountButton button) : base(button)
        { }

        public override void SwithMode(T modeValue)
        {
            AbilityCountButtonMode mode = this.Mode[modeValue];

            this.Button.SetButtonText(mode.Text);
            this.Button.SetButtonImg(mode.Img);
            this.Button.SetAbilityActiveTime(mode.ActiveTime);
        }
    }
}
