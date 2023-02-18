using System.Collections.Generic;

using UnityEngine;
using AmongUs.GameOptions;

using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Module;

namespace ExtremeRoles.GhostRoles
{
    public sealed class VanillaGhostRoleWrapper : GhostRoleBase
    {
        private RoleTypes vanillaRoleId;

        public VanillaGhostRoleWrapper(
            RoleTypes vanillaRoleId) : base(
                true, Roles.API.ExtremeRoleType.Crewmate,
                ExtremeGhostRoleId.VanillaRole,
                "", Color.white)
        {
            this.vanillaRoleId = vanillaRoleId;
            this.RoleName = vanillaRoleId.ToString();

            switch (vanillaRoleId)
            {
                case RoleTypes.GuardianAngel:
                case RoleTypes.CrewmateGhost:
                    this.Task = true;
                    this.TeamType = Roles.API.ExtremeRoleType.Crewmate;
                    this.NameColor = Palette.White;
                    break;
                case RoleTypes.ImpostorGhost:
                    this.Task = false;
                    this.TeamType = Roles.API.ExtremeRoleType.Impostor;
                    this.NameColor = Palette.ImpostorRed;
                    break;
                default:
                    break;
            }
        }

        public override string GetImportantText()
        {
            string addText = this.vanillaRoleId switch
            {
                RoleTypes.GuardianAngel or RoleTypes.CrewmateGhost => 
                    Helper.Translation.GetString("crewImportantText"),
                RoleTypes.ImpostorGhost =>
                    Helper.Translation.GetString("impImportantText"),
                _ => string.Empty,
            };
            return Helper.Design.ColoedString(
                this.NameColor,
                $"{this.GetColoredRoleName()}: {addText}");
        }

        public override string GetFullDescription()
        {
            return Helper.Translation.GetString(
                $"{this.vanillaRoleId}FullDescription");
        }


        public override HashSet<Roles.ExtremeRoleId> GetRoleFilter() => new HashSet<Roles.ExtremeRoleId> ();

        protected override void OnMeetingEndHook()
        {
            return;
        }

        protected override void OnMeetingStartHook()
        {
            return;
        }

        public override void CreateAbility()
        {
            throw new System.Exception("Don't call this class method!!");
        }

        public override void Initialize()
        {
            return;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        {
            throw new System.Exception("Don't call this class method!!");
        }

        protected override void UseAbility(
            RPCOperator.RpcCaller caller)
        {
            throw new System.Exception("Don't call this class method!!");
        }
    }
}
