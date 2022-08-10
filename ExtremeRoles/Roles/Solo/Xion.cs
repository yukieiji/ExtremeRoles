using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo
{
    public sealed class Xion : SingleRoleBase
    {
        public Xion() : base(
            ExtremeRoleId.Xion,
            ExtremeRoleType.Null,
            ExtremeRoleId.Xion.ToString(),
            ColorPalette.YokoShion,
            false, false, false, false,
            false, false, false, false, false)
        { }

        protected override void CommonInit()
        {
            return;
        }

        protected override void CreateSpecificOption(
            IOption parentOps)
        { }

        protected override void RoleSpecificInit()
        {
            return;
        }
    }
}
