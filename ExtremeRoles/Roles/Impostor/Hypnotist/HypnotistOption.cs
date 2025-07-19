using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Hypnotist
{
    public readonly record struct HypnotistSpecificOption(
        int AwakeCheckImpostorNum,
        int AwakeCheckTaskGage,
        int AwakeKillCount,
        float Range,
        float HideArrowRange,
        int DefaultRedAbilityPart,
        float HideKillButtonTime,
        int DollKillCoolReduceRate,
        bool IsResetKillCoolWhenDollKill,
        float DollCrakingCoolTime,
        float DollCrakingActiveTime,
        int AbilityUseCount
    ) : IRoleSpecificOption;

    public class HypnotistOptionLoader : ISpecificOptionLoader<HypnotistSpecificOption>
    {
        public HypnotistSpecificOption Load(IOptionLoader loader)
        {
            return new HypnotistSpecificOption(
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckImpostorNum),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckTaskGage),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeKillCount),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.Range),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideArrowRange),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DefaultRedAbilityPart),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideKillButtonTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollKillCoolReduceRate),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.IsResetKillCoolWhenDollKill),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingCoolTime),
                loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingActiveTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class HypnotistOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeKillCount,
                2, 0, 5, 1,
                format: OptionUnit.Shot);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.Range,
                1.6f, 0.5f, 5.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(factory, 1, 5);

            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideArrowRange,
                10.0f, 5.0f, 25.0f, 0.5f);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DefaultRedAbilityPart,
                0, 0, 10, 1);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideKillButtonTime,
                15.0f, 2.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollKillCoolReduceRate,
                10, 0, 75, 1,
                format: OptionUnit.Percentage);

            factory.CreateBoolOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.IsResetKillCoolWhenDollKill,
                true);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingCoolTime,
                30.0f, 0.5f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingActiveTime,
                3.0f, 0.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
