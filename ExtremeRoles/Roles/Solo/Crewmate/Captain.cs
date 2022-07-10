using System;
using System.Collections.Generic;
using UnityEngine;

using ExtremeRoles.Helper;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Captain : SingleRoleBase, IRoleAwake<RoleTypes>, IRoleMeetingButtonAbility
    {
        public enum CaptainOption
        {
            AwakeTaskGage
        }

        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        private bool awakeRole;
        private float awakeTaskGage;
        private bool awakeHasOtherVision;

        private float chargedVote;

        public Captain() : base(
            ExtremeRoleId.Captain,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Captain.ToString(),
            ColorPalette.CarpenterBrown,
            false, true, false, false)
        { }

        public void ButtonMod(PlayerVoteArea instance, UiElement abilityButton)
        {
            throw new NotImplementedException();
        }

        public Action CreateAbilityAction(PlayerVoteArea instance)
        {
            throw new NotImplementedException();
        }

        public string GetFakeOptionString() => "";

        public bool IsBlockMeetingButtonAbility(PlayerVoteArea instance) => !this.IsAwake && chargedVote < 1.0f;

        public void SetSprite(SpriteRenderer render)
        {
            throw new NotImplementedException();
        }

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awakeRole)
            {
                if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
                {
                    this.awakeRole = true;
                    this.HasOtherVison = this.awakeHasOtherVision;
                }
            }
        }

        public override string GetColoredRoleName(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetColoredRoleName();
            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    Translation.GetString(RoleTypes.Crewmate.ToString()));
            }
        }
        public override string GetFullDescription()
        {
            if (IsAwake)
            {
                return Translation.GetString(
                    $"{this.Id}FullDescription");
            }
            else
            {
                return Translation.GetString(
                    $"{RoleTypes.Crewmate}FullDescription");
            }
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (IsAwake)
            {
                return base.GetImportantText(isContainFakeTask);

            }
            else
            {
                return Design.ColoedString(
                    Palette.White,
                    $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
            }
        }

        public override string GetIntroDescription()
        {
            if (IsAwake)
            {
                return base.GetIntroDescription();
            }
            else
            {
                return Design.ColoedString(
                    Palette.CrewmateBlue,
                    CachedPlayerControl.LocalPlayer.Data.Role.Blurb);
            }
        }

        public override Color GetNameColor(bool isTruthColor = false)
        {
            if (isTruthColor || IsAwake)
            {
                return base.GetNameColor(isTruthColor);
            }
            else
            {
                return Palette.White;
            }
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            CreateIntOption(
                CaptainOption.AwakeTaskGage,
                70, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
               GetRoleOptionId(CaptainOption.AwakeTaskGage)].GetValue() / 100.0f;

            this.awakeHasOtherVision = this.HasOtherVison;

            if (this.awakeTaskGage <= 0.0f)
            {
                this.awakeRole = true;
                this.HasOtherVison = this.awakeHasOtherVision;
            }
            else
            {
                this.awakeRole = false;
                this.HasOtherVison = false;
            }
        }
    }
}
