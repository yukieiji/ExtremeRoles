using ExtremeRoles.Module;

namespace ExtremeRoles.Roles.Combination
{
    public class Lover : MultiAssignRoleAbs
    {

        public enum LoverSetting 
        {
            LoverNum,
            IsAssignImposter,
            IsAssignNeutral,
        }

        public Lover() : base(
            ExtremeRoleId.Lover,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Lover.ToString(),
            ColorPalette.LoverPink,
            false,
            true,
            false,
            false)
        {}

        protected override void CreateSpecificOption(CustomOption parentOps)
        {
            throw new System.NotImplementedException();
        }

        protected override void RoleSpecificInit()
        {
            throw new System.NotImplementedException();
        }
    }
}
