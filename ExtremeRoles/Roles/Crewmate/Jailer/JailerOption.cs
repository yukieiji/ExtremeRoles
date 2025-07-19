using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Crewmate.Jailer
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
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.AwakeTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.AwakeDeadPlayerNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseAdmin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseSecurity),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseVital),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.Range),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.TargetMode),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.CanReplaceAssassin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.IsMissingToDead),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.IsDeadAbilityZero),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerCanKill),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerUseVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerUseSab),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddCommonTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddNormalTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, int>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddLongTask),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, float>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdSpeedMod),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseAdmin),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseSecurity),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseVital),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseVent),
                loader.GetValue<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, bool>(
                    ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseSab),
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
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.AwakeTaskGage,
                70, 0, 100, 10,
                format: OptionUnit.Percentage);

            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.AwakeDeadPlayerNum,
                7, 0, 12, 1);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseAdmin, false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseSecurity, true);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.UseVital, false);

            IRoleAbility.CreateAbilityCountOption(
                factory, 1, 5);

            factory.CreateSelectionOption<ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option, ExtremeRoles.Roles.Solo.Crewmate.Jailer.TargetMode>(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.TargetMode);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.CanReplaceAssassin,
                true);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.Range,
                0.75f, 0.1f, 1.5f, 0.1f);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.IsDeadAbilityZero,
                true);

            var lowBreakerOpt = factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.IsMissingToDead, false);

            var lowBreakerKillOpt = factory.CreateBoolOption(
               ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerCanKill,
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
               ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerUseVent,
               true, lowBreakerOpt,
               invert: true);
            factory.CreateBoolOption(
               ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.LawbreakerUseSab,
               true, lowBreakerOpt,
               invert: true);


            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddCommonTask,
                2, 0, 15, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddNormalTask,
                1, 0, 15, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdAddLongTask,
                1, 0, 15, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdSpeedMod,
                0.8f, 0.1f, 1.0f, 0.1f);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseAdmin, false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseSecurity, false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseVital, false);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseVent, true);
            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Crewmate.Jailer.Option.YardbirdUseSab, true);
        }
    }
}
