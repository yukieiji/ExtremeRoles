using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.SlaveDriver
{
    public class SlaveDriverSpecificOption : IRoleSpecificOption
    {
        public bool CanSeeTaskBar { get; set; }
        public int RevartTaskNum { get; set; }
        public float Range { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class SlaveDriverOptionLoader : ISpecificOptionLoader<SlaveDriverSpecificOption>
    {
        public SlaveDriverSpecificOption Load(IOptionLoader loader)
        {
            return new SlaveDriverSpecificOption
            {
                CanSeeTaskBar = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.CanSeeTaskBar),
                RevartTaskNum = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.RevartTaskNum),
                Range = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.SlaveDriver.SlaveDriverOption.Range),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
