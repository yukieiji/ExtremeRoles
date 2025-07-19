using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Photographer
{
    public readonly record struct PhotographerSpecificOption(
        int AwakeTaskGage,
        int UpgradePhotoTaskGage,
        bool EnableAllSendChat,
        int UpgradeAllSendChatTaskGage,
        float PhotoRange,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class PhotographerOptionLoader : ISpecificOptionLoader<PhotographerSpecificOption>
    {
        public PhotographerSpecificOption Load(IOptionLoader loader)
        {
            return new PhotographerSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradePhotoTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.EnableAllSendChat),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradeAllSendChatTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.PhotoRange),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class PhotographerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.AwakeTaskGage,
                30, 0, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradePhotoTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);

            var chatUpgradeOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.EnableAllSendChat,
                false);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradeAllSendChatTaskGage,
                80, 0, 100, 10,
                chatUpgradeOpt,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.PhotoRange,
                10.0f, 2.5f, 50f, 0.5f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 10);
        }
    }
}
