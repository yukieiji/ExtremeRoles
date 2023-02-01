using System;
using AmongUs.GameOptions;

using ExtremeRoles.GameMode;
using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo
{
    public sealed class VanillaRoleWrapper : MultiAssignRoleBase
    {
        public RoleTypes VanilaRoleId;

        private VanillaRoleWrapper(RoleTypes id, bool isImpostor) : base(
            id: ExtremeRoleId.VanillaRole,
            team: isImpostor ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate,
            roleName: id.ToString(),
            roleColor: isImpostor ? Palette.ImpostorRed : Palette.White,
            canKill: isImpostor,
            hasTask: !isImpostor,
            useVent: isImpostor,
            useSabotage: isImpostor)
        {
            this.VanilaRoleId = id;

            var curOption = GameOptionsManager.Instance.CurrentGameOptions;
            if (this.CanKill)
            {
                this.KillCoolTime = curOption.GetFloat(FloatOptionNames.KillCooldown);
                this.KillRange = curOption.GetInt(Int32OptionNames.KillDistance);
            }
            switch (id)
            {
                case RoleTypes.Engineer:
                    this.UseVent = true;
                    break;
                default:
                    break;
            }
            this.CanHasAnotherRole = ExtremeGameModeManager.Instance.RoleSelector.IsVanillaRoleToMultiAssign;
        }

        public VanillaRoleWrapper(
            RoleTypes id) : 
            this(id, id == RoleTypes.Impostor || id == RoleTypes.Shapeshifter)
        { }

        public override void OverrideAnotherRoleSetting()
        {
            if (this.AnotherRole is VanillaRoleWrapper vanillaRole &&
                vanillaRole.VanilaRoleId == this.VanilaRoleId)
            {
                this.AnotherRole = null;
                this.CanHasAnotherRole = false;
            }
            else
            {
                this.CanCallMeeting = this.AnotherRole.CanCallMeeting;
                this.CanUseAdmin    = this.AnotherRole.CanUseAdmin   ;
                this.CanUseSecurity = this.AnotherRole.CanUseSecurity;
                this.CanUseVital    = this.AnotherRole.CanUseVital;
            }
        }

        public override string GetFullDescription()
        {
            return Translation.GetString(
                $"{this.VanilaRoleId}FullDescription");
        }

        public override string GetIntroDescription()
        {
            string baseIntro = Design.ColoedString(
                this.IsImpostor() ? Palette.ImpostorRed : Palette.CrewmateBlue,
                CachedPlayerControl.LocalPlayer.Data.Role.Blurb);

            if (this.AnotherRole == null ||
                (this.AnotherRole is IRoleAwake<RoleTypes> awakeRole &&
                 !awakeRole.IsAwake))
            {
                return baseIntro;
            }

            string anotherIntro;

            if (this.AnotherRole.IsVanillaRole())
            {
                RoleBehaviour role = CachedPlayerControl.LocalPlayer.Data.Role;
                anotherIntro = role.Blurb;
            }
            else
            {
                anotherIntro = this.AnotherRole.GetIntroDescription();
            }

            string concat = Design.ColoedString(
                Palette.White,
                string.Concat(
                    "\n ", Translation.GetString("introAnd")));

            return string.Concat(baseIntro, concat, Design.ColoedString(
                this.AnotherRole.GetNameColor(),
                anotherIntro));

        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {
            if (this.AnotherRole == null ||
                (this.AnotherRole is IRoleAwake<RoleTypes> awakeRole &&
                 !awakeRole.IsAwake))
            {
                return getVanilaImportantText();
            }

            string baseString = getVanilaImportantText(false);
            string anotherRoleString = this.AnotherRole.GetImportantText(false);

            baseString = $"{baseString}\r\n{anotherRoleString}";

            if (isContainFakeTask && (!this.HasTask || !this.AnotherRole.HasTask))
            {
                string fakeTaskString = Design.ColoedString(
                    this.NameColor,
                    FastDestroyableSingleton<TranslationController>.Instance.GetString(
                        StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()));
                baseString = $"{baseString}\r\n{fakeTaskString}";
            }

            return baseString;

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

        private string getVanilaImportantText(bool isContainFakeTask = true)
        {
            if (this.IsImpostor())
            {
                var trans = FastDestroyableSingleton<TranslationController>.Instance;

                return string.Concat(new string[]
                {
                    trans.GetString(StringNames.ImpostorTask, Array.Empty<Il2CppSystem.Object>()),
                    "\r\n",
                    Palette.ImpostorRed.ToTextColor(),
                    isContainFakeTask ? 
                        trans.GetString(StringNames.FakeTasks, Array.Empty<Il2CppSystem.Object>()) : 
                        string.Empty,
                    "</color>"
                });
            }

            return Design.ColoedString(
                this.NameColor,
                $"{Design.ColoedString(
                    this.NameColor, Translation.GetString(this.RoleName))}: {Translation.GetString("crewImportantText")}");
        }
    }
}
