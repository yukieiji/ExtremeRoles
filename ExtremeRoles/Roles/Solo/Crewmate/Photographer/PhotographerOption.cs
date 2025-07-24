using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Photographer
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
                loader.GetValue<PhotographerOption, int>(
                    PhotographerOption.AwakeTaskGage),
                loader.GetValue<PhotographerOption, int>(
                    PhotographerOption.UpgradePhotoTaskGage),
                loader.GetValue<PhotographerOption, bool>(
                    PhotographerOption.EnableAllSendChat),
                loader.GetValue<PhotographerOption, int>(
                    PhotographerOption.UpgradeAllSendChatTaskGage),
                loader.GetValue<PhotographerOption, float>(
                    PhotographerOption.PhotoRange),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class PhotographerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                PhotographerOption.AwakeTaskGage,
                30, 0, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                PhotographerOption.UpgradePhotoTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);

            var chatUpgradeOpt = factory.CreateBoolOption(
                PhotographerOption.EnableAllSendChat,
                false);

            factory.CreateIntOption(
                PhotographerOption.UpgradeAllSendChatTaskGage,
                80, 0, 100, 10,
                chatUpgradeOpt,
                format: OptionUnit.Percentage);

            factory.CreateFloatOption(
                PhotographerOption.PhotoRange,
                10.0f, 2.5f, 50f, 0.5f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 5, 10);
        }
    }
}
