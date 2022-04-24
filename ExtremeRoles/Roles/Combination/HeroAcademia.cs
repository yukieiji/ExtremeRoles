using System.Collections.Generic;

using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.RoleAbilityButton;
using ExtremeRoles.Resources;

namespace ExtremeRoles.Roles.Combination
{
    internal class AllPlayerArrows
    {
        public void SetActive(bool active)
        {

        }

        public void Update()
        {

        }
    }

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
        public RoleAbilityButtonBase Button
        {
            get => this.searchButton;
            set
            {
                this.searchButton = value;
            }
        }

        private RoleAbilityButtonBase searchButton;
        private AllPlayerArrows arrow;

        public Hero(
            ) : base(
                ExtremeRoleId.Hero,
                ExtremeRoleType.Crewmate,
                ExtremeRoleId.Hero.ToString(),
                Palette.ImpostorRed,
                false, true, false, false)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("search"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            arrow.SetActive(false);
        }

        public void Update(PlayerControl rolePlayer)
        {
            arrow.Update();
        }

        public bool UseAbility()
        {
            arrow.SetActive(true);
            return true;
        }

        public void CleanUp()
        {
            arrow.SetActive(false);
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
    public class Villain : MultiAssignRoleBase, IRoleAbility, IRoleUpdate
    {
        public RoleAbilityButtonBase Button
        {
            get => this.searchButton;
            set
            {
                this.searchButton = value;
            }
        }

        private RoleAbilityButtonBase searchButton;
        private AllPlayerArrows arrow;

        public Villain(
            ) : base(
                ExtremeRoleId.Villain,
                ExtremeRoleType.Impostor,
                ExtremeRoleId.Villain.ToString(),
                Palette.ImpostorRed,
                true, false, true, true)
        { }

        public void CreateAbility()
        {
            this.CreateNormalAbilityButton(
                Translation.GetString("search"),
                Loader.CreateSpriteFromResources(
                    Path.TestButton),
                abilityCleanUp: CleanUp);
        }

        public bool IsAbilityUse() => this.IsCommonUse();

        public void RoleAbilityResetOnMeetingEnd()
        {
            return;
        }

        public void RoleAbilityResetOnMeetingStart()
        {
            arrow.SetActive(false);
        }

        public void Update(PlayerControl rolePlayer)
        {
            arrow.Update();
        }

        public bool UseAbility()
        {
            arrow.SetActive(true);
            return true;
        }

        public void CleanUp()
        {
            arrow.SetActive(false);
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
        public enum VigilanteCondition
        {
            NewLawInTheShip,
            NewHeroForTheShip,
            NewVillainForTheShip,
        }

        public RoleAbilityButtonBase Button
        {
            get => this.callButton;
            set
            {
                this.callButton = value;
            }
        }

        private RoleAbilityButtonBase callButton;

        public Vigilante(
            ) : base(
                ExtremeRoleId.Vigilante,
                ExtremeRoleType.Neutral,
                ExtremeRoleId.Vigilante.ToString(),
                Palette.ImpostorRed,
                false, false, false, false)
        { }

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
