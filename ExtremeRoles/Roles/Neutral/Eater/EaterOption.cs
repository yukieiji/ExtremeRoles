using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Eater
{
    public readonly record struct EaterSpecificOption(
        bool CanUseVent,
        float EatRange,
        int DeadBodyEatActiveCoolTimePenalty,
        int KillEatCoolTimePenalty,
        int KillEatActiveCoolTimeReduceRate,
        bool IsResetCoolTimeWhenMeeting,
        bool IsShowArrowForDeadBody,
        int AbilityUseCount,
        float AbilityActiveTime,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class EaterOptionLoader : ISpecificOptionLoader<EaterSpecificOption>
    {
        public EaterSpecificOption Load(IOptionLoader loader)
        {
            return new EaterSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.CanUseVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.EatRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.DeadBodyEatActiveCoolTimePenalty),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatCoolTimePenalty),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatActiveCoolTimeReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsResetCoolTimeWhenMeeting),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsShowArrowForDeadBody),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class EaterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.CanUseVent,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 7, 7.5f);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.EatRange,
                1.0f, 0.0f, 2.0f, 0.1f);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.DeadBodyEatActiveCoolTimePenalty,
                10, 0, 25, 1,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatCoolTimePenalty,
                10, 0, 25, 1,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatActiveCoolTimeReduceRate,
                10, 0, 50, 1,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsResetCoolTimeWhenMeeting,
                false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsShowArrowForDeadBody,
                true);
        }
    }
}
