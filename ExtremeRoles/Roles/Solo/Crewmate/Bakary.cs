using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public class Bakary : SingleRoleBase
    {
        public enum BakaryOption
        {
            ChangeCooking,
            GoodBakeTime,
            BadBakeTime,
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
            var changeCooking = CustomOption.Create(
                GetRoleOptionId(BakaryOption.ChangeCooking),
                string.Concat(
                    this.RoleName,
                    BakaryOption.ChangeCooking.ToString()),
                true, parentOps);
            CustomOption.Create(
                GetRoleOptionId(BakaryOption.GoodBakeTime),
                string.Concat(
                    this.RoleName,
                    BakaryOption.GoodBakeTime.ToString()),
                60.0f, 45.0f, 75.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true, enableCheckOption: parentOps);
            CustomOption.Create(
                GetRoleOptionId(BakaryOption.BadBakeTime),
                string.Concat(
                    this.RoleName,
                    BakaryOption.BadBakeTime.ToString()),
                120.0f, 105.0f, 135.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true, enableCheckOption: parentOps);
        }

        protected override void RoleSpecificInit()
        {
            ExtremeRolesPlugin.GameDataStore.Union.SetCookingCondition(
                OptionHolder.AllOption[
                    GetRoleOptionId(BakaryOption.GoodBakeTime)].GetValue(),
                OptionHolder.AllOption[
                    GetRoleOptionId(BakaryOption.BadBakeTime)].GetValue(),
                OptionHolder.AllOption[
                    GetRoleOptionId(BakaryOption.ChangeCooking)].GetValue());
        }
    }
}
