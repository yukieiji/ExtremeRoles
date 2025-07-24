using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Madmate
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
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.IsDontCountAliveCrew),
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.CanFixSabotage),
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.CanUseVent),
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.CanMoveVentToVent),
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.HasTask),
                loader.GetValue<MadmateOption, int>(
                    MadmateOption.SeeImpostorTaskGage),
                loader.GetValue<MadmateOption, bool>(
                    MadmateOption.CanSeeFromImpostor),
                loader.GetValue<MadmateOption, int>(
                    MadmateOption.CanSeeFromImpostorTaskGage),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class MadmateOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                MadmateOption.IsDontCountAliveCrew,
                false);
            factory.CreateBoolOption(
                MadmateOption.CanFixSabotage,
                false);
            var ventUseOpt = factory.CreateBoolOption(
                MadmateOption.CanUseVent,
                false);
            factory.CreateBoolOption(
                MadmateOption.CanMoveVentToVent,
                false, ventUseOpt);
            var taskOpt = factory.CreateBoolOption(
                MadmateOption.HasTask,
                false);
            factory.CreateIntOption(
                MadmateOption.SeeImpostorTaskGage,
                70, 0, 100, 10,
                taskOpt,
                format: OptionUnit.Percentage);
            var impFromSeeOpt = factory.CreateBoolOption(
                MadmateOption.CanSeeFromImpostor,
                false, taskOpt);
            factory.CreateIntOption(
                MadmateOption.CanSeeFromImpostorTaskGage,
                70, 0, 100, 10,
                impFromSeeOpt,
                format: OptionUnit.Percentage);

            IRoleAbility.CreateCommonAbilityOption(factory);
        }
    }
}
