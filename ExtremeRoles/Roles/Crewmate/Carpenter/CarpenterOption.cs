using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Carpenter
{
    public class CarpenterSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public int RemoveVentScrew { get; set; }
        public float RemoveVentStopTime { get; set; }
        public int SetCameraScrew { get; set; }
        public float SetCameraStopTime { get; set; }
        public float AbilityCoolTime { get; set; }
        public int AbilityCount { get; set; }
    }

    public class CarpenterOptionLoader : ISpecificOptionLoader<CarpenterSpecificOption>
    {
        public CarpenterSpecificOption Load(IOptionLoader loader)
        {
            return new CarpenterSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.AwakeTaskGage),
                RemoveVentScrew = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentScrew),
                RemoveVentStopTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.RemoveVentStopTime),
                SetCameraScrew = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraScrew),
                SetCameraStopTime = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Carpenter.CarpenterOption.SetCameraStopTime),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime),
                AbilityCount = loader.GetValue<RoleAbilityCommonOption, int>(RoleAbilityCommonOption.AbilityCount)
            };
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
