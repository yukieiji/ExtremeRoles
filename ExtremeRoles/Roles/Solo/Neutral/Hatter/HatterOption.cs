using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Neutral.Hatter.HatterRole;

namespace ExtremeRoles.Roles.Solo.Neutral.Hatter
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
                loader.GetValue<Option, bool>(
                    Option.CanRepairSabotage),
                loader.GetValue<Option, int>(
                    Option.WinCount),
                loader.GetValue<Option, int>(
                    Option.MeetingTimerDecreaseLower),
                loader.GetValue<Option, int>(
                    Option.MeetingTimerDecreaseUpper),
                loader.GetValue<Option, bool>(
                    Option.HideMeetingTimer),
                loader.GetValue<Option, int>(
                    Option.IncreaseTaskGage),
                loader.GetValue<Option, int>(
                    Option.IncreseNum),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class HatterOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateBoolOption(
               Option.CanRepairSabotage,
               false);

            factory.CreateIntOption(
               Option.WinCount,
               3, 1, 10, 1);

            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 10, minAbilityCount: 0);

            factory.CreateBoolOption(
                Option.HideMeetingTimer, true);

            var lowerOpt = factory.CreateIntDynamicOption(
                Option.MeetingTimerDecreaseLower,
                0, 0, 5,
                format: OptionUnit.Percentage);

            var upperOpt = factory.CreateIntOption(
                Option.MeetingTimerDecreaseUpper,
                20, 0, 50, 5,
                format: OptionUnit.Percentage);
            upperOpt.AddWithUpdate(lowerOpt);

            factory.CreateIntOption(
                Option.IncreaseTaskGage,
                50, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                Option.IncreseNum,
                3, 1, 10, 1);
        }
    }
}
