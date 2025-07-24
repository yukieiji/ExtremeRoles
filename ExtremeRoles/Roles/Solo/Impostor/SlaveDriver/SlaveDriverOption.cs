using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using static ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverRole;

namespace ExtremeRoles.Roles.Solo.Impostor.SlaveDriver
{
    public readonly record struct SlaveDriverSpecificOption(
        bool CanSeeTaskBar,
        int RevartTaskNum,
        float Range,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class SlaveDriverOptionLoader : ISpecificOptionLoader<SlaveDriverSpecificOption>
    {
        public SlaveDriverSpecificOption Load(IOptionLoader loader)
        {
            return new SlaveDriverSpecificOption(
                loader.GetValue<SlaveDriverOption, bool>(
                    SlaveDriverOption.CanSeeTaskBar),
                loader.GetValue<SlaveDriverOption, int>(
                    SlaveDriverOption.RevartTaskNum),
                loader.GetValue<SlaveDriverOption, float>(
                    SlaveDriverOption.Range),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class SlaveDriverOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                SlaveDriverOption.CanSeeTaskBar,
                true);
            IRoleAbility.CreateAbilityCountOption(factory, 2, 10);
            factory.CreateIntOption(
                SlaveDriverOption.RevartTaskNum,
                2, 1, 5, 1);
            factory.CreateFloatOption(
                SlaveDriverOption.Range,
                0.75f, 0.25f, 3.5f, 0.25f);
        }
    }
}
