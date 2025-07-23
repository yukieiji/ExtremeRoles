using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Carpenter
{
    public readonly record struct CarpenterSpecificOption(
        int AwakeTaskGage,
        int RemoveVentScrew,
        float RemoveVentStopTime,
        int SetCameraScrew,
        float SetCameraStopTime,
        float AbilityCoolTime,
        int AbilityCount
    ) : IRoleSpecificOption;

    public class CarpenterOptionLoader : ISpecificOptionLoader<CarpenterSpecificOption>
    {
        public CarpenterSpecificOption Load(IOptionLoader loader)
        {
            return new CarpenterSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentScrew),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentStopTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraScrew),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraStopTime),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime),
                loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount)
            );
        }
    }

    public class CarpenterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);
            createAbilityOption(factory);
        }

        private void createAbilityOption(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                15.0f, 2.0f, 60.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                RoleAbilityCommonOption.AbilityCount,
                15, 5, 100, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentScrew,
                10, 1, 20, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentStopTime,
                5.0f, 2.0f, 15.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraScrew,
                5, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraStopTime,
                2.5f, 1.0f, 5.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
