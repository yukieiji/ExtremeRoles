using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;
using ExtremeRoles.Roles;
using ExtremeRoles.Roles.API;
using Hazel;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeRoles.GhostRoles.Impostor
{
    public class NoNameNow : GhostRoleBase
    {
        private Vent targetVent;

        public NoNameNow() : base(
            true,
            ExtremeRoleType.Impostor,
            ExtremeGhostRoleId.NoNameNow,
            ExtremeGhostRoleId.NoNameNow.ToString(),
            Palette.ImpostorRed)
        { }

        public override void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter() => new HashSet<ExtremeRoleId>();

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override void ReseOnMeetingEnd()
        {
            return;
        }

        public override void ReseOnMeetingStart()
        {
            this.targetVent = null;
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void UseAbility(MessageWriter writer)
        {
            writer.Write(targetVent.Id);
        }
    }
}
