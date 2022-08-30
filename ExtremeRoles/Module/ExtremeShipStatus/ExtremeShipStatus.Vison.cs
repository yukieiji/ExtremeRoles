using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Module.SpecialWinChecker;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Performance.Il2Cpp;

namespace ExtremeRoles.Module.ExtremeShipStatus
{
    public sealed partial class ExtremeShipStatus
    {
        public enum ForceVisonType
        {
            None,
            LastWolfLightOff
        }

        public ForceVisonType CurVison => this.modVison;
        private ForceVisonType modVison;

        public void SetVison(ForceVisonType newVison)
        {
            this.modVison = newVison;
        }

        public void ResetVison()
        {
            this.modVison = ForceVisonType.None;
        }
        public bool IsCustomVison() => this.modVison != ForceVisonType.None;
    }
}
