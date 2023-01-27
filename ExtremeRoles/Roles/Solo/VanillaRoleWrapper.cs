using System;
using AmongUs.GameOptions;

using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Performance;
using ExtremeRoles.Helper;

namespace ExtremeRoles.Roles.Solo
{
    public sealed class VanillaRoleWrapper : MultiAssignRoleBase
    {
        public RoleTypes VanilaRoleId;

        private VanillaRoleWrapper(RoleTypes id, bool isImpostor) : base(
            ExtremeRoleId.VanillaRole,
            isImpostor ? ExtremeRoleType.Impostor : ExtremeRoleType.Crewmate,
            id.ToString(),
            isImpostor ? Palette.ImpostorRed : Palette.White,
            isImpostor, !isImpostor,
            isImpostor, isImpostor)
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
        }


        public VanillaRoleWrapper(
            RoleTypes id) : 
            this(id, id == RoleTypes.Impostor || id == RoleTypes.Shapeshifter)
        { }

        public override string GetFullDescription()
        {
            return Translation.GetString(
                $"{this.VanilaRoleId}FullDescription");
        }

        public override string GetImportantText(bool isContainFakeTask = true)
        {

            if (this.AnotherRole == null)
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
                $"{this.GetColoredRoleName()}: {Translation.GetString("crewImportantText")}");
        }
    }
}
