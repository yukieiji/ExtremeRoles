using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Whisper
{
    public readonly record struct WhisperSpecificOption(
        float AbilityOffTime,
        float AbilityOnTime,
        float TellTextTime,
        int MaxTellText,
        bool EnableAwakeAbility,
        int AbilityTaskGage
    ) : IRoleSpecificOption;

    public class WhisperOptionLoader : ISpecificOptionLoader<WhisperSpecificOption>
    {
        public WhisperSpecificOption Load(IOptionLoader loader)
        {
            return new WhisperSpecificOption(
                loader.GetValue<WhisperOption, float>(
                    WhisperOption.AbilityOffTime),
                loader.GetValue<WhisperOption, float>(
                    WhisperOption.AbilityOnTime),
                loader.GetValue<WhisperOption, float>(
                    WhisperOption.TellTextTime),
                loader.GetValue<WhisperOption, int>(
                    WhisperOption.MaxTellText),
                loader.GetValue<WhisperOption, bool>(
                    WhisperOption.EnableAwakeAbility),
                loader.GetValue<WhisperOption, int>(
                    WhisperOption.AbilityTaskGage)
            );
        }
    }

    public class WhisperOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                WhisperOption.AbilityOffTime,
                2.0f, 1.0f, 5.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                WhisperOption.AbilityOnTime,
                4.0f, 1.0f, 10.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                WhisperOption.TellTextTime,
                3.0f, 1.0f, 25.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateIntOption(
                WhisperOption.MaxTellText,
                3, 1, 10, 1);

            var awakeOpt = factory.CreateBoolOption(
                WhisperOption.EnableAwakeAbility,
                false);
            factory.CreateIntOption(
                WhisperOption.AbilityTaskGage,
                70, 0, 100, 10,
                awakeOpt,
                format: OptionUnit.Percentage);
        }
    }
}
