using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Heretic.HereticRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Heretic
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
                loader.GetValue<Option, bool>(
                    Option.HasTask),
                loader.GetValue<Option, int>(
                    Option.SeeImpostorTaskGage),
                loader.GetValue<Option, int>(
                    Option.KillMode),
                loader.GetValue<Option, bool>(
                    Option.CanKillImpostor),
                loader.GetValue<Option, float>(
                    Option.Range),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class HereticOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            var taskOpt = factory.CreateBoolOption(
                Option.HasTask,
                false);
            factory.Create0To100Percentage10StepOption(
                Option.SeeImpostorTaskGage, taskOpt);
            var killModeOpt = factory.CreateSelectionOption(
                Option.KillMode,
                new[]
                {
                    KillMode.AbilityOnTaskPhase,
                    KillMode.AbilityOnTaskPhaseTarget
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
                Option.Range,
                1.2f, 0.1f, 2.5f, 0.1f,
                killModeOpt,
                invert: true);

            factory.CreateBoolOption(
                Option.CanKillImpostor,
                false);
        }
    }
}
