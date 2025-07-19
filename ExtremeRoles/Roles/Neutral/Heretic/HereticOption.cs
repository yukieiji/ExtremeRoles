using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Heretic
{
    public class HereticSpecificOption : IRoleSpecificOption
    {
        public bool HasTask { get; set; }
        public int SeeImpostorTaskGage { get; set; }
        public int KillMode { get; set; }
        public bool CanKillImpostor { get; set; }
        public float Range { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class HereticOptionLoader : ISpecificOptionLoader<HereticSpecificOption>
    {
        public HereticSpecificOption Load(IOptionLoader loader)
        {
            return new HereticSpecificOption
            {
                HasTask = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.HasTask),
                SeeImpostorTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.SeeImpostorTaskGage),
                KillMode = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.KillMode),
                CanKillImpostor = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.CanKillImpostor),
                Range = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.Range),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
        }
    }

    public class HereticOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            var taskOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.HasTask,
                false);
            factory.Create0To100Percentage10StepOption(
                ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.SeeImpostorTaskGage, taskOpt);
            var killModeOpt = factory.CreateSelectionOption(
                ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.KillMode,
                new[]
                {
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.KillMode.AbilityOnTaskPhase,
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.KillMode.AbilityOnTaskPhaseTarget
                });
            factory.CreateFloatOption(
                RoleAbilityCommonOption.AbilityCoolTime,
                IRoleAbility.DefaultCoolTime,
                IRoleAbility.MinCoolTime,
                IRoleAbility.MaxCoolTime,
                IRoleAbility.Step,
                killModeOpt,
                invert: true,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.Range,
                1.2f, 0.1f, 2.5f, 0.1f,
                killModeOpt,
                invert: true);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.CanKillImpostor,
                false);
        }
    }
}
