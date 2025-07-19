using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Madmate
{
    public readonly record struct MadmateSpecificOption(
        bool IsDontCountAliveCrew,
        bool CanFixSabotage,
        bool CanUseVent,
        bool CanMoveVentToVent,
        bool HasTask,
        int SeeImpostorTaskGage,
        bool CanSeeFromImpostor,
        int CanSeeFromImpostorTaskGage,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class MadmateOptionLoader : ISpecificOptionLoader<MadmateSpecificOption>
    {
        public MadmateSpecificOption Load(IOptionLoader loader)
        {
            return new MadmateSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.IsDontCountAliveCrew),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanFixSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanUseVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanMoveVentToVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.HasTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.SeeImpostorTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostor),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostorTaskGage),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class MadmateOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.IsDontCountAliveCrew,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanFixSabotage,
                false);
            var ventUseOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanUseVent,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanMoveVentToVent,
                false, ventUseOpt);
            var taskOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.HasTask,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.SeeImpostorTaskGage,
                70, 0, 100, 10,
                taskOpt,
                format: OptionUnit.Percentage);
            var impFromSeeOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostor,
                false, taskOpt);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostorTaskGage,
                70, 0, 100, 10,
                impFromSeeOpt,
                format: OptionUnit.Percentage);

            IRoleAbility.CreateCommonAbilityOption(factory);
        }
    }
}
