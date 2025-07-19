using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;

namespace ExtremeRoles.Roles.Crewmate.Agency
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.CanSeeTaskBar),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.MaxTaskNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.TakeTaskRange),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class AgencyOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.CanSeeTaskBar,
                true);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.MaxTaskNum,
                2, 1, 3, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Agency.AgencyOption.TakeTaskRange,
                1.0f, 0.5f, 2.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 5);
        }
    }
}
