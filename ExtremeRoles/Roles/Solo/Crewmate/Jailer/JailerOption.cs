using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Jailer.JailerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Jailer
{
    public readonly record struct JailerSpecificOption(
        int AwakeTaskGage,
        int AwakeDeadPlayerNum,
        bool UseAdmin,
        bool UseSecurity,
        bool UseVital,
        float Range,
        int TargetMode,
        bool CanReplaceAssassin,
        bool IsMissingToDead,
        bool IsDeadAbilityZero,
        bool LawbreakerCanKill,
        bool LawbreakerUseVent,
        bool LawbreakerUseSab,
        int YardbirdAddCommonTask,
        int YardbirdAddNormalTask,
        int YardbirdAddLongTask,
        float YardbirdSpeedMod,
        bool YardbirdUseAdmin,
        bool YardbirdUseSecurity,
        bool YardbirdUseVital,
        bool YardbirdUseVent,
        bool YardbirdUseSab,
        int AbilityUseCount,
        bool HasOtherKillCool,
        float KillCoolDown,
        bool HasOtherKillRange,
        int KillRange
    ) : IRoleSpecificOption;

    public class JailerOptionLoader : ISpecificOptionLoader<JailerSpecificOption>
    {
        public JailerSpecificOption Load(IOptionLoader loader)
        {
            return new JailerSpecificOption(
                loader.GetValue<Option, int>(
                    Option.AwakeTaskGage),
                loader.GetValue<Option, int>(
                    Option.AwakeDeadPlayerNum),
                loader.GetValue<Option, bool>(
                    Option.UseAdmin),
                loader.GetValue<Option, bool>(
                    Option.UseSecurity),
                loader.GetValue<Option, bool>(
                    Option.UseVital),
                loader.GetValue<Option, float>(
                    Option.Range),
                loader.GetValue<Option, int>(
                    Option.TargetMode),
                loader.GetValue<Option, bool>(
                    Option.CanReplaceAssassin),
                loader.GetValue<Option, bool>(
                    Option.IsMissingToDead),
                loader.GetValue<Option, bool>(
                    Option.IsDeadAbilityZero),
                loader.GetValue<Option, bool>(
                    Option.LawbreakerCanKill),
                loader.GetValue<Option, bool>(
                    Option.LawbreakerUseVent),
                loader.GetValue<Option, bool>(
                    Option.LawbreakerUseSab),
                loader.GetValue<Option, int>(
                    Option.YardbirdAddCommonTask),
                loader.GetValue<Option, int>(
                    Option.YardbirdAddNormalTask),
                loader.GetValue<Option, int>(
                    Option.YardbirdAddLongTask),
                loader.GetValue<Option, float>(
                    Option.YardbirdSpeedMod),
                loader.GetValue<Option, bool>(
                    Option.YardbirdUseAdmin),
                loader.GetValue<Option, bool>(
                    Option.YardbirdUseSecurity),
                loader.GetValue<Option, bool>(
                    Option.YardbirdUseVital),
                loader.GetValue<Option, bool>(
                    Option.YardbirdUseVent),
                loader.GetValue<Option, bool>(
                    Option.YardbirdUseSab),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillCool),
                loader.GetValue<KillerCommonOption, float>(KillerCommonOption.KillCoolDown),
                loader.GetValue<KillerCommonOption, bool>(KillerCommonOption.HasOtherKillRange),
                loader.GetValue<KillerCommonOption, int>(KillerCommonOption.KillRange)
            );
        }
    }

    public class JailerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                Option.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                Option.AwakeDeadPlayerNum,
                7, 0, 12, 1);

            factory.CreateBoolOption(
                Option.UseAdmin, false);
            factory.CreateBoolOption(
                Option.UseSecurity, true);
            factory.CreateBoolOption(
                Option.UseVital, false);

            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 5);

            factory.CreateSelectionOption<Option, TargetMode>(
                Option.TargetMode);
            factory.CreateBoolOption(
                Option.CanReplaceAssassin,
                true);

            factory.CreateFloatOption(
                Option.Range,
                0.75f, 0.1f, 1.5f, 0.1f);

            factory.CreateBoolOption(
                Option.IsDeadAbilityZero,
                true);

            var lowBreakerOpt = factory.CreateBoolOption(
                Option.IsMissingToDead, false);

            var lowBreakerKillOpt = factory.CreateBoolOption(
               Option.LawbreakerCanKill,
               true, lowBreakerOpt,
               invert: true);

            var killCoolOption = factory.CreateBoolOption(
                KillerCommonOption.HasOtherKillCool,
                false, lowBreakerKillOpt,
                invert: true);
            factory.CreateFloatOption(
                KillerCommonOption.KillCoolDown,
                30f, 1.0f, 120f, 0.5f,
                killCoolOption, format: OptionUnit.Second);

            var killRangeOption = factory.CreateBoolOption(
                KillerCommonOption.HasOtherKillRange,
                false, lowBreakerKillOpt,
                invert: true);
            factory.CreateSelectionOption(
                KillerCommonOption.KillRange,
                ExtremeRoles.Module.OptionCreator.Range,
                killRangeOption);

            factory.CreateBoolOption(
               Option.LawbreakerUseVent,
               true, lowBreakerOpt,
               invert: true);
            factory.CreateBoolOption(
               Option.LawbreakerUseSab,
               true, lowBreakerOpt,
               invert: true);


            factory.CreateIntOption(
                Option.YardbirdAddCommonTask,
                2, 0, 15, 1);
            factory.CreateIntOption(
                Option.YardbirdAddNormalTask,
                1, 0, 15, 1);
            factory.CreateIntOption(
                Option.YardbirdAddLongTask,
                1, 0, 15, 1);
            factory.CreateFloatOption(
                Option.YardbirdSpeedMod,
                0.8f, 0.1f, 1.0f, 0.1f);

            factory.CreateBoolOption(
                Option.YardbirdUseAdmin, false);
            factory.CreateBoolOption(
                Option.YardbirdUseSecurity, false);
            factory.CreateBoolOption(
                Option.YardbirdUseVital, false);
            factory.CreateBoolOption(
                Option.YardbirdUseVent, true);
            factory.CreateBoolOption(
                Option.YardbirdUseSab, true);
        }
    }
}
