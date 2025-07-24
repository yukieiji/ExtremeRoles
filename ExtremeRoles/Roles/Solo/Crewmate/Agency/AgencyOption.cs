using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Solo.Crewmate.Agency
{
    public readonly record struct AgencySpecificOption(
        bool CanSeeTaskBar,
        int MaxTaskNum,
        float TakeTaskRange,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class AgencyOptionLoader : ISpecificOptionLoader<AgencySpecificOption>
    {
        public AgencySpecificOption Load(IOptionLoader loader)
        {
            return new AgencySpecificOption(
                loader.GetValue<AgencyRole.AgencyOption, bool>(
                    AgencyRole.AgencyOption.CanSeeTaskBar),
                loader.GetValue<AgencyRole.AgencyOption, int>(
                    AgencyRole.AgencyOption.MaxTaskNum),
                loader.GetValue<AgencyRole.AgencyOption, float>(
                    AgencyRole.AgencyOption.TakeTaskRange),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class AgencyOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                AgencyRole.AgencyOption.CanSeeTaskBar,
                true);
            factory.CreateIntOption(
                AgencyRole.AgencyOption.MaxTaskNum,
                2, 1, 3, 1);
            factory.CreateFloatOption(
                AgencyRole.AgencyOption.TakeTaskRange,
                1.0f, 0.5f, 2.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);
        }
    }
}
