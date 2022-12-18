using System;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Roles.Solo
{
    public sealed class VanillaRoleWrapper : SingleRoleBase
    {
        public RoleTypes VanilaRoleId;

        public VanillaRoleWrapper(
            RoleTypes id) : base()
        {
            this.VanilaRoleId = id;
            this.Id = ExtremeRoleId.VanillaRole;
            this.RawRoleName = this.VanilaRoleId.ToString();

            var curOption = GameOptionsManager.Instance.CurrentGameOptions;

            switch (id)
            {
                case RoleTypes.Shapeshifter:
                case RoleTypes.Impostor:
                    this.Team = ExtremeRoleType.Impostor;
                    this.NameColor = Palette.ImpostorRed;
                    this.CanKill = true;
                    this.UseVent = curOption.GameMode == GameModes.Normal;
                    this.UseSabotage = curOption.GameMode == GameModes.Normal;
                    this.HasTask = false;
                    this.KillCoolTime = curOption.GetFloat(
                        FloatOptionNames.KillCooldown);
                    this.KillRange = curOption.GetInt(
                        Int32OptionNames.KillDistance);
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

        public override string GetFullDescription()
        {
            return Helper.Translation.GetString(
                $"{this.VanilaRoleId}FullDescription");
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if(this.IsImpostor())
            {
                return string.Concat(new string[]
                {
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                    "\r\n",
                    Palette.ImpostorRed.ToTextColor(),
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()),
                    "</color>"
                });
            }

            return Helper.Design.ColoedString(
                this.NameColor,
                $"{this.GetColoredRoleName()}: {Helper.Translation.GetString("crewImportantText")}");

        }
        protected override void CommonInit()
        {
            return;
        }
        protected override void RoleSpecificInit()
        {
            return;
        }

        protected override void CreateSpecificOption(IOption parentOps)
        {
            throw new System.Exception("Don't call this class method!!");
        }
    }
}
