using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Bakary.BakaryRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Bakary
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
                loader.GetValue<BakaryOption, bool>(
                    BakaryOption.ChangeCooking),
                loader.GetValue<BakaryOption, float>(
                    BakaryOption.GoodBakeTime),
                loader.GetValue<BakaryOption, float>(
                    BakaryOption.BadBakeTime)
            );
        }
    }

    public class BakaryOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            var changeCooking = factory.CreateBoolOption(
                BakaryOption.ChangeCooking,
                true);

            factory.CreateFloatOption(
                BakaryOption.GoodBakeTime,
                60.0f, 45.0f, 75.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true);
            factory.CreateFloatOption(
                BakaryOption.BadBakeTime,
                120.0f, 105.0f, 135.0f, 0.5f,
                changeCooking, format: OptionUnit.Second,
                invert: true);
        }
    }
}
