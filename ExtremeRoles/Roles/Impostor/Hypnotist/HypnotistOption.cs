using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;

namespace ExtremeRoles.Roles.Impostor.Hypnotist
{
    public class HypnotistSpecificOption : IRoleSpecificOption
    {
        public int AwakeCheckImpostorNum { get; set; }
        public int AwakeCheckTaskGage { get; set; }
        public int AwakeKillCount { get; set; }
        public float Range { get; set; }
        public float HideArrowRange { get; set; }
        public int DefaultRedAbilityPart { get; set; }
        public float HideKillButtonTime { get; set; }
        public int DollKillCoolReduceRate { get; set; }
        public bool IsResetKillCoolWhenDollKill { get; set; }
        public float DollCrakingCoolTime { get; set; }
        public float DollCrakingActiveTime { get; set; }
        public int AbilityUseCount { get; set; }
    }

    public class HypnotistOptionLoader : ISpecificOptionLoader<HypnotistSpecificOption>
    {
        public HypnotistSpecificOption Load(IOptionLoader loader)
        {
            return new HypnotistSpecificOption
            {
                AwakeCheckImpostorNum = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckImpostorNum),
                AwakeCheckTaskGage = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeCheckTaskGage),
                AwakeKillCount = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.AwakeKillCount),
                Range = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.Range),
                HideArrowRange = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideArrowRange),
                DefaultRedAbilityPart = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DefaultRedAbilityPart),
                HideKillButtonTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.HideKillButtonTime),
                DollKillCoolReduceRate = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, int>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollKillCoolReduceRate),
                IsResetKillCoolWhenDollKill = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, bool>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.IsResetKillCoolWhenDollKill),
                DollCrakingCoolTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingCoolTime),
                DollCrakingActiveTime = loader.GetValue<ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption, float>(
                    ExtremeRoles.Roles.Solo.Impostor.Hypnotist.HypnotistOption.DollCrakingActiveTime),
                AbilityUseCount = loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount)
            };
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
