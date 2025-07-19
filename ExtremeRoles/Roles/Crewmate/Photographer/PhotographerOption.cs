using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Photographer
{
    public class PhotographerSpecificOption : IRoleSpecificOption
    {
        public int AwakeTaskGage { get; set; }
        public int UpgradePhotoTaskGage { get; set; }
        public bool EnableAllSendChat { get; set; }
        public int UpgradeAllSendChatTaskGage { get; set; }
        public float PhotoRange { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class PhotographerOptionLoader : ISpecificOptionLoader<PhotographerSpecificOption>
    {
        public PhotographerSpecificOption Load(IOptionLoader loader)
        {
            return new PhotographerSpecificOption
            {
                AwakeTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.AwakeTaskGage),
                UpgradePhotoTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradePhotoTaskGage),
                EnableAllSendChat = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.EnableAllSendChat),
                UpgradeAllSendChatTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.UpgradeAllSendChatTaskGage),
                PhotoRange = loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Photographer.PhotographerOption.PhotoRange),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
