using System;
using System.Collections.Generic;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Jackal : SingleRoleBase
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

    public class SideKick : SingleRoleBase
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
