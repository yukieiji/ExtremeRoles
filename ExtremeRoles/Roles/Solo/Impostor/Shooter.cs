using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Impostor
{
    public class Shooter : SingleRoleBase
    {
        public int CurShootNum => this.curShootNum;

        private int curShootNum = 10;

        public Shooter(): base(
            ExtremeRoleId.Shooter,
            ExtremeRoleType.Impostor,
            ExtremeRoleId.Shooter.ToString(),
            Palette.ImpostorRed,
            true, false, true, true)
        {}

        public void ReduceShootNum()
        {
            this.curShootNum = this.curShootNum - 1;
        }

        protected override void CreateSpecificOption(CustomOptionBase parentOps)
        {
            return;
        }
        
        protected override void RoleSpecificInit()
        {
            return;
        }

    }
}
