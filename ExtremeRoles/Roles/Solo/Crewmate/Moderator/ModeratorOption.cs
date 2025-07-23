using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Moderator
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption.MeetingTimerOffset),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class ModeratorOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption.AwakeTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            IRoleAbility.CreateAbilityCountOption(
                factory, 2, 10);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Moderator.ModeratorOption.MeetingTimerOffset,
                30, 5, 360, 5, format: OptionUnit.Second);
        }
    }
}
