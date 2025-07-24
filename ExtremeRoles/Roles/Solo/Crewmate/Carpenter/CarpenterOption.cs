using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Carpenter
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
                loader.GetValue<CarpenterOption, int>(
                    CarpenterOption.AwakeTaskGage),
                loader.GetValue<CarpenterOption, int>(
                    CarpenterOption.RemoveVentScrew),
                loader.GetValue<CarpenterOption, float>(
                    CarpenterOption.RemoveVentStopTime),
                loader.GetValue<CarpenterOption, int>(
                    CarpenterOption.SetCameraScrew),
                loader.GetValue<CarpenterOption, float>(
                    CarpenterOption.SetCameraStopTime),
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
                CarpenterOption.AwakeTaskGage,
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
                CarpenterOption.RemoveVentScrew,
                10, 1, 20, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                CarpenterOption.RemoveVentStopTime,
                5.0f, 2.0f, 15.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                CarpenterOption.SetCameraScrew,
                5, 1, 10, 1,
                format: OptionUnit.ScrewNum);
            factory.CreateFloatOption(
                CarpenterOption.SetCameraStopTime,
                2.5f, 1.0f, 5.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
