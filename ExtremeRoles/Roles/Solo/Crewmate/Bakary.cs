using ExtremeRoles.Module;
using ExtremeRoles.Roles.API;

namespace ExtremeRoles.Roles.Solo.Crewmate
{
    public sealed class Bakary : SingleRoleBase
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
            IOption parentOps)
        {
            var changeCooking = CreateBoolOption(
                BakaryOption.ChangeCooking,
                true, parentOps);

            CreateFloatOption(
                BakaryOption.GoodBakeTime,
                60.0f, 45.0f, 75.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true, enableCheckOption: parentOps);
            CreateFloatOption(
                BakaryOption.BadBakeTime,
                120.0f, 105.0f, 135.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true, enableCheckOption: parentOps);
        }

        protected override void RoleSpecificInit()
        {
            ExtremeRolesPlugin.GameDataStore.AddGlobalActionRole(this);
        }
    }
}
