using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Madmate
{
    public class MadmateSpecificOption : IRoleSpecificOption
    {
        public bool IsDontCountAliveCrew { get; set; }
        public bool CanFixSabotage { get; set; }
        public bool CanUseVent { get; set; }
        public bool CanMoveVentToVent { get; set; }
        public bool HasTask { get; set; }
        public int SeeImpostorTaskGage { get; set; }
        public bool CanSeeFromImpostor { get; set; }
        public int CanSeeFromImpostorTaskGage { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class MadmateOptionLoader : ISpecificOptionLoader<MadmateSpecificOption>
    {
        public MadmateSpecificOption Load(IOptionLoader loader)
        {
            return new MadmateSpecificOption
            {
                IsDontCountAliveCrew = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.IsDontCountAliveCrew),
                CanFixSabotage = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanFixSabotage),
                CanUseVent = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanUseVent),
                CanMoveVentToVent = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanMoveVentToVent),
                HasTask = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.HasTask),
                SeeImpostorTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.SeeImpostorTaskGage),
                CanSeeFromImpostor = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostor),
                CanSeeFromImpostorTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Madmate.MadmateOption.CanSeeFromImpostorTaskGage),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
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
