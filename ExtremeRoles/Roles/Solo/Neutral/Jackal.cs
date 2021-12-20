using System;
using System.Collections.Generic;

using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jackal : SingleRoleAbs, IRoleAbility
    {
        public List<byte> SideKickPlayerId = new List<byte>();

        AbilityButton Button;
        public Jackal() : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        { }

        public Jackal(int buttonCount) : base(
            ExtremeRoleId.Jackal,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Jackal.ToString(),
            ColorPalette.JackalBlue,
            true, false, true, false)
        { }

        public void CreateAbility()
        {
            RoleAbilityButton abilityButton = new RoleAbilityButton();
            abilityButton.ButtonInit("a", "a");
            abilityButton.SetEnabled();
            abilityButton.gameObject.SetActive(true);
            abilityButton.SetCoolDown(0f, 1f);
        }

        public void UseAbility()
        {
            throw new NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new NotImplementedException();
        }
    }

    public class SideKick : SingleRoleAbs
    {
        public SideKick() : base(
            ExtremeRoleId.SideKick,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.SideKick.ToString(),
            ColorPalette.JackalBlue,
            false, false, false, false)
        { }

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new NotImplementedException();
        }
    }

}
