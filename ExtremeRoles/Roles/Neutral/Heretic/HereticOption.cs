using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Heretic
{
    public readonly record struct HereticSpecificOption(
        bool HasTask,
        int SeeImpostorTaskGage,
        int KillMode,
        bool CanKillImpostor,
        float Range,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class HereticOptionLoader : ISpecificOptionLoader<HereticSpecificOption>
    {
        public HereticSpecificOption Load(IOptionLoader loader)
        {
            return new HereticSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.HasTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.SeeImpostorTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.KillMode),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.CanKillImpostor),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Heretic.Option, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Heretic.Option.Range),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
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
