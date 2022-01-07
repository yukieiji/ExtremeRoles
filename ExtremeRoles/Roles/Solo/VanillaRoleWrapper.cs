using System;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo
{
    public class VanillaRoleWrapper : SingleRoleBase
    {
        public RoleTypes VanilaRoleId;
        public VanillaRoleWrapper(
            RoleTypes id) : base()
        {
            this.VanilaRoleId = id;
            this.Id = ExtremeRoleId.VanillaRole;
            this.BytedRoleId = (byte)ExtremeRoleId.VanillaRole;
            this.RoleName = id.ToString();

            switch (id)
            {
                case RoleTypes.Shapeshifter:
                case RoleTypes.Impostor:
                    this.Team = ExtremeRoleType.Impostor;
                    this.NameColor = Palette.ImpostorRed;
                    this.CanKill = true;
                    this.UseVent = true;
                    this.UseSabotage = true;
                    this.HasTask = false;
                    break;
                case RoleTypes.Engineer:
                    this.Team = ExtremeRoleType.Crewmate;
                    this.UseVent = true;
                    this.NameColor = Palette.White;
                    break;
                case RoleTypes.Crewmate:
                case RoleTypes.Scientist:
                    this.Team = ExtremeRoleType.Crewmate;
                    this.NameColor = Palette.White;
                    break;
                default:
                    break;
            };
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if(this.IsImposter())
            {
                return string.Concat(new string[]
                {
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                    "\r\n",
                    Palette.ImpostorRed.ToTextColor(),
                    DestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                    "</color>"
                });
            }

            return Helper.Design.ColoedString(
                this.NameColor,
                string.Format("{0}: {1}",
                    this.GetColoredRoleName(),
                    Helper.Translation.GetString("crewImportantText")));

        }

        protected override void CommonInit()
        {
            return;
        }
        protected override void RoleSpecificInit()
        {
            return;
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            throw new System.Exception("Don't call this class method!!");
        }
        protected override CustomOptionBase CreateSpawnOption()
        {
            throw new System.Exception("Don't call this class method!!");
        }
    }
}
