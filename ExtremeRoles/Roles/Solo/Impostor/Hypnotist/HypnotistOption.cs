using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistRole;

namespace ExtremeRoles.Roles.Solo.Impostor.Hypnotist
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
                loader.GetValue<HypnotistOption, int>(
                    HypnotistOption.AwakeCheckImpostorNum),
                loader.GetValue<HypnotistOption, int>(
                    HypnotistOption.AwakeCheckTaskGage),
                loader.GetValue<HypnotistOption, int>(
                    HypnotistOption.AwakeKillCount),
                loader.GetValue<HypnotistOption, float>(
                    HypnotistOption.Range),
                loader.GetValue<HypnotistOption, float>(
                    HypnotistOption.HideArrowRange),
                loader.GetValue<HypnotistOption, int>(
                    HypnotistOption.DefaultRedAbilityPart),
                loader.GetValue<HypnotistOption, float>(
                    HypnotistOption.HideKillButtonTime),
                loader.GetValue<HypnotistOption, int>(
                    HypnotistOption.DollKillCoolReduceRate),
                loader.GetValue<HypnotistOption, bool>(
                    HypnotistOption.IsResetKillCoolWhenDollKill),
                loader.GetValue<HypnotistOption, float>(
                    HypnotistOption.DollCrakingCoolTime),
                loader.GetValue<HypnotistOption, float>(
                    HypnotistOption.DollCrakingActiveTime),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            );
        }
    }

    public class HypnotistOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                HypnotistOption.AwakeCheckImpostorNum,
                1, 1, GameSystem.MaxImposterNum, 1);
            factory.CreateIntOption(
                HypnotistOption.AwakeCheckTaskGage,
                60, 0, 100, 10,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                HypnotistOption.AwakeKillCount,
                2, 0, 5, 1,
                format: OptionUnit.Shot);

            factory.CreateFloatOption(
                HypnotistOption.Range,
                1.6f, 0.5f, 5.0f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(factory, 1, 5);

            factory.CreateFloatOption(
                HypnotistOption.HideArrowRange,
                10.0f, 5.0f, 25.0f, 0.5f);
            factory.CreateIntOption(
                HypnotistOption.DefaultRedAbilityPart,
                0, 0, 10, 1);
            factory.CreateFloatOption(
                HypnotistOption.HideKillButtonTime,
                15.0f, 2.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateIntOption(
                HypnotistOption.DollKillCoolReduceRate,
                10, 0, 75, 1,
                format: OptionUnit.Percentage);

            factory.CreateBoolOption(
                HypnotistOption.IsResetKillCoolWhenDollKill,
                true);
            factory.CreateFloatOption(
                HypnotistOption.DollCrakingCoolTime,
                30.0f, 0.5f, 120.0f, 0.5f,
                format: OptionUnit.Second);
            factory.CreateFloatOption(
                HypnotistOption.DollCrakingActiveTime,
                3.0f, 0.5f, 60.0f, 0.5f,
                format: OptionUnit.Second);
        }
    }
}
