using System;

using ExtremeRoles.Module.AbilityButton.Roles;


namespace ExtremeRoles.Module.AbilityButton.Mode
{
    public sealed class ReusableAbilityButtonMode<T> :
        AbilityButtonModeBase<ReusableAbilityButton, T, NormalAbilityButtonMode>
        where T : struct, Enum
    {
        public ReusableAbilityButtonMode(ReusableAbilityButton button) : base(button)
        { }

        public override void SwithMode(T modeValue)
        {
            NormalAbilityButtonMode mode = this.Mode[modeValue];

            this.Button.SetButtonText(mode.Text);
            this.Button.SetButtonImg(mode.Img);
            this.Button.SetAbilityActiveTime(mode.ActiveTime);
        }
    }
}
