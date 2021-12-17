using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Solo
{
    public class VanillaRoleWrapper : SingleRoleAbs
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
                    this.Teams = ExtremeRoleType.Impostor;
                    this.NameColor = Palette.ImpostorRed;
                    this.UseVent = true;
                    this.UseSabotage = true;
                    this.HasTask = false;
                    break;
                case RoleTypes.Engineer:
                    this.Teams = ExtremeRoleType.Crewmate;
                    this.UseVent = true;
                    this.NameColor = Palette.White;
                    break;
                case RoleTypes.Crewmate:
                case RoleTypes.Scientist:
                    this.Teams = ExtremeRoleType.Crewmate;
                    this.NameColor = Palette.White;
                    break;
                default:
                    break;
            };
        }

        protected override void CommonInit()
        {
            return;
        }
        protected override void RoleSpecificInit()
        {
            return;
        }

        protected override void CreateSpecificOption(CustomOption parentOps)
        {
            throw new System.Exception("Don't call this class method!!");
        }
        protected override CustomOption CreateSpawnOption()
        {
            throw new System.Exception("Don't call this class method!!");
        }
    }
}
