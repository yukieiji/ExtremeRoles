using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.SlaveDriver
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.CanSeeTaskBar),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.RevartTaskNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.Range),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class SlaveDriverOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.CanSeeTaskBar,
                true);
            IRoleAbility.CreateAbilityCountOption(factory, 2, 10);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.RevartTaskNum,
                2, 1, 5, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.Range,
                0.75f, 0.25f, 3.5f, 0.25f);
        }
    }
}
