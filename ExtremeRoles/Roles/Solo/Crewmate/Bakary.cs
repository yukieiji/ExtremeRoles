using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Bakary : SingleRoleBase
    {
        public enum BakaryOption
        {
            ChangeCooking
        }

        public Bakary() : base(
            ExtremeRoleId.Bakary,
            ExtremeRoleType.Crewmate,
            ExtremeRoleId.Bakary.ToString(),
            ColorPalette.BakaryWheatColor,
            false, true, false, false)
        { }

        protected override void CreateSpecificOption(
            CustomOptionBase parentOps)
        {
            CustomOption.Create(
                GetRoleOptionId((int)BakaryOption.ChangeCooking),
                string.Concat(
                    this.RoleName,
                    BakaryOption.ChangeCooking.ToString()),
                true, parentOps);
        }

        protected override void RoleSpecificInit()
        {
            ExtremeRolesPlugin.GameDataStore.Union.SetCooking(OptionHolder.AllOption[
                GetRoleOptionId((int)BakaryOption.ChangeCooking)].GetValue());
        }
    }
}
