using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Bakary
{
    public readonly record struct BakarySpecificOption(
        bool ChangeCooking,
        float GoodBakeTime,
        float BadBakeTime
    ) : IRoleSpecificOption;

    public class BakaryOptionLoader : ISpecificOptionLoader<BakarySpecificOption>
    {
        public BakarySpecificOption Load(IOptionLoader loader)
        {
            return new BakarySpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.ChangeCooking),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.GoodBakeTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.BadBakeTime)
            );
        }
    }

    public class BakaryOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            var changeCooking = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.ChangeCooking,
                true);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.GoodBakeTime,
                60.0f, 45.0f, 75.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryOption.BadBakeTime,
                120.0f, 105.0f, 135.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true);
        }
    }
}
