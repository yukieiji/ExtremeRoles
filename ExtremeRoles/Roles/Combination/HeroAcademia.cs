using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;

namespace ExtremeRoles.Roles.Combination
{
    public class HeroAcademia : ConstCombinationRoleManagerBase
    {
        public const string Name = "HeroAca";
        public HeroAcademia() : base(
            Name, new Color(255f, 255f, 255f), 3,
            OptionHolder.MaxImposterNum)
        {
            this.Roles.Add(new Hero());
            this.Roles.Add(new Villain());
            this.Roles.Add(new Vigilante());
        }
    }

    public class Hero : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public Hero(
            ) : base(
                ExtremeRoleId.Hero,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Hero.ToString(),
                Palette.ImpostorRed,
                false, true, false, false)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

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

        public void Update(PlayerControl rolePlayer)
        {
            throw new System.NotImplementedException();
        }

        public bool UseAbility()
        {
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
    }
    public class Villain : MultiAssignRoleBase, IRoleAbility
    {
        public Villain(
            ) : base(
                ExtremeRoleId.Villain,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Villain.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        { }

        public RoleAbilityButtonBase Button
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

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
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }

    }
    public class Vigilante : MultiAssignRoleBase, IRoleAbility, IRoleWinPlayerModifier
    {
        public Vigilante(
            ) : base(
                ExtremeRoleId.Vigilante,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Vigilante.ToString(),
                Palette.ImpostorRed,
                false, false, false, false)
        { }

        public RoleAbilityButtonBase Button
        { 
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

        public void CreateAbility()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAbilityUse()
        {
            throw new System.NotImplementedException();
        }

        public void ModifiedWinPlayer(
            GameData.PlayerInfo rolePlayerInfo,
            GameOverReason reason,
            Il2CppSystem.Collections.Generic.List<WinningPlayerData> winner)
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
            throw new System.NotImplementedException();
        }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }

    }
}
