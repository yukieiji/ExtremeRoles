using System;
using System.Collections.Generic;

using Hazel;
using UnityEngine;

using ExtremeRoles.Module;

namespace ExtremeRoles.GhostRoles.API
{
    public class VanillaGhostRoleWrapper : GhostRoleBase
    {
        public VanillaGhostRoleWrapper(
            RoleTypes vanillaRoleId) : base(
                true, Roles.API.ExtremeRoleType.Crewmate,
                ExtremeGhostRoleId.VanillaRole,
                "", Color.white)
        {
            this.RoleName = vanillaRoleId.ToString();

            switch (vanillaRoleId)
            {
                case RoleTypes.GuardianAngel:
                    this.Task = true;
                    this.TeamType = Roles.API.ExtremeRoleType.Crewmate;
                    this.NameColor = Palette.ClearWhite;
                    break;
                default:
                    break;
            }
        }

        public override HashSet<ExtremeGhostRoleId> GetRoleFilter() => new HashSet<ExtremeGhostRoleId> ();

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            return;
        }

        public override void CreateAbility()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            return;
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new NotImplementedException();
        }

        protected override void UseAbility(
            MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
