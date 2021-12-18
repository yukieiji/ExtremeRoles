using ExtremeRoles.Module;
using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace ExtremeRoles.Roles.Solo.Neutral
{
    public class Alice : SingleRoleAbs
    {
        public Alice(): base(
            ExtremeRoleId.Alice,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Alice.ToString(),
            ColorPalette.AliceGold,
            true, false, true, true)
        {}

        protected override void CreateSpecificOption(
            CustomOption parentOps)
        {}

        protected override void RoleSpecificInit()
        {}
    }
}
