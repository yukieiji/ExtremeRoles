using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Whisper
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityOffTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityOnTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.TellTextTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.MaxTellText),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.EnableAwakeAbility),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityTaskGage)
            );
        }
    }

    public class WhisperOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityOffTime,
                2.0f, 1.0f, 5.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityOnTime,
                4.0f, 1.0f, 10.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.TellTextTime,
                3.0f, 1.0f, 25.0f, 0.5f,
                format: OptionUnit.Second);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.MaxTellText,
                3, 1, 10, 1);

            var awakeOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.EnableAwakeAbility,
                false);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Whisper.WhisperOption.AbilityTaskGage,
                70, 0, 100, 10,
                awakeOpt,
                format: OptionUnit.Percentage);
        }
    }
}
