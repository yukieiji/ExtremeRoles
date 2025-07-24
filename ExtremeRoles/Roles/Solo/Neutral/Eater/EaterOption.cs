using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Eater.EaterRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Eater
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
                loader.GetValue<EaterOption, bool>(
                    EaterOption.CanUseVent),
                loader.GetValue<EaterOption, float>(
                    EaterOption.EatRange),
                loader.GetValue<EaterOption, int>(
                    EaterOption.DeadBodyEatActiveCoolTimePenalty),
                loader.GetValue<EaterOption, int>(
                    EaterOption.KillEatCoolTimePenalty),
                loader.GetValue<EaterOption, int>(
                    EaterOption.KillEatActiveCoolTimeReduceRate),
                loader.GetValue<EaterOption, bool>(
                    EaterOption.IsResetCoolTimeWhenMeeting),
                loader.GetValue<EaterOption, bool>(
                    EaterOption.IsShowArrowForDeadBody),
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
                EaterOption.CanUseVent,
                true);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 7, 7.5f);

            factory.CreateFloatOption(
                EaterOption.EatRange,
                1.0f, 0.0f, 2.0f, 0.1f);
            factory.CreateIntOption(
                EaterOption.DeadBodyEatActiveCoolTimePenalty,
                10, 0, 25, 1,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                EaterOption.KillEatCoolTimePenalty,
                10, 0, 25, 1,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                EaterOption.KillEatActiveCoolTimeReduceRate,
                10, 0, 50, 1,
                format: OptionUnit.Percentage);
            factory.CreateBoolOption(
                EaterOption.IsResetCoolTimeWhenMeeting,
                false);
            factory.CreateBoolOption(
                EaterOption.IsShowArrowForDeadBody,
                true);
        }
    }
}
