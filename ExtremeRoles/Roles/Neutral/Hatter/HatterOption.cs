using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Neutral.Hatter
{
    public readonly record struct HatterSpecificOption(
        bool CanRepairSabotage,
        int WinCount,
        int MeetingTimerDecreaseLower,
        int MeetingTimerDecreaseUpper,
        bool HideMeetingTimer,
        int IncreaseTaskGage,
        int IncreseNum,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class HatterOptionLoader : ISpecificOptionLoader<HatterSpecificOption>
    {
        public HatterSpecificOption Load(IOptionLoader loader)
        {
            return new HatterSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.CanRepairSabotage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.WinCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.MeetingTimerDecreaseLower),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.MeetingTimerDecreaseUpper),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, bool>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.HideMeetingTimer),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.IncreaseTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption, int>(
                    ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.IncreseNum),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class HatterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
               ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.CanRepairSabotage,
               false);

            factory.CreateIntOption(
               ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.WinCount,
               3, 1, 10, 1);

            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 10, minAbilityCount: 0);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.HideMeetingTimer, true);

            var lowerOpt = factory.CreateIntDynamicOption(
                ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.MeetingTimerDecreaseLower,
                0, 0, 5,
                format: OptionUnit.Percentage);

            var upperOpt = factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.MeetingTimerDecreaseUpper,
                20, 0, 50, 5,
                format: OptionUnit.Percentage);
            upperOpt.AddWithUpdate(lowerOpt);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.IncreaseTaskGage,
                50, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterOption.IncreseNum,
                3, 1, 10, 1);
        }
    }
}
