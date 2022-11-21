using UnityEngine;

using ExtremeRoles.GhostRoles;
using ExtremeRoles.GhostRoles.API;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module;
using System.Collections.Generic;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.AbilityButton.Roles;

namespace ExtremeRoles.Roles.Combination
{
    public sealed class Kids : GhostAndAliveCombinationRoleManagerBase
    {
        public const string Name = "Kids";
        public Kids() : base(
            Name, new Color(255f, 255f, 255f), 2,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Delinquent());

            this.CombGhostRole.Add(
                ExtremeRoleId.Delinquent, new Wisp());
        }
    }

    public sealed class Delinquent : MultiAssignRoleBase, IRoleAbility
    {
        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException(); 
            set => throw new System.NotImplementedException();
        }

        public Delinquent() : base(
            ExtremeRoleId.Delinquent,
            ExtremeRoleType.Neutral,
            ExtremeRoleId.Delinquent.ToString(),
            Palette.White,
            false, false, false, false,
            tab: OptionTab.Combination)
        { }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            return true;
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            
        }

        protected override void RoleSpecificInit()
        {
            
        }
    }

    public sealed class Wisp : GhostRoleBase
    {
        public Wisp() : base(
            false, ExtremeRoleType.Neutral,
            ExtremeGhostRoleId.Wisp,
            ExtremeGhostRoleId.Wisp.ToString(),
            Palette.White,
            OptionTab.Combination)
        { }

        public override void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public override HashSet<ExtremeRoleId> GetRoleFilter()
        {
            throw new System.NotImplementedException();
        }

        public override void Initialize()
        {
            
        }

        public override void ReseOnMeetingEnd()
        {
            throw new System.NotImplementedException();
        }

        public override void ReseOnMeetingStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            
        }

        protected override void UseAbility(RPCOperator.RpcCaller caller)
        {
            throw new System.NotImplementedException();
        }
    }
}
