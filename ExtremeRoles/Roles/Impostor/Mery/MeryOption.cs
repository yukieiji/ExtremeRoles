using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Impostor.Mery
{
    public class MerySpecificOption : IRoleSpecificOption
    {
        public int ActiveNum { get; set; }
        public float ActiveRange { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class MeryOptionLoader : ISpecificOptionLoader<MerySpecificOption>
    {
        public MerySpecificOption Load(IOptionLoader loader)
        {
            return new MerySpecificOption
            {
                ActiveNum = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption.ActiveNum),
                ActiveRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption.ActiveRange),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
        }
    }

    public class MeryOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 5);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption.ActiveNum,
                3, 1, 5, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Mery.MeryOption.ActiveRange,
                2.0f, 0.1f, 3.0f, 0.1f);
        }
    }
}
