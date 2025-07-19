using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Eater
{
    public class EaterSpecificOption : IRoleSpecificOption
    {
        public bool CanUseVent { get; set; }
        public float EatRange { get; set; }
        public int DeadBodyEatActiveCoolTimePenalty { get; set; }
        public int KillEatCoolTimePenalty { get; set; }
        public int KillEatActiveCoolTimeReduceRate { get; set; }
        public bool IsResetCoolTimeWhenMeeting { get; set; }
        public bool IsShowArrowForDeadBody { get; set; }
        public int AbilityUseCount { get; set; }
        public float AbilityActiveTime { get; set; }
        public float AbilityCoolTime { get; set; }
    }

    public class EaterOptionLoader : ISpecificOptionLoader<EaterSpecificOption>
    {
        public EaterSpecificOption Load(IOptionLoader loader)
        {
            return new EaterSpecificOption
            {
                CanUseVent = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.CanUseVent),
                EatRange = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, float>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.EatRange),
                DeadBodyEatActiveCoolTimePenalty = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.DeadBodyEatActiveCoolTimePenalty),
                KillEatCoolTimePenalty = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatCoolTimePenalty),
                KillEatActiveCoolTimeReduceRate = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.KillEatActiveCoolTimeReduceRate),
                IsResetCoolTimeWhenMeeting = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsResetCoolTimeWhenMeeting),
                IsShowArrowForDeadBody = loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Eater.EaterOption.IsShowArrowForDeadBody),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                AbilityActiveTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityActiveTime),
                AbilityCoolTime = loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            };
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
