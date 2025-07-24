using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Moderator
{
    public readonly record struct ModeratorSpecificOption(
        int AwakeTaskGage,
        int MeetingTimerOffset,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class ModeratorOptionLoader : ISpecificOptionLoader<ModeratorSpecificOption>
    {
        public ModeratorSpecificOption Load(IOptionLoader loader)
        {
            return new ModeratorSpecificOption(
                loader.GetValue<ModeratorOption, int>(
                    ModeratorOption.AwakeTaskGage),
                loader.GetValue<ModeratorOption, int>(
                    ModeratorOption.MeetingTimerOffset),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class ModeratorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ModeratorOption.AwakeTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 10);
            factory.CreateIntOption(
                ModeratorOption.MeetingTimerOffset,
                30, 5, 360, 5, format: OptionUnit.Second);
        }
    }
}
