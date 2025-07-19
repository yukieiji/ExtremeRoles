using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.TimeMaster
{
    public class TimeMasterSpecificOption : IRoleSpecificOption
    {
        public float RewindTime { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class TimeMasterOptionLoader : ISpecificOptionLoader<TimeMasterSpecificOption>
    {
        public TimeMasterSpecificOption Load(IOptionLoader loader)
        {
            return new TimeMasterSpecificOption
            {
                RewindTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.TimeMaster.TimeMasterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.TimeMaster.TimeMasterOption.RewindTime),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class TimeMasterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            IRoleAbility.CreateCommonAbilityOption(
                factory, 3.0f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.TimeMaster.TimeMasterOption.RewindTime,
                5.0f, 1.0f, 60.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
