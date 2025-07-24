using ExtremeRoles.Module.CustomOption.Factory;
using ExtremeRoles.Module.CustomOption.Interfaces;
using ExtremeRoles.Roles.API;
using ExtremeRoles.Roles.API.Interface;
using ExtremeRoles.Module.CustomOption.Enums;
using static ExtremeRoles.Roles.Solo.Crewmate.Delusioner.DelusionerRole;

namespace ExtremeRoles.Roles.Solo.Crewmate.Delusioner
{
    public readonly record struct DelusionerSpecificOption(
        int AwakeVoteNum,
        bool IsOnetimeAwake,
        float Range,
        int VoteCoolTimeReduceRate,
        int DeflectDamagePenaltyRate,
        bool IsIncludeLocalPlayer,
        bool IsIncludeSpawnPoint,
        bool EnableCounter,
        int AbilityUseCount,
        float AbilityCoolTime
    ) : IRoleSpecificOption;

    public class DelusionerOptionLoader : ISpecificOptionLoader<DelusionerSpecificOption>
    {
        public DelusionerSpecificOption Load(IOptionLoader loader)
        {
            return new DelusionerSpecificOption(
                loader.GetValue<DelusionerOption, int>(
                    DelusionerOption.AwakeVoteNum),
                loader.GetValue<DelusionerOption, bool>(
                    DelusionerOption.IsOnetimeAwake),
                loader.GetValue<DelusionerOption, float>(
                    DelusionerOption.Range),
                loader.GetValue<DelusionerOption, int>(
                    DelusionerOption.VoteCoolTimeReduceRate),
                loader.GetValue<DelusionerOption, int>(
                    DelusionerOption.DeflectDamagePenaltyRate),
                loader.GetValue<DelusionerOption, bool>(
                    DelusionerOption.IsIncludeLocalPlayer),
                loader.GetValue<DelusionerOption, bool>(
                    DelusionerOption.IsIncludeSpawnPoint),
                loader.GetValue<DelusionerOption, bool>(
                    DelusionerOption.EnableCounter),
                loader.GetValue<RoleAbilityCountOption, int>(RoleAbilityCountOption.AbilityUseCount),
                loader.GetValue<RoleAbilityCommonOption, float>(RoleAbilityCommonOption.AbilityCoolTime)
            );
        }
    }

    public class DelusionerOptionFactory : IRoleOptionFactory
    {
        public void Build(AutoParentSetOptionCategoryFactory factory)
        {
            factory.CreateIntOption(
                DelusionerOption.AwakeVoteNum,
                3, 0, 8, 1,
                format: OptionUnit.VoteNum);
            factory.CreateBoolOption(
                DelusionerOption.IsOnetimeAwake,
                false);

            factory.CreateFloatOption(
                DelusionerOption.Range,
                2.5f, 0.0f, 7.5f, 0.1f);

            IRoleAbility.CreateAbilityCountOption(
                factory, 3, 25);

            factory.CreateIntOption(
                DelusionerOption.VoteCoolTimeReduceRate,
                5, 0, 100, 5,
                format: OptionUnit.Percentage);
            factory.CreateIntOption(
                DelusionerOption.DeflectDamagePenaltyRate,
                10, 0, 100, 5,
                format: OptionUnit.Percentage);

            factory.CreateBoolOption(
                DelusionerOption.IsIncludeLocalPlayer,
                true);
            factory.CreateBoolOption(
                DelusionerOption.IsIncludeSpawnPoint,
                false);

            factory.CreateBoolOption(
                DelusionerOption.EnableCounter,
                false);
        }
    }
}
