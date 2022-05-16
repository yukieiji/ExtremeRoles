using UnityEngine;



using ExtremeRoles.Module;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class SecurityGuard : SingleRoleBase, IRoleAwake<RoleTypes>
    {
        public bool IsAwake
        {
            get
            {
                return GameSystem.IsLobby || this.awakeRole;
            }
        }

        public RoleTypes NoneAwakeRole => RoleTypes.Crewmate;

        public enum SecurityGuardOption
        {
            AwakeTaskGage
        }

        private bool awakeRole = false;
        private float awakeTaskGage;
        public SecurityGuard() : base(
            ExtremeRoleId.SecurityGuard,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.SecurityGuard.ToString(),
            Palette.CrewmateBlue,
            false, true, false, false)
        { }

        public string GetFakeOptionString() => "";

        public void Update(PlayerControl rolePlayer)
        {
            if (!this.awakeRole)
            {
                if (Player.GetPlayerTaskGage(rolePlayer) >= this.awakeTaskGage)
                {
                    this.awakeRole = true;
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
                    Palette.White, Translation.GetString(RoleTypes.Crewmate.ToString()));
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
                return PlayerControl.LocalPlayer.Data.Role.Blurb;
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

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CreateIntOption(
                SecurityGuardOption.AwakeTaskGage,
                100, 0, 100, 10,
                parentOps,
                format: OptionUnit.Percentage);
        }

        protected override void RoleSpecificInit()
        {
            this.awakeTaskGage = (float)OptionHolder.AllOption[
                GetRoleOptionId(SecurityGuardOption.AwakeTaskGage)].GetValue() / 100.0f;
            if (this.awakeTaskGage <= 0.0f)
            {
                this.awakeRole = true;
            }
            else
            {
                this.awakeRole = false;
            }
        }

    }
}
